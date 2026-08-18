using System;
using System.Collections.Generic;
using System.Threading;
using StrongerWorkDistance.Core;
using UnityEngine;

namespace StrongerWorkDistance.Runtime
{
    internal static class WorkAreaRuntime
    {
        private static int _applicationCount;

        internal static void Apply(SystemMgr systemManager)
        {
            if (systemManager == null)
            {
                throw new ArgumentNullException(nameof(systemManager));
            }

            var plannedOffsets = WorkAreaRules.CreateExpandedOffsets();
            var unityOffsets = new List<Vector2Int>(plannedOffsets.Count);
            for (var index = 0; index < plannedOffsets.Count; index++)
            {
                var offset = plannedOffsets[index];
                unityOffsets.Add(new Vector2Int(offset.X, offset.Y));
            }

            var originalWorkMarkCount = systemManager.List_WM_EnableArea.Count;
            var originalBlueprintCount = systemManager.List_BP_Ld_EnableArea.Count;
            AtomicListUpdater.ReplaceBoth(
                systemManager.List_WM_EnableArea,
                systemManager.List_BP_Ld_EnableArea,
                unityOffsets);

            var invocation = Interlocked.Increment(ref _applicationCount);
            Plugin.LogRuntimeInfo(
                $"工作距离已应用（第 {invocation} 次）：" +
                $"常规 {originalWorkMarkCount}->{systemManager.List_WM_EnableArea.Count}，" +
                $"蓝图 {originalBlueprintCount}->{systemManager.List_BP_Ld_EnableArea.Count}。");
        }
    }
}
