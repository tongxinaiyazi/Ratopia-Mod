using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SuperBow.Core;
using UnityEngine;

namespace SuperBow.Runtime
{
    internal sealed class RuntimeCombatTarget : IEquatable<RuntimeCombatTarget>
    {
        private readonly UnityEngine.Object _instance;
        private readonly T_Queen _queen;

        private RuntimeCombatTarget(
            CombatTargetKind kind,
            UnityEngine.Object instance,
            T_Queen queen)
        {
            Kind = kind;
            _instance = instance;
            _queen = queen;
        }

        public CombatTargetKind Kind { get; }

        public GameUnit GameUnit => _instance as GameUnit;

        public float CurrentHealth
        {
            get
            {
                switch (Kind)
                {
                    case CombatTargetKind.GameUnit:
                        var unit = _instance as GameUnit;
                        return unit != null ? unit.m_CurHP : 0f;
                    case CombatTargetKind.AnimalBody:
                        var animal = _instance as AnimalBody;
                        return animal != null ? animal.m_CurHP : 0f;
                    case CombatTargetKind.MapObject:
                        var mapObject = _instance as MapObj;
                        return mapObject != null ? mapObject.m_CurHp : 0f;
                    case CombatTargetKind.Building:
                        var building = _instance as Building;
                        return building != null ? building.m_CurHP : 0f;
                    default:
                        return 0f;
                }
            }
        }

        public float MaxHealth
        {
            get
            {
                switch (Kind)
                {
                    case CombatTargetKind.GameUnit:
                        var unit = _instance as GameUnit;
                        return unit != null ? unit.GetMaxHP() : 0f;
                    case CombatTargetKind.AnimalBody:
                        var animal = _instance as AnimalBody;
                        return animal != null ? animal.m_MaxHP : 0f;
                    case CombatTargetKind.MapObject:
                        var mapObject = _instance as MapObj;
                        return mapObject != null ? mapObject.m_MaxHp : 0f;
                    case CombatTargetKind.Building:
                        var building = _instance as Building;
                        return building != null ? building.m_MaxHP : 0f;
                    default:
                        return 0f;
                }
            }
        }

        public float CenterX => GetPosition().x;

        public float CenterY => GetPosition().y;

        public bool IsAlive
        {
            get
            {
                if (_instance == null || CurrentHealth <= 0f)
                {
                    return false;
                }

                var animal = _instance as AnimalBody;
                if (animal != null)
                {
                    return animal.m_State != AnimalState.Death;
                }

                var mapObject = _instance as MapObj;
                if (mapObject != null)
                {
                    return mapObject.gameObject.activeSelf;
                }

                return true;
            }
        }

        public bool IsBoss
        {
            get
            {
                var enemy = _instance as GameEnemy;
                return enemy != null &&
                       enemy.m_EnemyInfo != null &&
                       enemy.m_EnemyInfo.m_Category == EnemyCategory.Boss;
            }
        }

        public static IReadOnlyList<RuntimeCombatTarget> EnumerateSplashCandidates(
            T_Queen queen)
        {
            var result = new List<RuntimeCombatTarget>();
            if (queen == null)
            {
                return result;
            }

            var manager = GameMgr.Instance;
            if (manager == null)
            {
                return result;
            }

            if (manager._T_UnitMgr != null &&
                manager._T_UnitMgr.List_AllEnemy != null)
            {
                foreach (var enemy in new List<GameEnemy>(
                             manager._T_UnitMgr.List_AllEnemy))
                {
                    AddCandidate(
                        result,
                        new RuntimeCombatTarget(
                            CombatTargetKind.GameUnit,
                            enemy,
                            queen));
                }
            }

            if (manager._AnimalMgr != null &&
                manager._AnimalMgr.List_Animal != null)
            {
                foreach (var animal in new List<AnimalBody>(
                             manager._AnimalMgr.List_Animal))
                {
                    AddCandidate(
                        result,
                        new RuntimeCombatTarget(
                            CombatTargetKind.AnimalBody,
                            animal,
                            queen));
                }
            }

            if (manager._MapObjMgr != null &&
                manager._MapObjMgr.List_MapObj != null)
            {
                foreach (var mapObject in new List<MapObj>(
                             manager._MapObjMgr.List_MapObj))
                {
                    AddCandidate(
                        result,
                        new RuntimeCombatTarget(
                            CombatTargetKind.MapObject,
                            mapObject,
                            queen));
                }
            }

            if (manager._BuildingMgr != null &&
                manager._BuildingMgr.List_Building != null)
            {
                foreach (var building in new List<Building>(
                             manager._BuildingMgr.List_Building))
                {
                    AddCandidate(
                        result,
                        new RuntimeCombatTarget(
                            CombatTargetKind.Building,
                            building,
                            queen));
                }
            }

            return result;
        }

        public static bool TryFromCollision(
            Collider2D collider,
            T_Queen queen,
            out RuntimeCombatTarget target)
        {
            target = null;
            if (collider == null || queen == null)
            {
                return false;
            }

            var unit = Helpers.GetGameUnitByCollision(collider);
            if (unit != null && Helpers.IsTeamEnemy(unit))
            {
                target = new RuntimeCombatTarget(
                    CombatTargetKind.GameUnit,
                    unit,
                    queen);
                return true;
            }

            var animal = collider.GetComponent<AnimalBody>();
            if (animal != null &&
                animal.m_State != AnimalState.Death &&
                animal.m_State != AnimalState.Stun)
            {
                target = new RuntimeCombatTarget(
                    CombatTargetKind.AnimalBody,
                    animal,
                    queen);
                return true;
            }

            var mapObject = collider.GetComponent<MapObj>();
            if (mapObject != null)
            {
                target = new RuntimeCombatTarget(
                    CombatTargetKind.MapObject,
                    mapObject,
                    queen);
                return true;
            }

            var building = collider.GetComponent<Building>();
            if (IsVanillaArrowBuildingTarget(building))
            {
                target = new RuntimeCombatTarget(
                    CombatTargetKind.Building,
                    building,
                    queen);
                return true;
            }

            return false;
        }

        public void ApplyDamage(float damage)
        {
            if (damage <= 0f || !IsAlive)
            {
                return;
            }

            switch (Kind)
            {
                case CombatTargetKind.GameUnit:
                    (_instance as GameUnit)?.BeAttacked(
                        -damage,
                        Unit_Attacekd_Tag.Queen,
                        0);
                    break;
                case CombatTargetKind.AnimalBody:
                    (_instance as AnimalBody)?.BeAttacked(
                        -damage,
                        Unit_Attacekd_Tag.Queen);
                    break;
                case CombatTargetKind.MapObject:
                    (_instance as MapObj)?.BeAttacked(-damage);
                    break;
                case CombatTargetKind.Building:
                    var building = _instance as Building;
                    if (building != null)
                    {
                        var source = _queen != null && _queen.Tf != null
                            ? (Vector2)_queen.Tf.position
                            : (Vector2)building.Tf.position;
                        building.BeAttacked(
                            -damage,
                            source,
                            Unit_Attacekd_Tag.Queen);
                    }
                    break;
            }
        }

        public bool Equals(RuntimeCombatTarget other)
        {
            return other != null &&
                   Kind == other.Kind &&
                   ReferenceEquals(_instance, other._instance);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RuntimeCombatTarget);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ RuntimeHelpers.GetHashCode(_instance);
            }
        }

        private Vector3 GetPosition()
        {
            switch (Kind)
            {
                case CombatTargetKind.GameUnit:
                    var unit = _instance as GameUnit;
                    return unit != null && unit.Tf != null
                        ? unit.Tf.position
                        : Vector3.zero;
                case CombatTargetKind.AnimalBody:
                    var animal = _instance as AnimalBody;
                    return animal != null && animal.Tf != null
                        ? animal.Tf.position
                        : Vector3.zero;
                case CombatTargetKind.MapObject:
                    var mapObject = _instance as MapObj;
                    return mapObject != null && mapObject.Tf != null
                        ? mapObject.Tf.position
                        : Vector3.zero;
                case CombatTargetKind.Building:
                    var building = _instance as Building;
                    return building != null && building.Tf != null
                        ? building.Tf.position
                        : Vector3.zero;
                default:
                    return Vector3.zero;
            }
        }

        private bool CanReceiveSplashDamage()
        {
            if (!IsAlive)
            {
                return false;
            }

            switch (Kind)
            {
                case CombatTargetKind.GameUnit:
                    var unit = _instance as GameUnit;
                    return unit != null && Helpers.IsTeamEnemy(unit);
                case CombatTargetKind.AnimalBody:
                    var animal = _instance as AnimalBody;
                    return animal != null &&
                           animal.m_State != AnimalState.Death &&
                           animal.m_State != AnimalState.Stun;
                case CombatTargetKind.MapObject:
                    var mapObject = _instance as MapObj;
                    return mapObject != null && mapObject.gameObject.activeSelf;
                case CombatTargetKind.Building:
                    return IsVanillaArrowBuildingTarget(_instance as Building);
                default:
                    return false;
            }
        }

        private static void AddCandidate(
            ICollection<RuntimeCombatTarget> targets,
            RuntimeCombatTarget candidate)
        {
            if (candidate == null || !candidate.CanReceiveSplashDamage())
            {
                return;
            }

            if (!targets.Contains(candidate))
            {
                targets.Add(candidate);
            }
        }

        private static bool IsVanillaArrowBuildingTarget(Building building)
        {
            return building != null &&
                   building.m_Info != null &&
                   building.m_Info.Name == BuildingName.EnemyNexus &&
                   building.m_Info.Ability != BuildAbility.Wallpaper &&
                   building.m_BuildState != BuildState.NeedRepair;
        }
    }
}
