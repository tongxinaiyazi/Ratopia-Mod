using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ResearchAndTradeOptimization.Runtime;

namespace ResearchAndTradeOptimization.Patching
{
    internal static class TradeLayoutTranspiler
    {
        private static readonly MethodInfo VisibleSlotCountMethod =
            AccessTools.Method(typeof(TradeQueueRuntime), nameof(TradeQueueRuntime.GetVisibleSlotCount));

        internal static IEnumerable<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> instructions)
        {
            var rewritten = instructions.ToList();
            var matches = new List<int>();

            for (var index = 0; index < rewritten.Count - 1; index++)
            {
                if (rewritten[index].opcode == OpCodes.Ldc_I4_7 &&
                    IsLessThanBranch(rewritten[index + 1].opcode))
                {
                    matches.Add(index);
                }
            }

            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"贸易槽位 IL 结构不匹配：应找到 1 处固定 7 槽循环，实际找到 {matches.Count} 处。");
            }

            var match = matches[0];
            var original = rewritten[match];
            var loadCountry = new CodeInstruction(OpCodes.Ldarg_1);
            loadCountry.labels.AddRange(original.labels);
            loadCountry.blocks.AddRange(original.blocks);
            rewritten[match] = loadCountry;
            rewritten.Insert(match + 1, new CodeInstruction(OpCodes.Call, VisibleSlotCountMethod));

            return rewritten;
        }

        private static bool IsLessThanBranch(OpCode opcode)
        {
            return opcode == OpCodes.Blt || opcode == OpCodes.Blt_S;
        }
    }
}
