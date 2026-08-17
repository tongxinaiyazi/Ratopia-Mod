using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ResearchAndTradeOptimization.Runtime;

namespace ResearchAndTradeOptimization.Patching
{
    internal static class ResearchProgressTranspiler
    {
        private static readonly MethodInfo TryStartAndCheckMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.TryStartAndCheck));

        private static readonly MethodInfo CanUseFastResearchMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.CanUseFastResearch));

        internal static IEnumerable<CodeInstruction> Rewrite(
            IEnumerable<CodeInstruction> instructions)
        {
            var rewritten = instructions.ToList();
            var stateChecks = new List<int>();
            var fastResearchChecks = new List<int>();

            for (var index = 0; index < rewritten.Count; index++)
            {
                if (rewritten[index].operand is MethodInfo method &&
                    method.DeclaringType?.Name == "UpgradeNode" &&
                    method.Name == "StateCheck")
                {
                    stateChecks.Add(index);
                }

                if (rewritten[index].operand is FieldInfo field &&
                    field.DeclaringType?.Name == "CheatMgr" &&
                    field.Name == "IsResearchFast")
                {
                    fastResearchChecks.Add(index);
                }
            }

            RequireThree("队首 StateCheck", stateChecks);
            RequireThree("快速研究检查", fastResearchChecks);

            foreach (var index in stateChecks)
            {
                ReplaceWithCall(rewritten[index], TryStartAndCheckMethod);
            }

            foreach (var index in fastResearchChecks)
            {
                ReplaceWithCall(rewritten[index], CanUseFastResearchMethod);
            }

            return rewritten;
        }

        private static void ReplaceWithCall(
            CodeInstruction instruction,
            MethodInfo method)
        {
            instruction.opcode = OpCodes.Call;
            instruction.operand = method;
        }

        private static void RequireThree(string description, List<int> matches)
        {
            if (matches.Count != 3)
            {
                throw new InvalidOperationException(
                    $"研究进度 IL 结构不匹配：{description}应找到 3 处，" +
                    $"实际找到 {matches.Count} 处。");
            }
        }
    }
}
