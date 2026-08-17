using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ResearchAndTradeOptimization.Runtime;

namespace ResearchAndTradeOptimization.Patching
{
    internal static class ResearchQueueTranspiler
    {
        private static readonly MethodInfo EffectiveLimitMethod =
            AccessTools.Method(typeof(ResearchQueueRuntime), nameof(ResearchQueueRuntime.GetEffectiveLimit));

        internal static IEnumerable<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> instructions)
        {
            var rewritten = instructions.ToList();
            var matches = new List<int>();

            for (var index = 1; index < rewritten.Count - 1; index++)
            {
                if (rewritten[index].opcode == OpCodes.Ldc_I4_3 &&
                    IsUpgradeQueueCount(rewritten[index - 1]) &&
                    IsConditionalLimitBranch(rewritten[index + 1].opcode))
                {
                    matches.Add(index);
                }
            }

            if (matches.Count != 2)
            {
                throw new InvalidOperationException(
                    $"研究队列 IL 结构不匹配：应找到 2 处上限判断，实际找到 {matches.Count} 处。");
            }

            foreach (var index in matches)
            {
                rewritten[index].opcode = OpCodes.Call;
                rewritten[index].operand = EffectiveLimitMethod;
            }

            return rewritten;
        }

        private static bool IsUpgradeQueueCount(CodeInstruction instruction)
        {
            if (!(instruction.operand is MethodInfo method) || method.Name != "get_Count")
            {
                return false;
            }

            var declaringType = method.DeclaringType;
            return declaringType != null &&
                   declaringType.IsGenericType &&
                   declaringType.GetGenericTypeDefinition() == typeof(List<>) &&
                   declaringType.GetGenericArguments()[0].Name == "UpgradeNode";
        }

        private static bool IsConditionalLimitBranch(OpCode opcode)
        {
            return opcode == OpCodes.Bge || opcode == OpCodes.Bge_S ||
                   opcode == OpCodes.Blt || opcode == OpCodes.Blt_S;
        }
    }
}
