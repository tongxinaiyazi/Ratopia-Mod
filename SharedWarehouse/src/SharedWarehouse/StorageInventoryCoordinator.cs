using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using SharedWarehouse.Core;

namespace SharedWarehouse
{
    internal sealed class StorageInventoryCoordinator
    {
        private readonly ManualLogSource _log;
        private readonly SharedListCoordinator<Building_Storage, TileSt_Info> _core;
        private readonly CapacityOverrideRegistry<BuildInfo> _capacityOverrides;

        public StorageInventoryCoordinator(ManualLogSource log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _core = new SharedListCoordinator<Building_Storage, TileSt_Info>(
                storage => storage.m_ID,
                storage => storage.List_TileObj,
                (storage, inventory) => storage.List_TileObj = inventory,
                MergeInventories);
            _capacityOverrides = new CapacityOverrideRegistry<BuildInfo>(
                info => info.EffectValue1_Num,
                (info, value) => info.EffectValue1_Num = value,
                float.PositiveInfinity,
                float.IsPositiveInfinity);
        }

        public bool IsInitialized => _core.IsInitialized;

        public int SharedMaterialTypeCount => _core.SharedInventory?.Count ?? 0;

        public static bool IsTarget(Building_Storage storage)
        {
            return storage != null
                && storage.m_Info != null
                && StorageRules.IsTargetBuilding((int)storage.m_Info.Name);
        }

        public void Initialize(BuildingMgr manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var targets = (manager.List_Storage ?? new List<Building_Storage>())
                .Where(IsTarget)
                .OrderBy(storage => storage.m_ID)
                .ToList();

            foreach (var storage in targets)
            {
                ApplyInfiniteCapacity(storage);
            }

            _core.Initialize(targets);
            var itemCount = _core.SharedInventory?.Sum(ItemCount) ?? 0;
            _log.LogInfo(
                $"共享仓库初始化完成：仓库 {targets.Count} 座，材料种类 {_core.SharedInventory?.Count ?? 0}，物品 {itemCount} 个。");
        }

        public void Attach(Building_Storage storage)
        {
            if (!IsTarget(storage))
            {
                return;
            }

            ApplyInfiniteCapacity(storage);
            _core.Attach(storage);
            _log.LogDebug($"仓库 #{storage.m_ID} 已接入共享库存。");
        }

        public bool DetachForDemolition(Building_Storage storage)
        {
            if (!IsTarget(storage))
            {
                return false;
            }

            var detached = _core.DetachForDemolition(storage);
            if (detached)
            {
                _log.LogDebug($"仓库 #{storage.m_ID} 已在拆除前与共享库存安全分离。");
            }

            return detached;
        }

        public void Remove(Building_Storage storage)
        {
            if (ReferenceEquals(storage, null))
            {
                return;
            }

            _core.Remove(storage);
        }

        public IDisposable EnterSingleView()
        {
            return _core.EnterSingleView();
        }

        public IDisposable EnterActiveResourceView(BuildingMgr manager)
        {
            var representative = manager?.List_Storage?
                .Where(storage => IsTarget(storage)
                    && storage.m_BuildState.Equals(default(BuildState))
                    && storage.m_Activation)
                .OrderBy(storage => storage.m_ID)
                .FirstOrDefault();
            return _core.EnterSingleView(representative);
        }

        public void ResetSession()
        {
            _core.Reset();
        }

        public void RestoreCapacityOverrides()
        {
            _capacityOverrides.RestoreAll();
        }

        private void ApplyInfiniteCapacity(Building_Storage storage)
        {
            if (storage?.m_Info != null)
            {
                _capacityOverrides.Apply(storage.m_Info);
            }
        }

        private static List<TileSt_Info> MergeInventories(IEnumerable<List<TileSt_Info>> inventories)
        {
            return InventoryMerger.MergeUnique(
                inventories,
                entry => entry.m_Type,
                CloneEntry,
                (target, source) =>
                {
                    if (source.List_Reservation != null)
                    {
                        target.List_Reservation.AddRange(source.List_Reservation);
                    }
                });
        }

        private static TileSt_Info CloneEntry(TileSt_Info source)
        {
            var clone = new TileSt_Info(source.m_Type, source.m_State, 0);
            if (source.List_Reservation != null)
            {
                clone.List_Reservation.AddRange(source.List_Reservation);
            }

            return clone;
        }

        private static int ItemCount(TileSt_Info entry)
        {
            return entry?.List_Reservation?.Count ?? 0;
        }
    }
}
