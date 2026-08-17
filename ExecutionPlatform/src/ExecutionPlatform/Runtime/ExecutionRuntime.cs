using System;
using System.Collections.Generic;
using BepInEx.Logging;
using ExecutionPlatform.Core;
using UnityEngine;

namespace ExecutionPlatform.Runtime
{
    internal static class ExecutionRuntime
    {
        private static readonly Dictionary<T_Citizen, ExecutionStateMachine> States =
            new Dictionary<T_Citizen, ExecutionStateMachine>();

        private static ManualLogSource _logger;
        private static DB_Mgr _registeredDatabase;
        private static bool _catalogReady;
        private static bool _failed;

        internal static bool IsEnabled
        {
            get
            {
                if (!_catalogReady || _failed)
                {
                    return false;
                }

                if (ExecutionCatalog.IsOwnedRegistration(_registeredDatabase))
                {
                    return true;
                }

                FailClosed($"建筑值或数据库索引 {ExecutionCatalog.RuntimeBuildingValue} 的所有权发生冲突，处刑台已停用。");
                return false;
            }
        }

        internal static void Configure(ManualLogSource logger)
        {
            _logger = logger;
            _registeredDatabase = null;
            _catalogReady = false;
            _failed = false;
            States.Clear();
        }

        internal static void RegisterDatabase(DB_Mgr database)
        {
            if (_failed)
            {
                return;
            }

            try
            {
                if (ReferenceEquals(_registeredDatabase, database) &&
                    database?.Dic_BuildDB != null &&
                    database.Dic_BuildDB.TryGetValue(ExecutionCatalog.RuntimeBuildingName, out var existing) &&
                    existing?.Key == "ExecutionPlatform")
                {
                    _catalogReady = true;
                    return;
                }

                if (!ExecutionCatalog.TryRegister(database, out var failure))
                {
                    FailClosed($"处刑台建筑注册失败：{failure}");
                    return;
                }

                _registeredDatabase = database;
                _catalogReady = true;
                _logger?.LogInfo($"已注册处刑台建筑，建筑值和数据库索引均为 {ExecutionCatalog.RuntimeBuildingValue}。");
            }
            catch (Exception exception)
            {
                FailClosed("处刑台建筑注册时发生异常。", exception);
            }
        }

        internal static void OnJobChanging(T_Citizen citizen)
        {
            Cancel(citizen, restoreAnimation: true);
        }

        internal static void OnJobSet(T_Citizen citizen, Building building)
        {
            if (!IsEnabled || citizen == null || !ExecutionCatalog.IsExecution(building))
            {
                Cancel(citizen, restoreAnimation: true);
                return;
            }

            try
            {
                if (PlayDataMgr.Instance?.IsLoadGame == true)
                {
                    return;
                }

                if (!IsEligibleCitizen(citizen))
                {
                    _logger?.LogWarning($"已拒绝鼠民 {citizen.m_ID} 的处刑台岗位：该单位当前不是有效处刑目标。");
                    citizen.JobFire(false);
                    return;
                }

                if (!IsValid(citizen, building))
                {
                    return;
                }

                citizen.ForJob_WakeUp();
                citizen.BehaviorStop();
                var machine = new ExecutionStateMachine();
                States[citizen] = machine;
                Apply(citizen, building, machine.Assign(building.m_ID, Time.time), wasCounting: false);
            }
            catch (Exception exception)
            {
                _logger?.LogError($"为鼠民 {citizen.m_ID} 准备处刑任务失败：{exception}");
                Cancel(citizen, restoreAnimation: true);
            }
        }

        internal static bool TryHandleUpdate(T_Citizen citizen)
        {
            if (!IsEnabled || citizen == null)
            {
                return false;
            }

            var building = citizen.m_Job;
            if (!ExecutionCatalog.IsExecution(building))
            {
                Cancel(citizen, restoreAnimation: true);
                return false;
            }

            try
            {
                citizen.DrownCheck();
                citizen.InjuryCheck();

                if (!IsValid(citizen, building))
                {
                    Cancel(citizen, restoreAnimation: true);
                    return false;
                }

                if (!States.TryGetValue(citizen, out var machine))
                {
                    OnJobSet(citizen, building);
                    return States.ContainsKey(citizen);
                }

                var wasCounting = machine.Phase == ExecutionPhase.Counting;
                var atWorkPosition = IsAtWorkPosition(citizen, building);
                var action = machine.Tick(Time.time, isValid: true, atWorkPosition);
                Apply(citizen, building, action, wasCounting);
                return true;
            }
            catch (Exception exception)
            {
                _logger?.LogError($"更新鼠民 {citizen.m_ID} 的处刑任务失败，已取消本次任务：{exception}");
                Cancel(citizen, restoreAnimation: true);
                return false;
            }
        }

        internal static void ClearTransient(string reason)
        {
            if (States.Count == 0)
            {
                return;
            }

            var citizens = new List<T_Citizen>(States.Keys);
            States.Clear();
            foreach (var citizen in citizens)
            {
                try
                {
                    StopWorkVisuals(citizen, citizen == null ? null : citizen.m_Job, restoreAnimation: true);
                }
                catch (Exception exception)
                {
                    _logger?.LogWarning($"清理处刑临时状态时发生异常：{exception}");
                }
            }

            _logger?.LogInfo($"已清空处刑临时状态：{reason}。倒计时将在有效岗位上重新开始。");
        }

        internal static void FailClosed(string reason, Exception exception = null)
        {
            if (_failed)
            {
                return;
            }

            _failed = true;
            _catalogReady = false;
            ClearTransient("功能失败关闭");
            if (exception == null)
            {
                _logger?.LogError(reason);
            }
            else
            {
                _logger?.LogError($"{reason}\n{exception}");
            }
        }

        internal static void Shutdown()
        {
            ClearTransient("插件卸载");
            _catalogReady = false;
            _failed = true;
            _registeredDatabase = null;
        }

        private static bool IsValid(T_Citizen citizen, Building building)
        {
            return citizen != null &&
                   building != null &&
                   citizen.m_Job == building &&
                   ExecutionCatalog.IsExecution(building) &&
                   PlayDataMgr.Instance != null &&
                   !PlayDataMgr.Instance.IsLoadGame &&
                   IsEligibleCitizen(citizen) &&
                   building.gameObject.activeSelf &&
                   building.m_BuildState == BuildState.Basic &&
                   building.m_Activation &&
                   !building.m_Demolition;
        }

        private static bool IsEligibleCitizen(T_Citizen citizen)
        {
            return citizen != null &&
                   citizen.Obj != null &&
                   citizen.Obj.activeSelf &&
                   citizen.m_UnitKind == UnitKind.Citizen &&
                   citizen.m_CurHP > 0f &&
                   !citizen.IsNotNormalState() &&
                   !citizen.m_ImprisonCheck &&
                   !citizen.List_State.Contains(CitizenState.FallDown) &&
                   !citizen.List_State.Contains(CitizenState.Fear);
        }

        private static bool IsAtWorkPosition(T_Citizen citizen, Building building)
        {
            if (citizen.m_CurNode == null)
            {
                citizen.NodeUpdate();
            }

            return citizen.m_CurNode != null && building.IsInArea(1, citizen.m_CurNode.GetIntPos());
        }

        private static void Apply(
            T_Citizen citizen,
            Building building,
            ExecutionAction action,
            bool wasCounting)
        {
            if (action == ExecutionAction.None)
            {
                return;
            }

            if (action.HasFlag(ExecutionAction.RequestPath))
            {
                if (wasCounting)
                {
                    StopWorkVisuals(citizen, building, restoreAnimation: true);
                }

                citizen.PathFindCall(
                    building.Pos_Tile,
                    CitizenState.Nothing,
                    C_Key.JobReCompass,
                    _IsReverse: false);
            }

            if (action.HasFlag(ExecutionAction.StartCounting))
            {
                StartWorkVisuals(citizen, building);
            }

            if (action.HasFlag(ExecutionAction.Cancel))
            {
                Cancel(citizen, restoreAnimation: true);
            }

            if (action.HasFlag(ExecutionAction.Execute))
            {
                Execute(citizen, building);
            }
        }

        private static void StartWorkVisuals(T_Citizen citizen, Building building)
        {
            citizen.KillMoving(_fall_check: false);
            citizen.List_State.Clear();
            citizen.List_State.Add(CitizenState.Working);
            if (building.m_Body?.m_Animator != null)
            {
                building.m_Body.m_Animator.Play("Action");
            }

            citizen.SetZ_Order(back: true);
            citizen.FlipX(right: false);
            citizen.SetAniState(
                AniState.Idle,
                "Prison_Idle",
                _loop: true,
                _now: false);
        }

        private static void StopWorkVisuals(
            T_Citizen citizen,
            Building building,
            bool restoreAnimation)
        {
            if (building != null)
            {
                building.BuildingStopAni();
            }

            if (citizen == null)
            {
                return;
            }

            citizen.List_State.Remove(CitizenState.Working);
            citizen.SetZ_Order(back: false);
            if (restoreAnimation &&
                citizen.m_CurHP > 0f &&
                !citizen.IsNotNormalState() &&
                !citizen.m_ImprisonCheck &&
                !citizen.List_State.Contains(CitizenState.FallDown) &&
                !citizen.List_State.Contains(CitizenState.Fear))
            {
                citizen.SetAniState(AniState.Idle, _loop: true, _now: false);
            }
        }

        private static void Cancel(T_Citizen citizen, bool restoreAnimation)
        {
            if (citizen == null || !States.TryGetValue(citizen, out var machine))
            {
                return;
            }

            States.Remove(citizen);
            machine.Cancel();
            StopWorkVisuals(citizen, citizen.m_Job, restoreAnimation);
        }

        private static void Execute(T_Citizen citizen, Building building)
        {
            States.Remove(citizen);
            StopWorkVisuals(citizen, building, restoreAnimation: false);
            citizen.HpUpdate(-citizen.m_CurHP);
            citizen.DeathCheck();
        }
    }
}
