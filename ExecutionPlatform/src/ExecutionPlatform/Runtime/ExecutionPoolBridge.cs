using System;
using UnityEngine;

namespace ExecutionPlatform.Runtime
{
    internal static class ExecutionPoolBridge
    {
        private const string PrisonPoolName = "Pool_Prison";

        internal static bool TryBuild(
            BuildingMgr manager,
            BuildingName name,
            Vector2 position,
            int attributeNumber,
            out Building result)
        {
            result = null;
            if (!name.Equals(ExecutionCatalog.RuntimeBuildingName))
            {
                return false;
            }

            if (!ExecutionRuntime.IsEnabled)
            {
                return true;
            }

            MemoryPool prisonPool = null;
            GameObject pooledObject = null;
            Building building = null;
            BuildInfo prison = null;
            BuildInfo executionInfo = null;
            var initializationStarted = false;
            try
            {
                var database = GameMgr.Instance?._DB_Mgr;
                if (manager?.List_Pool == null ||
                    database?.Dic_BuildDB == null ||
                    !database.Dic_BuildDB.TryGetValue(name, out executionInfo))
                {
                    ExecutionRuntime.FailClosed("创建处刑台失败：建筑数据库或对象池管理器不可用。");
                    return true;
                }

                foreach (var pool in manager.List_Pool)
                {
                    if (pool != null && string.Equals(pool.name, PrisonPoolName, StringComparison.Ordinal))
                    {
                        prisonPool = pool;
                        break;
                    }
                }

                database.Dic_BuildDB.TryGetValue(BuildingName.Prison, out prison);
                pooledObject = prisonPool?.GetNextObj();
                building = pooledObject == null ? null : pooledObject.GetComponent<Building>();
                if (!(building is Building_Prison))
                {
                    ReturnCheckedOutObject(prisonPool, pooledObject, building, prison);
                    pooledObject = null;
                    ExecutionRuntime.FailClosed("创建处刑台失败：找不到可复用的 Pool_Prison 监狱实例。");
                    return true;
                }

                initializationStarted = true;
                building.BuildingSet(executionInfo, position, attributeNumber);
                result = building;
                return true;
            }
            catch (Exception exception)
            {
                if (initializationStarted)
                {
                    CleanupFailedInitialization(pooledObject, building, executionInfo);
                }
                else
                {
                    ReturnCheckedOutObject(prisonPool, pooledObject, building, prison);
                }
                ExecutionRuntime.FailClosed("从 Pool_Prison 创建处刑台时发生异常。", exception);
                return true;
            }
        }

        internal static BuildInfo PrepareRecycle(Building building)
        {
            if (!ExecutionCatalog.IsExecution(building))
            {
                return null;
            }

            var original = building.m_Info;
            var database = GameMgr.Instance?._DB_Mgr;
            if (database?.Dic_BuildDB == null ||
                !database.Dic_BuildDB.TryGetValue(BuildingName.Prison, out var prison))
            {
                ExecutionRuntime.FailClosed("回收处刑台失败：找不到原版监狱建筑信息。");
                return null;
            }

            building.m_Info = prison;
            return original;
        }

        internal static void RestoreAfterRecycle(Building building, BuildInfo original)
        {
            if (building != null && original != null)
            {
                building.m_Info = original;
            }
        }

        private static void ReturnCheckedOutObject(
            MemoryPool pool,
            GameObject pooledObject,
            Building building,
            BuildInfo prison)
        {
            if (pooledObject == null || pool == null)
            {
                return;
            }

            try
            {
                if (building != null && prison != null)
                {
                    building.m_Info = prison;
                }

                pool.AddObj(pooledObject);
            }
            catch (Exception exception)
            {
                ExecutionRuntime.FailClosed("处刑台创建失败后无法归还监狱对象池实例。", exception);
            }
        }

        private static void CleanupFailedInitialization(
            GameObject pooledObject,
            Building building,
            BuildInfo executionInfo)
        {
            if (building == null)
            {
                if (pooledObject != null)
                {
                    pooledObject.SetActive(false);
                }
                return;
            }

            try
            {
                if (building.m_Info == null)
                {
                    building.m_Info = executionInfo;
                }

                building.BuildingDemolition(false);
            }
            catch (Exception exception)
            {
                if (pooledObject != null)
                {
                    pooledObject.SetActive(false);
                }

                ExecutionRuntime.FailClosed(
                    "处刑台初始化失败后，原版建筑清理也失败；该实例已隔离且不会放回对象池。",
                    exception);
            }
        }
    }
}
