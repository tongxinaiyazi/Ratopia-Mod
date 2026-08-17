using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using SuperBow.Core;
using UnityEngine;

namespace SuperBow.Runtime
{
    internal static class CombatRuntime
    {
        private const float ValueTolerance = 0.0001f;
        private static readonly BleedTracker<RuntimeCombatTarget> Bleed =
            new BleedTracker<RuntimeCombatTarget>();
        private static ManualLogSource _logger;
        private static bool _enabled;
        private static bool _hitEffectsEnabled;
        private static bool _bleedTicksEnabled;
        private static bool _firstArrowInvocationLogged;
        private static readonly HashSet<CombatTargetKind> LoggedTargetKinds =
            new HashSet<CombatTargetKind>();
        private static bool _firstTickInvocationLogged;
        private static bool _firstBleedAppliedLogged;
        private static bool _firstBleedTickLogged;
        private static bool _firstSplashDamageLogged;

        public static void Initialize(ManualLogSource logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _enabled = true;
            _hitEffectsEnabled = true;
            _bleedTicksEnabled = true;
            LoggedTargetKinds.Clear();
            _firstArrowInvocationLogged = false;
            _firstTickInvocationLogged = false;
            _firstBleedAppliedLogged = false;
            _firstBleedTickLogged = false;
            _firstSplashDamageLogged = false;
        }

        public static bool IsSupportedQueen(T_Queen queen)
        {
            if (!_enabled || queen == null)
            {
                return false;
            }

            var weapon = queen.m_WeaponInfo;
            return weapon != null && QueenBowIdentity.IsMatch(
                weapon.Index,
                weapon.m_Type,
                weapon.Name);
        }

        public static void ReportArrowPatchInvocation()
        {
            if (_firstArrowInvocationLogged)
            {
                return;
            }

            _firstArrowInvocationLogged = true;
            _logger?.LogInfo("弓箭碰撞补丁首次执行。");
        }

        public static void ReportSupportedHit(CombatTargetKind kind)
        {
            if (!LoggedTargetKinds.Add(kind))
            {
                return;
            }

            _logger?.LogInfo($"WoodBow 已确认原版有效命中：{kind}。");
        }

        public static void ProcessHitSafely(BowHitState state)
        {
            if (!_enabled || !_hitEffectsEnabled || state == null)
            {
                return;
            }

            try
            {
                ProcessHit(state);
            }
            catch (Exception exception)
            {
                _hitEffectsEnabled = false;
                _logger?.LogError($"处理超级弓箭命中失败，已停用后续命中附加效果：{exception}");
            }
        }

        public static void ReportCaptureFailure(Exception exception)
        {
            _hitEffectsEnabled = false;
            _logger?.LogError($"捕获箭矢命中失败，已停用后续命中附加效果：{exception}");
        }

        public static void TickSafely(float now)
        {
            if (!_enabled || !_bleedTicksEnabled || Bleed.Count == 0)
            {
                return;
            }

            if (!_firstTickInvocationLogged)
            {
                _firstTickInvocationLogged = true;
                _logger?.LogInfo("流血计时补丁首次执行。");
            }

            try
            {
                var ticks = Bleed.Advance(
                    now,
                    target => target != null && target.IsAlive,
                    target => target != null && target.IsBoss);
                foreach (var tick in ticks)
                {
                    var target = tick.Target;
                    if (target == null || !target.IsAlive)
                    {
                        continue;
                    }

                    var exactDamage = BleedDamageRules.CalculateExact(
                        target.MaxHealth,
                        tick.Fraction);
                    var damage = BleedDamageRules.CalculateApplied(
                        target.MaxHealth,
                        tick.Fraction);
                    if (damage > 0f)
                    {
                        using (DamageDisplayRuntime.Override(damage))
                        {
                            target.ApplyDamage(damage);
                        }
                        if (!_firstBleedTickLogged)
                        {
                            _firstBleedTickLogged = true;
                            _logger?.LogInfo(
                                $"流血首次结算：{target.Kind}，" +
                                $"百分比结果 {exactDamage:0.###}，" +
                                $"实际伤害/飘字 {damage}。");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Bleed.Clear();
                _bleedTicksEnabled = false;
                _logger?.LogError($"结算流血失败，已清理状态并停用流血计时：{exception}");
            }
        }

        public static void Clear(string reason = null)
        {
            Bleed.Clear();
            if (!string.IsNullOrEmpty(reason))
            {
                _logger?.LogDebug(reason);
            }
        }

        public static void Disable(string reason)
        {
            _enabled = false;
            _hitEffectsEnabled = false;
            _bleedTicksEnabled = false;
            Bleed.Clear();
            _logger?.LogError($"超级弓箭战斗运行时已停用：{reason}");
        }

        public static void Shutdown()
        {
            _enabled = false;
            _hitEffectsEnabled = false;
            _bleedTicksEnabled = false;
            Bleed.Clear();
            _logger = null;
        }

        private static void ProcessHit(BowHitState state)
        {
            if (!IsSupportedQueen(state.Queen) ||
                state.Target == null ||
                state.DirectDamage <= 0f)
            {
                return;
            }

            var hasBleed = _bleedTicksEnabled && HasAffix(
                state.Queen,
                2,
                (Res_Ability)SuperBowConstants.BloodDrainAbilityId,
                SuperBowConstants.BleedMarkerValue);
            if (hasBleed && state.Target.IsAlive)
            {
                Bleed.ApplyOrRefresh(state.Target, Time.time);
                if (!_firstBleedAppliedLogged)
                {
                    _firstBleedAppliedLogged = true;
                    _logger?.LogInfo($"流血首次施加：{state.Target.Kind}。");
                }
            }

            var hasSplash = HasAffix(
                state.Queen,
                1,
                (Res_Ability)SuperBowConstants.RangeAttackAbilityId,
                SuperBowConstants.RangeAttackValue);
            if (!hasSplash)
            {
                return;
            }

            var splashDamage = SplashRules.CalculateDamage(state.DirectDamage);

            foreach (var candidate in RuntimeCombatTarget.EnumerateSplashCandidates(
                         state.Queen))
            {
                if (candidate == null)
                {
                    continue;
                }

                if (!SplashRules.ShouldDamage(
                        candidate.Equals(state.Target),
                        true,
                        candidate.IsAlive,
                        state.CenterX,
                        state.CenterY,
                        candidate.CenterX,
                        candidate.CenterY))
                {
                    continue;
                }

                var healthBeforeSplash = candidate.CurrentHealth;
                candidate.ApplyDamage(splashDamage);
                if (!HitConfirmation.DidTakeDamage(
                        healthBeforeSplash,
                        candidate.CurrentHealth))
                {
                    continue;
                }

                if (!_firstSplashDamageLogged)
                {
                    _firstSplashDamageLogged = true;
                    _logger?.LogInfo(
                        $"范围伤害首次生效：{candidate.Kind}，伤害 {splashDamage:0.###}。");
                }

                if (hasBleed && candidate.IsAlive)
                {
                    Bleed.ApplyOrRefresh(candidate, Time.time);
                }
            }
        }

        private static bool HasAffix(
            T_Queen queen,
            int level,
            Res_Ability ability,
            float value)
        {
            var weapon = queen.m_WeaponInfo;
            if (weapon == null ||
                queen.Dic_ItemPlusEffect == null ||
                !queen.Dic_ItemPlusEffect.TryGetValue(weapon.Index, out var effects) ||
                effects == null)
            {
                return false;
            }

            return effects.Any(effect =>
                effect != null &&
                effect.Level == level &&
                effect.m_Ability.Equals(ability) &&
                Math.Abs(effect.m_AbilityValue - value) <= ValueTolerance);
        }

    }
}
