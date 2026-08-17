using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ResearchAndTradeOptimization.Runtime;

namespace ResearchAndTradeOptimization.Patching
{
    internal static class ResearchRefundTranspiler
    {
        private static readonly MethodInfo RemoveMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.RemoveAndRememberRefund));

        private static readonly MethodInfo RefundMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.RefundRemovedResearch));

        internal static IEnumerable<CodeInstruction> Rewrite(
            IEnumerable<CodeInstruction> instructions)
        {
            var rewritten = instructions.ToList();
            var removals = new List<int>();
            var refunds = new List<int>();

            for (var index = 0; index < rewritten.Count; index++)
            {
                if (IsUpgradeQueueMethod(rewritten[index], "RemoveAt"))
                {
                    removals.Add(index);
                }

                if (rewritten[index].operand is MethodInfo method &&
                    method.DeclaringType?.Name == "ResearchUI" &&
                    method.Name == "PointUp")
                {
                    refunds.Add(index);
                }
            }

            RequireThree("队列移除", removals);
            RequireThree("研究点退款", refunds);

            foreach (var index in removals)
            {
                ReplaceWithCall(rewritten[index], RemoveMethod);
            }

            foreach (var index in refunds)
            {
                ReplaceWithCall(rewritten[index], RefundMethod);
            }

            return rewritten;
        }

        private static bool IsUpgradeQueueMethod(
            CodeInstruction instruction,
            string methodName)
        {
            if (!(instruction.operand is MethodInfo method) ||
                method.Name != methodName)
            {
                return false;
            }

            var type = method.DeclaringType;
            return type != null &&
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(List<>) &&
                   type.GetGenericArguments()[0].Name == "UpgradeNode";
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
                    $"研究退款 IL 结构不匹配：{description}应找到 3 处，" +
                    $"实际找到 {matches.Count} 处。");
            }
        }
    }
}
