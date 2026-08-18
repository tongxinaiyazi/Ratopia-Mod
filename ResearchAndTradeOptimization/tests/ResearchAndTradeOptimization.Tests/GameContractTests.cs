using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesTheInspectedBuild()
        {
            using (var stream = File.OpenRead(GetAssemblyPath()))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void HarmonyTargetsAndPrivateUiFieldsKeepTheirExactContracts()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "Tech_RPInfo", "UpgradBtn", "System.Void");
                AssertMethod(module, "ResearchingGroup", "ResearchingGroupSet", "System.Void");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "IsFullTradeAgreement",
                    "System.Boolean");
                AssertMethod(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI",
                    "UpdateSlot",
                    "System.Void",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData");
                AssertMethod(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "Refresh",
                    "System.Void");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "PickUpTradeResources",
                    "TileType[]",
                    "CasselGames.Diplomatic.Asset.DiplomaticAsset",
                    "CasselGames.Diplomatic.Asset.DiplomaticCountryTradeRawData[]",
                    "System.Collections.Generic.KeyValuePair`2<System.Int32,TileType>[]&");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "SetTradeResource",
                    "System.Void",
                    "CasselGames.Diplomatic.Asset.DiplomaticAsset");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "SetSavableData",
                    "System.Void",
                    "CasselGames.Diplomatic.Asset.DiplomaticAsset",
                    "Utility.Savable.SavableData");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "RemakeTradeData",
                    "System.Void",
                    "CasselGames.Diplomatic.Asset.DiplomaticAsset");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryPackage",
                    "RunProcessDaily",
                    "System.Void",
                    "System.TimeSpan",
                    "CasselGames.Diplomatic.Asset.DiplomaticAsset");

                AssertPrivateField(module, "ResearchingGroup", "Arr_Technode", "TechNode[]");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI",
                    "_newSlotUI",
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSlotUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_country",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_tradeAgreementValueText",
                    "TMPro.TextMeshProUGUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeDetailUI",
                    "_layoutUI",
                    "Utility.UI.VerticalLayoutUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeDetailSlotUI",
                    "_typeOrder",
                    "CasselGames.Diplomatic.UI.TypeTradeOrder");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSheetUI",
                    "_newData",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSheetUI",
                    "_isModified",
                    "System.Boolean");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSheetDetailSlotUI",
                    "_minValue",
                    "System.Int32");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSheetDetailSlotUI",
                    "_maxValue",
                    "System.Int32");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_raw",
                    "CasselGames.Diplomatic.Asset.DiplomaticCountryRawData");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_countryToHometownArray",
                    "System.Collections.Generic.KeyValuePair`2<System.Int32,TileType>[]");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_hometownToCountryArray",
                    "System.Collections.Generic.KeyValuePair`2<System.Int32,TileType>[]");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_allCountryToHometownList",
                    "System.Collections.Generic.List`1<TileType>");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_allHometownToCountryList",
                    "System.Collections.Generic.List`1<TileType>");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_countryToHometownList",
                    "System.Collections.Generic.List`1<TileType>");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_hometownToCountryList",
                    "System.Collections.Generic.List`1<TileType>");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "_useResources",
                    "System.Collections.Generic.List`1<TileType>");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.Asset.DiplomaticTradeResourceGroupAsset",
                    "_usedResourceList",
                    "System.Collections.Generic.List`1<TileType>");
                AssertPublicProperty(module,
                    "CasselGames.Diplomatic.Asset.DiplomaticTradeResourceGroupData",
                    "IsGlobal",
                    "System.Boolean");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceLayoutUI",
                    "_titleText",
                    "TMPro.TextMeshProUGUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceLayoutUI",
                    "_contents",
                    "UnityEngine.Transform");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_informationPanel",
                    "UnityEngine.GameObject");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_importsLayoutUI",
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceLayoutUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_exportsLayoutUI",
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceLayoutUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticUI",
                    "_sheetUI",
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSheetUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceLayoutUI",
                    "_slotsUI",
                    "System.Collections.Generic.List`1<CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceSlotUI>");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceSlotUI",
                    "_tileType",
                    "TileType");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailResourceSlotUI",
                    "_icon",
                    "UnityEngine.UI.Image");

                AssertMethod(module,
                    "CasselGames.Diplomatic.UI.DiplomaticUI",
                    "OnTradeDetailEvent",
                    "System.Void",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                    "CasselGames.Diplomatic.UI.TypeTradeOrder");
                AssertMethod(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSheetDetailSlotUI",
                    "SetData",
                    "System.Void",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                    "CasselGames.Diplomatic.UI.TypeTradeSheet",
                    "System.Boolean");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                    "IsInfinitePeriod",
                    "System.Boolean");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                    "IsEnded",
                    "System.Boolean");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                    "get_Resource",
                    "TileType");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "get_Sheets",
                    "System.Collections.Generic.List`1<CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData>");
            }
        }

        [Fact]
        public void ResearchUpgradeButtonContainsExactlyTwoQueueLimitGuards()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var method = FindMethod(module, "Tech_RPInfo", "UpgradBtn");
                var instructions = method.Body.Instructions;
                var matches = new List<Instruction>();
                for (var index = 1; index < instructions.Count; index++)
                {
                    if (!IsLoadInt(instructions[index], 3))
                    {
                        continue;
                    }

                    if (instructions[index - 1].Operand is MethodReference call &&
                        call.DeclaringType.FullName == "System.Collections.Generic.List`1<UpgradeNode>" &&
                        call.Name == "get_Count")
                    {
                        matches.Add(instructions[index]);
                    }
                }

                Assert.Equal(2, matches.Count);
            }
        }

        [Fact]
        public void DeferredResearchTargetsKeepTheirExactIlContracts()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var upgrade = FindMethod(module, "Tech_RPInfo", "UpgradBtn");
                Assert.Equal(1, CountCalls(upgrade, "ResearchUI", "PointUp"));
                Assert.Equal(1, CountConditionalResearchPointGuards(upgrade));
                Assert.Equal(1, CountQueueAnnouncementBranches(upgrade));

                var update = FindMethod(module, "ResearchUI", "UpdateUpgradeNode");
                Assert.Equal(3, CountCalls(update, "UpgradeNode", "StateCheck"));
                Assert.Equal(3, CountCalls(update, "UpgradeNode", "Refresh"));

                var remove = FindMethod(module, "Tech_RPInfo", "RemoveUpgradeNode");
                Assert.Equal(3, CountCalls(remove, "System.Collections.Generic.List`1<UpgradeNode>", "RemoveAt"));
                Assert.Equal(3, CountCalls(remove, "ResearchUI", "PointUp"));

                var refresh = FindMethod(module, "UpgradeNode", "Refresh");
                Assert.NotNull(refresh);
                var upgradeNode = FindTypes(module.Types)
                    .Single(type => type.FullName == "UpgradeNode");
                Assert.True(upgradeNode.IsSerializable);
                AssertPublicField(module, "UpgradeNode", "m_StartTime", "System.Int32");

                var researchData = FindTypes(module.Types)
                    .Single(type => type.FullName == "Research_Data");
                Assert.True(researchData.IsSerializable);
                AssertPublicField(
                    module,
                    "Research_Data",
                    "List_UpgradeNode",
                    "System.Collections.Generic.List`1<UpgradeNode>");
                AssertPublicField(
                    module,
                    "Research_Data",
                    "List_UpgradeNodeByScience",
                    "System.Collections.Generic.List`1<UpgradeNode>");
                AssertPublicField(
                    module,
                    "Research_Data",
                    "List_UpgradeNodeByMagic",
                    "System.Collections.Generic.List`1<UpgradeNode>");
            }
        }

        [Fact]
        public void ResearchRefreshUsesThreeFullQueueLoopBoundsWithNativeNodeIndexing()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var method = FindMethod(
                    module,
                    "ResearchingGroup",
                    "ResearchingGroupSet");
                var instructions = method.Body.Instructions;
                var fullQueueLoopGuards = new List<Instruction>();
                for (var index = 1; index < instructions.Count; index++)
                {
                    var code = instructions[index].OpCode.Code;
                    if (code != Code.Bge && code != Code.Bge_S)
                    {
                        continue;
                    }

                    if (instructions[index - 1].Operand is MethodReference called &&
                        called.DeclaringType.FullName ==
                            "System.Collections.Generic.List`1<UpgradeNode>" &&
                        called.Name == "get_Count")
                    {
                        fullQueueLoopGuards.Add(instructions[index]);
                    }
                }

                Assert.Equal(3, fullQueueLoopGuards.Count);
                Assert.Contains(instructions, instruction =>
                    instruction.Operand is FieldReference field &&
                    field.DeclaringType.FullName == "ResearchingGroup" &&
                    field.Name == "Arr_Technode");
                Assert.Contains(instructions, instruction =>
                    instruction.OpCode.Code == Code.Ldelem_Ref);
            }
        }

        [Fact]
        public void TradeLayoutContainsOneSevenSlotLoopBoundary()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var method = FindMethod(
                    module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI",
                    "UpdateSlot");
                var instructions = method.Body.Instructions;
                var matches = new List<Instruction>();
                for (var index = 0; index < instructions.Count - 1; index++)
                {
                    if (IsLoadInt(instructions[index], 7) &&
                        (instructions[index + 1].OpCode.Code == Code.Blt ||
                         instructions[index + 1].OpCode.Code == Code.Blt_S))
                    {
                        matches.Add(instructions[index]);
                    }
                }

                Assert.Single(matches);
            }
        }

        [Fact]
        public void TradeAgreementGetterAndSheetCleanupRemainSeparateVanillaLimits()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var maxAgreement = FindMethod(
                    module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "get_MaxTradeAgreementCount");
                var fullSheet = FindMethod(
                    module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "IsFullTradeSheet");
                Assert.Contains(maxAgreement.Body.Instructions, instruction => IsLoadInt(instruction, 3));
                Assert.Contains(fullSheet.Body.Instructions, instruction => IsLoadInt(instruction, 7));
            }
        }

        [Fact]
        public void VanillaTradeResourcePickerStillContainsRandomSelectionAndGlobalPoolCalls()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var picker = FindMethod(
                    module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "PickUpTradeResources");
                Assert.Contains(picker.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName == "UnityEngine.Random" &&
                    called.Name == "Range");
                Assert.Contains(picker.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference called &&
                    called.DeclaringType.FullName ==
                        "CasselGames.Diplomatic.Asset.DiplomaticTradeResourceGroupAsset" &&
                    called.Name == "AddUseResource");
            }
        }

        [Fact]
        public void OrdinaryTradeBoundaryConstantsMatchTheInspectedBuild()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var tileType = FindTypes(module.Types)
                    .Single(type => type.FullName == "TileType");
                var watt = tileType.Fields.Single(field => field.Name == "Watt");
                Assert.Equal(4001, Convert.ToInt32(watt.Constant));

                var tradeState = FindTypes(module.Types)
                    .Single(type => type.FullName ==
                        "CasselGames.Diplomatic.Data.TypeTradeState");
                Assert.Equal(1, GetEnumValue(tradeState, "Run"));
                Assert.Equal(2, GetEnumValue(tradeState, "End"));
                Assert.Equal(3, GetEnumValue(tradeState, "Stop"));
                Assert.Equal(10, GetEnumValue(tradeState, "Storage_Trouble"));
                Assert.Equal(17, GetEnumValue(tradeState, "Not_Connected_PowerGrid"));

                var initializer = FindMethod(module, "Defines", ".cctor");
                var instructions = initializer.Body.Instructions;
                Assert.Contains(Enumerable.Range(0, instructions.Count - 1), index =>
                    IsLoadInt(instructions[index], 12) &&
                    instructions[index + 1].Operand is FieldReference field &&
                    field.DeclaringType.FullName == "Defines" &&
                    field.Name == "DayOfQuarter");
            }
        }

        private static string GetAssemblyPath()
        {
            var ratopiaDir = Environment.GetEnvironmentVariable("RATOPIA_DIR");
            if (string.IsNullOrWhiteSpace(ratopiaDir))
            {
                ratopiaDir = typeof(GameContractTests).Assembly
                    .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .Cast<AssemblyMetadataAttribute>()
                    .Single(attribute => attribute.Key == "RatopiaDir")
                    .Value;
            }

            var path = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");
            Assert.True(File.Exists(path), $"Assembly-CSharp.dll not found: {path}");
            return path;
        }

        private static MethodDefinition FindMethod(ModuleDefinition module, string typeName, string methodName)
        {
            return FindTypes(module.Types).Single(type => type.FullName == typeName)
                .Methods.Single(method => method.Name == methodName);
        }

        private static void AssertMethod(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string returnType,
            params string[] parameterTypes)
        {
            var method = FindTypes(module.Types).Single(type => type.FullName == typeName)
                .Methods.Single(item =>
                    item.Name == methodName &&
                    item.ReturnType.FullName == returnType &&
                    item.Parameters.Select(parameter => parameter.ParameterType.FullName)
                        .SequenceEqual(parameterTypes));
            Assert.NotNull(method);
        }

        private static void AssertPrivateField(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            string fieldType)
        {
            var field = FindTypes(module.Types).Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.True(field.IsPrivate);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static void AssertPublicField(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            string fieldType)
        {
            var field = FindTypes(module.Types).Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.True(field.IsPublic);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static int CountCalls(
            MethodDefinition method,
            string declaringType,
            string methodName)
        {
            return method.Body.Instructions.Count(instruction =>
                instruction.Operand is MethodReference called &&
                called.DeclaringType.FullName == declaringType &&
                called.Name == methodName);
        }

        private static int CountConditionalResearchPointGuards(MethodDefinition method)
        {
            var instructions = method.Body.Instructions;
            var count = 0;
            for (var index = 0; index < instructions.Count; index++)
            {
                if (!(instructions[index].Operand is FieldReference field) ||
                    field.DeclaringType.FullName != "ResearchUI" ||
                    field.Name != "m_Point")
                {
                    continue;
                }

                var end = Math.Min(instructions.Count, index + 10);
                if (Enumerable.Range(index + 1, end - index - 1).Any(candidate =>
                    instructions[candidate].Operand is MethodReference called &&
                    called.DeclaringType.FullName == "CenterAlarmUI" &&
                    called.Name == "CenterAlarmSet"))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountQueueAnnouncementBranches(MethodDefinition method)
        {
            return method.Body.Instructions.Count(instruction =>
                instruction.OpCode.Code == Code.Ldstr &&
                string.Equals(
                    instruction.Operand as string,
                    "Alarm/Research reserved",
                    StringComparison.Ordinal));
        }

        private static void AssertPublicProperty(
            ModuleDefinition module,
            string typeName,
            string propertyName,
            string propertyType)
        {
            var property = FindTypes(module.Types).Single(type => type.FullName == typeName)
                .Properties.Single(item => item.Name == propertyName);
            Assert.Equal(propertyType, property.PropertyType.FullName);
            Assert.NotNull(property.GetMethod);
            Assert.True(property.GetMethod.IsPublic);
        }

        private static bool IsLoadInt(Instruction instruction, int value)
        {
            if (value == 3 && instruction.OpCode.Code == Code.Ldc_I4_3)
            {
                return true;
            }

            if (value == 7 && instruction.OpCode.Code == Code.Ldc_I4_7)
            {
                return true;
            }

            if (value == 12 && instruction.OpCode.Code == Code.Ldc_I4_S &&
                Convert.ToInt32(instruction.Operand) == value)
            {
                return true;
            }

            return (instruction.OpCode.Code == Code.Ldc_I4 || instruction.OpCode.Code == Code.Ldc_I4_S) &&
                   Convert.ToInt32(instruction.Operand) == value;
        }

        private static int GetEnumValue(TypeDefinition enumType, string name)
        {
            return Convert.ToInt32(
                enumType.Fields.Single(field => field.Name == name).Constant);
        }

        private static IEnumerable<TypeDefinition> FindTypes(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                yield return type;
                foreach (var nested in FindTypes(type.NestedTypes))
                {
                    yield return nested;
                }
            }
        }
    }
}
