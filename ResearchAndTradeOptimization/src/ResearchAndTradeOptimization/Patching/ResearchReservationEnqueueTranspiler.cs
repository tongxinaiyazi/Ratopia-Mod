using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ResearchAndTradeOptimization.Runtime;

namespace ResearchAndTradeOptimization.Patching
{
    internal static class ResearchReservationEnqueueTranspiler
    {
        private static readonly MethodInfo GetBudgetMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.GetReservationBudget));

        private static readonly MethodInfo AnnounceReservationMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.ShouldAnnounceReservation));

        private static readonly MethodInfo OnQueuedMethod =
            AccessTools.Method(
                typeof(ResearchReservationRuntime),
                nameof(ResearchReservationRuntime.OnResearchQueued));

        internal static IEnumerable<CodeInstruction> Rewrite(
            IEnumerable<CodeInstruction> instructions)
        {
            var rewritten = instructions.ToList();
            var pointGateMatches = new List<int>();
            var announcementMatches = new List<int>();
            var chargeMatches = new List<int>();

            for (var index = 0; index < rewritten.Count; index++)
            {
                if (IsField(rewritten[index], "ResearchUI", "m_Point") &&
                    IsPointComparisonWithin(rewritten, index + 1) &&
                    HasCallWithin(
                        rewritten,
                        index + 1,
                        10,
                        "CenterAlarmUI",
                        "CenterAlarmSet"))
                {
                    pointGateMatches.Add(index);
                }

                if (IsCall(
                        rewritten[index],
                        "System.Collections.Generic.List`1",
                        "get_Count") &&
                    index + 1 < rewritten.Count &&
                    IsTrueBranch(rewritten[index + 1].opcode) &&
                    HasStringWithin(
                        rewritten,
                        index + 2,
                        55,
                        "Alarm/Research reserved"))
                {
                    announcementMatches.Add(index);
                }

                if (IsCall(rewritten[index], "ResearchUI", "PointUp"))
                {
                    chargeMatches.Add(index);
                }
            }

            RequireSingle("研究点预约门槛", pointGateMatches);
            RequireSingle("研究开始/预约提示分支", announcementMatches);
            RequireSingle("研究预约立即扣点", chargeMatches);

            ReplaceWithCall(
                rewritten[pointGateMatches[0]],
                GetBudgetMethod);
            ReplaceWithCall(
                rewritten[announcementMatches[0]],
                AnnounceReservationMethod);
            ReplaceWithCall(
                rewritten[chargeMatches[0]],
                OnQueuedMethod);

            return rewritten;
        }

        private static void ReplaceWithCall(
            CodeInstruction instruction,
            MethodInfo method)
        {
            instruction.opcode = OpCodes.Call;
            instruction.operand = method;
        }

        private static bool IsField(
            CodeInstruction instruction,
            string declaringType,
            string fieldName)
        {
            return instruction.operand is FieldInfo field &&
                   field.DeclaringType?.Name == declaringType &&
                   field.Name == fieldName;
        }

        private static bool IsCall(
            CodeInstruction instruction,
            string declaringType,
            string methodName)
        {
            if (!(instruction.operand is MethodInfo method) ||
                method.Name != methodName)
            {
                return false;
            }

            var type = method.DeclaringType;
            if (type == null)
            {
                return false;
            }

            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            return type.FullName == declaringType || type.Name == declaringType;
        }

        private static bool HasCallWithin(
            IReadOnlyList<CodeInstruction> instructions,
            int start,
            int length,
            string declaringType,
            string methodName)
        {
            var end = Math.Min(instructions.Count, start + length);
            for (var index = start; index < end; index++)
            {
                if (IsCall(instructions[index], declaringType, methodName))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPointComparisonWithin(
            IReadOnlyList<CodeInstruction> instructions,
            int start)
        {
            var end = Math.Min(instructions.Count, start + 3);
            for (var index = start; index < end; index++)
            {
                var opcode = instructions[index].opcode;
                if (opcode == OpCodes.Ble || opcode == OpCodes.Ble_S ||
                    opcode == OpCodes.Bge || opcode == OpCodes.Bge_S)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasStringWithin(
            IReadOnlyList<CodeInstruction> instructions,
            int start,
            int length,
            string value)
        {
            var end = Math.Min(instructions.Count, start + length);
            for (var index = start; index < end; index++)
            {
                if (instructions[index].opcode == OpCodes.Ldstr &&
                    string.Equals(
                        instructions[index].operand as string,
                        value,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTrueBranch(OpCode opcode)
        {
            return opcode == OpCodes.Brtrue || opcode == OpCodes.Brtrue_S;
        }

        private static void RequireSingle(string description, List<int> matches)
        {
            if (matches.Count != 1)
            {
                throw new InvalidOperationException(
                    $"研究预约 IL 结构不匹配：{description}应找到 1 处，" +
                    $"实际找到 {matches.Count} 处。");
            }
        }
    }
}
