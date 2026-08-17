using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Xunit;

public sealed class UpgradeNode
{
    public int m_StartTime;

    public bool StateCheck()
    {
        return false;
    }
}

public sealed class ResearchUI
{
    public int m_Point;

    public void PointUp(int amount)
    {
    }
}

public sealed class TechInfo
{
    public int Point;
}

public sealed class CenterAlarmUI
{
    public void CenterAlarmSet(int state)
    {
    }
}

public sealed class CheatMgr
{
    public bool IsResearchFast;
}

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class TranspilerTests
    {
        [Fact]
        public void ResearchRewriteReplacesOnlyTwoUpgradeQueueLimitConstants()
        {
            var countGetter = typeof(List<UpgradeNode>).GetProperty("Count").GetGetMethod();
            var input = new[]
            {
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Callvirt, countGetter),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Bge_S, null),
                new CodeInstruction(OpCodes.Nop),
                new CodeInstruction(OpCodes.Callvirt, countGetter),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Blt_S, null)
            };

            var output = Rewrite("ResearchQueueTranspiler", input);

            Assert.Single(output, code => code.opcode == OpCodes.Ldc_I4_3);
            Assert.Equal(2, output.Count(code =>
                code.opcode == OpCodes.Call &&
                code.operand is MethodInfo method &&
                method.Name == "GetEffectiveLimit"));
        }

        [Fact]
        public void ResearchRewriteRejectsUnexpectedGuardCount()
        {
            var countGetter = typeof(List<UpgradeNode>).GetProperty("Count").GetGetMethod();
            var input = new[]
            {
                new CodeInstruction(OpCodes.Callvirt, countGetter),
                new CodeInstruction(OpCodes.Ldc_I4_3),
                new CodeInstruction(OpCodes.Bge_S, null)
            };

            Assert.Throws<InvalidOperationException>(() => Rewrite("ResearchQueueTranspiler", input));
        }

        [Fact]
        public void ReservationEnqueueRewriteRemovesThePointGateAndDefersTheCharge()
        {
            var pointField = typeof(ResearchUI).GetField(nameof(ResearchUI.m_Point));
            var techPointField = typeof(TechInfo).GetField(nameof(TechInfo.Point));
            var pointUp = typeof(ResearchUI).GetMethod(nameof(ResearchUI.PointUp));
            var alarm = typeof(CenterAlarmUI).GetMethod(nameof(CenterAlarmUI.CenterAlarmSet));
            var countGetter = typeof(List<UpgradeNode>).GetProperty("Count").GetGetMethod();
            var input = new[]
            {
                new CodeInstruction(OpCodes.Ldfld, techPointField),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, pointField),
                new CodeInstruction(OpCodes.Ble_S, null),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_2),
                new CodeInstruction(OpCodes.Callvirt, alarm),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Callvirt, countGetter),
                new CodeInstruction(OpCodes.Brtrue_S, null),
                new CodeInstruction(OpCodes.Ldstr, "Alarm/Research start"),
                new CodeInstruction(OpCodes.Ldstr, "Alarm/Research reserved"),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldc_I4_M1),
                new CodeInstruction(OpCodes.Callvirt, pointUp)
            };

            var output = Rewrite("ResearchReservationEnqueueTranspiler", input);

            Assert.Contains(output, code => Calls(code, "GetReservationBudget"));
            Assert.Contains(output, code => Calls(code, "ShouldAnnounceReservation"));
            Assert.Contains(output, code => Calls(code, "OnResearchQueued"));
            Assert.DoesNotContain(output, code =>
                code.operand is MethodInfo method && method == pointUp);
        }

        [Fact]
        public void ReservationProgressRewriteGuardsAllThreeHeadsAndFastResearchChecks()
        {
            var stateCheck = typeof(UpgradeNode).GetMethod(nameof(UpgradeNode.StateCheck));
            var fastField = typeof(CheatMgr).GetField(nameof(CheatMgr.IsResearchFast));
            var input = new List<CodeInstruction>();
            for (var index = 0; index < 3; index++)
            {
                input.Add(new CodeInstruction(OpCodes.Ldarg_0));
                input.Add(new CodeInstruction(OpCodes.Callvirt, stateCheck));
                input.Add(new CodeInstruction(OpCodes.Ldarg_0));
                input.Add(new CodeInstruction(OpCodes.Ldfld, fastField));
            }

            var output = Rewrite("ResearchProgressTranspiler", input);

            Assert.Equal(3, output.Count(code => Calls(code, "TryStartAndCheck")));
            Assert.Equal(3, output.Count(code => Calls(code, "CanUseFastResearch")));
            Assert.DoesNotContain(output, code =>
                (code.operand is MethodInfo method && method == stateCheck) ||
                (code.operand is FieldInfo field && field == fastField));
        }

        [Fact]
        public void ReservationRefundRewriteCapturesAndConditionallyRefundsAllThreeRemovals()
        {
            var removeAt = typeof(List<UpgradeNode>).GetMethod("RemoveAt");
            var pointUp = typeof(ResearchUI).GetMethod(nameof(ResearchUI.PointUp));
            var input = new List<CodeInstruction>();
            for (var index = 0; index < 3; index++)
            {
                input.Add(new CodeInstruction(OpCodes.Ldarg_0));
                input.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                input.Add(new CodeInstruction(OpCodes.Callvirt, removeAt));
                input.Add(new CodeInstruction(OpCodes.Ldarg_0));
                input.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                input.Add(new CodeInstruction(OpCodes.Callvirt, pointUp));
            }

            var output = Rewrite("ResearchRefundTranspiler", input);

            Assert.Equal(3, output.Count(code => Calls(code, "RemoveAndRememberRefund")));
            Assert.Equal(3, output.Count(code => Calls(code, "RefundRemovedResearch")));
            Assert.DoesNotContain(output, code =>
                code.operand is MethodInfo method &&
                (method == removeAt || method == pointUp));
        }

        [Fact]
        public void TradeRewriteReplacesTheSingleSevenSlotLoopWithRuntimeCapacity()
        {
            var input = new[]
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Blt_S, null)
            };

            var output = Rewrite("TradeLayoutTranspiler", input);

            Assert.DoesNotContain(output, code => code.opcode == OpCodes.Ldc_I4_7);
            Assert.Equal(OpCodes.Ldarg_1, output[1].opcode);
            Assert.Equal(OpCodes.Call, output[2].opcode);
            Assert.Equal("GetVisibleSlotCount", ((MethodInfo)output[2].operand).Name);
            Assert.Equal(OpCodes.Blt_S, output[3].opcode);
        }

        [Fact]
        public void TradeRewriteRejectsMissingOrDuplicateLoopBoundaries()
        {
            Assert.Throws<InvalidOperationException>(() => Rewrite(
                "TradeLayoutTranspiler",
                new[] { new CodeInstruction(OpCodes.Ldc_I4_6) }));

            Assert.Throws<InvalidOperationException>(() => Rewrite(
                "TradeLayoutTranspiler",
                new[]
                {
                    new CodeInstruction(OpCodes.Ldc_I4_7),
                    new CodeInstruction(OpCodes.Blt_S, null),
                    new CodeInstruction(OpCodes.Ldc_I4_7),
                    new CodeInstruction(OpCodes.Blt_S, null)
                }));
        }

        private static List<CodeInstruction> Rewrite(string typeName, IEnumerable<CodeInstruction> input)
        {
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, "ResearchAndTradeOptimization.dll");
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(
                "ResearchAndTradeOptimization.Patching." + typeName,
                throwOnError: false);
            Assert.NotNull(type);
            var method = type.GetMethod(
                "Rewrite",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);

            try
            {
                return ((IEnumerable<CodeInstruction>)method.Invoke(null, new object[] { input })).ToList();
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

        private static bool Calls(CodeInstruction instruction, string methodName)
        {
            return instruction.opcode == OpCodes.Call &&
                   instruction.operand is MethodInfo method &&
                   method.Name == methodName;
        }
    }
}
