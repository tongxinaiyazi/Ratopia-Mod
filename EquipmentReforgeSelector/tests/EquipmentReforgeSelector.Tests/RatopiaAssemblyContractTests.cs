using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class RatopiaAssemblyContractTests
    {
        private const string ExpectedHash = "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void Assembly_CSharp_matches_the_pinned_game_build()
        {
            using (var stream = File.OpenRead(AssemblyPath))
            using (var sha = SHA256.Create())
            {
                Assert.Equal(ExpectedHash, BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty));
            }
        }

        [Fact]
        public void Harmony_targets_and_runtime_fields_have_exact_signatures()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                AssertMethod(assembly, "BuildMidUI", "ItemDetail_Open", "System.Void", "ItemInfo", "System.Boolean", "System.Boolean", "System.Int32");
                AssertMethod(assembly, "T_Queen", "ItemEnhance", "System.String", "ItemInfo", "System.Int32", "Res_Ability");
                AssertMethod(assembly, "Helpers", "GetToolTipString", "System.String", "Res_Ability", "System.Single", "System.Boolean");
                AssertMethod(
                    assembly,
                    "SimpleToolTip",
                    "SimpleToolTipSet",
                    "System.Void",
                    "SimpleToolTip/SimpleToolTipList",
                    "System.Single",
                    "System.Single",
                    "System.Single");

                AssertField(assembly, "BuildMidUI", "Obj_Main", "UnityEngine.GameObject");
                AssertField(assembly, "BuildUI", "m_BuildType", "System.Int32");
                AssertField(assembly, "ItemInfo", "Index", "System.Int32");
                AssertField(assembly, "ItemInfo", "m_Type", "System.Int32");
                AssertField(assembly, "ItemEnhanceInfo", "Type", "System.Int32");
                AssertField(assembly, "ItemEnhanceInfo", "List_Ability1", "System.Collections.Generic.List`1<Res_Ability>");
                AssertField(assembly, "ItemEnhanceInfo", "List_AbilityValue1", "System.Collections.Generic.List`1<System.Single>");
                AssertField(assembly, "ItemEnhanceInfo", "List_Ability2", "System.Collections.Generic.List`1<Res_Ability>");
                AssertField(assembly, "ItemEnhanceInfo", "List_AbilityValue2", "System.Collections.Generic.List`1<System.Single>");
                AssertField(assembly, "T_Queen", "Dic_ItemPlusEffect", "System.Collections.Generic.Dictionary`2<System.Int32,System.Collections.Generic.List`1<ItemPlusInfo>>");
                AssertField(assembly, "ItemPlusInfo", "Level", "System.Int32");
                AssertField(assembly, "ItemPlusInfo", "m_Ability", "Res_Ability");
                AssertField(assembly, "SimpleToolTip", "m_EffectFrame", "Batch_ResEffect");
                AssertField(assembly, "Batch_ResEffect", "Txt_Value", "TMPro.TextMeshProUGUI[]");

                var tooltipEnum = RequireType(assembly, "SimpleToolTip")
                    .NestedTypes.Single(type => type.Name == "SimpleToolTipList");
                var enhanceEffect = tooltipEnum.Fields.Single(field => field.Name == "EnhanceEffect");
                Assert.Equal(92, Convert.ToInt32(enhanceEffect.Constant));
            }
        }

        [Fact]
        public void Vanilla_enhance_lookup_matches_ItemEnhanceInfo_Type_to_ItemInfo_m_Type()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                var displayClass = RequireType(assembly, "T_Queen").NestedTypes.Single(type => type.Name.Contains("DisplayClass180"));
                var predicate = displayClass.Methods.Single(method => method.Name == "<ItemEnhance>b__0");
                var fields = predicate.Body.Instructions
                    .Where(instruction => instruction.OpCode == OpCodes.Ldfld)
                    .Select(instruction => ((FieldReference)instruction.Operand).FullName)
                    .ToArray();

                Assert.Equal(new[] { "System.Int32 ItemEnhanceInfo::Type", "ItemInfo T_Queen/<>c__DisplayClass180_0::_info", "System.Int32 ItemInfo::m_Type" }, fields);
                Assert.Contains(predicate.Body.Instructions, instruction => instruction.OpCode == OpCodes.Ceq);
            }
        }

        [Fact]
        public void Royal_blacksmith_opens_level_1_and_HellAnvil_opens_level_2()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                var slotAction = RequireType(assembly, "BuildMid_QueenSlot_2").Methods.Single(method => method.Name == "SlotAction");
                var calls = slotAction.Body.Instructions
                    .Select((instruction, index) => new { instruction, index })
                    .Where(entry => entry.instruction.Operand is MethodReference &&
                                    ((MethodReference)entry.instruction.Operand).FullName == "System.Void BuildUI::BuildUI_AnvilSet(System.Int32,System.Int32)")
                    .ToArray();

                Assert.Equal(2, calls.Length);
                AssertCallContract(slotAction.Body.Instructions, calls[0].index, 344, 1, 2);
                AssertCallContract(slotAction.Body.Instructions, calls[1].index, 200, 1, 1);
            }
        }

        private static string AssemblyPath => Path.Combine(ContractTestPaths.GameDirectory, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");

        private static void AssertMethod(AssemblyDefinition assembly, string typeName, string methodName, string returnType, params string[] parameters)
        {
            var method = RequireType(assembly, typeName).Methods.Single(candidate => candidate.Name == methodName && candidate.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameters));
            Assert.Equal(returnType, method.ReturnType.FullName);
        }

        private static void AssertField(AssemblyDefinition assembly, string typeName, string fieldName, string fieldType)
        {
            Assert.Equal(fieldType, RequireType(assembly, typeName).Fields.Single(field => field.Name == fieldName).FieldType.FullName);
        }

        private static TypeDefinition RequireType(AssemblyDefinition assembly, string fullName)
        {
            return assembly.MainModule.Types.Single(type => type.FullName == fullName);
        }

        private static void AssertCallContract(IList<Instruction> instructions, int callIndex, int buildingName, int category, int level)
        {
            Assert.Equal(category, GetConstant(instructions[callIndex - 2]));
            Assert.Equal(level, GetConstant(instructions[callIndex - 1]));
            Assert.Contains(instructions.Skip(Math.Max(0, callIndex - 12)).Take(12), instruction => GetConstant(instruction) == buildingName);
        }

        private static int? GetConstant(Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldc_I4_M1: return -1;
                case Code.Ldc_I4_0: return 0;
                case Code.Ldc_I4_1: return 1;
                case Code.Ldc_I4_2: return 2;
                case Code.Ldc_I4_3: return 3;
                case Code.Ldc_I4_4: return 4;
                case Code.Ldc_I4_5: return 5;
                case Code.Ldc_I4_6: return 6;
                case Code.Ldc_I4_7: return 7;
                case Code.Ldc_I4_8: return 8;
                case Code.Ldc_I4:
                case Code.Ldc_I4_S: return Convert.ToInt32(instruction.Operand);
                default: return null;
            }
        }
    }
}
