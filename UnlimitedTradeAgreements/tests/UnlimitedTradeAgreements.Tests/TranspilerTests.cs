using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Xunit;

namespace UnlimitedTradeAgreements.Tests
{
    public sealed class TranspilerTests
    {
        [Fact]
        public void RewritesTheSingleVanillaSevenSlotLoop()
        {
            var output = Rewrite(new[]
            {
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Blt_S, null)
            });

            Assert.DoesNotContain(output, instruction => instruction.opcode == OpCodes.Ldc_I4_7);
            Assert.Equal(OpCodes.Ldarg_1, output[1].opcode);
            Assert.Equal(OpCodes.Call, output[2].opcode);
            Assert.Equal("GetVisibleSlotCount", ((MethodInfo)output[2].operand).Name);
            Assert.Equal(OpCodes.Blt_S, output[3].opcode);
        }

        [Fact]
        public void RejectsMissingOrDuplicateVanillaLoopBounds()
        {
            Assert.Throws<InvalidOperationException>(() => Rewrite(
                new[] { new CodeInstruction(OpCodes.Ldc_I4_6) }));

            Assert.Throws<InvalidOperationException>(() => Rewrite(new[]
            {
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Blt_S, null),
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Blt_S, null)
            }));
        }

        private static List<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> input)
        {
            var assembly = Assembly.LoadFrom(TestPaths.RequireFile(TestPaths.PluginAssembly));
            var type = assembly.GetType(
                "UnlimitedTradeAgreements.Patching.TradeLayoutTranspiler",
                throwOnError: false);
            Assert.NotNull(type);
            var method = type.GetMethod(
                "Rewrite",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);

            try
            {
                return ((IEnumerable<CodeInstruction>)method.Invoke(null, new object[] { input }))
                    .ToList();
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }
    }
}
