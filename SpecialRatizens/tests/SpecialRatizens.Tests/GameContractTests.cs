using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace SpecialRatizens.Tests
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
        public void GenerationAndLifecycleTargetsKeepTheirExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "DB_Mgr", "Character_DB_Setting", "System.Void");
                AssertMethod(module, "TileMgr", "All_NotUseListClear", "System.Void");
                AssertMethod(module, "CitizenCaveUI", "MakeCitizenList", "System.Void");
                AssertMethod(module, "CCMake_Info", ".ctor", "System.Void", "System.Int32", "System.Boolean");
                AssertMethod(module, "CCMake_Info", "MakeCharacterList", "System.Void");
                AssertMethod(module, "T_Citizen", "MakeCtizen_ByCC", "System.Void", "UnityEngine.Vector2", "CCMake_Info");
            }
        }

        [Fact]
        public void EffectAndAppearanceTargetsKeepTheirExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "T_Citizen", "BeAttacked", "System.Void", "System.Single", "Unit_Attacekd_Tag", "System.Int32");
                AssertMethod(module, "T_Citizen", "HungerUpdate", "System.Void", "System.Single");
                AssertMethod(module, "T_Citizen", "DefaultClothesUpdate", "System.Void");
                AssertMethod(module, "GameUnit", "UpdatePDI", "System.Void", "PDI", "System.Single");
                AssertMethod(module, "GameUnit", "ClothesUpdate", "System.Void", "System.Int32", "System.Boolean");
                AssertMethod(module, "MasonryInfo", "WorkUpdate", "System.Boolean", "System.Single");
                AssertMethod(module, "CitizenBuff/RefInfo", "GetIconAddress", "System.String", "System.String", "C_Buff_Category");
                AssertMethod(module, "CitizenBuff/RefInfo", "Get_T_Name", "System.String", "System.String", "C_Buff_Category", "System.Boolean");
                AssertMethod(module, "CitizenBuff/RefInfo", "GetDescript", "System.String");
            }
        }

        [Fact]
        public void PowerIndustryAndEconomyTargetsKeepTheirExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethod(module, "GBot", "MakeCitizen", "System.Void", "UnityEngine.Vector2", "System.Int32");
                AssertMethod(module, "GBot", "FatigueUpate", "System.Void", "System.Single", "System.Boolean");
                AssertMethod(module, "ElecLine_Info", "AddConnectUseBuild", "System.Void", "System.Int32", "System.Single");
                AssertMethod(module, "ElecLine_Info", "AddWatt", "System.Void", "System.Single");
                AssertMethod(module, "ElecLine_Info", "UseWatt", "System.Boolean", "System.Int32", "System.Single");
                AssertMethod(module, "Building", "WireCheck", "System.Boolean", "System.Boolean");
                AssertMethod(module, "Building_ElecMasonry", "WireCheck", "System.Boolean", "System.Boolean");
                AssertMethod(module, "Building_ElecCarrierStation", "WireCheck", "System.Boolean", "System.Boolean");
                AssertMethod(module, "Building_ElecBandstand", "WireCheck", "System.Boolean", "System.Boolean");
                AssertMethod(module, "BuildingMgr", "GetFourDir_ElecGroup", "System.Collections.Generic.List`1<ElecLine_Info>", "ElecPort", "System.Boolean");
                AssertMethod(module, "BuildingMgr", "DeleteConnectCheck", "System.Void", "System.Int32", "System.Collections.Generic.List`1<ElecPort>");
                AssertMethod(module, "T_Citizen", "ApplyFoodOrLife_ResAbility", "System.Void", "TileInfo");
                AssertMethod(module, "Helpers", "Get_MaximumGuestNum", "System.Int32", "BuildingName");
                AssertMethod(module, "CasselGames.Diplomatic.Data.DiplomaticCountryResourceData", "TradeCountryToMyKingdomPrice", "System.Single", "System.Single", "System.Int32");
                AssertMethod(module, "CasselGames.Diplomatic.Data.DiplomaticCountryResourceData", "TradeMyKingdomToCountryPrice", "System.Single", "System.Single", "System.Int32");
                AssertMethod(module, "CasselGames.Diplomatic.DiplomaticMgr", "OnTradeResultEvent", "CasselGames.Diplomatic.Data.TradeReceive", "CasselGames.Diplomatic.Data.TradeResult");
                AssertMethod(module, "CasselGames.Diplomatic.Data.DiplomaticData", "SetTerrainTotalDistance", "System.Void", "CasselGames.Diplomatic.Data.DiplomaticWorldTerrainEntity");
                AssertMethod(module, "CasselGames.Diplomatic.Data.DiplomaticCountryData", "get_MaxTradeAgreementCount", "System.Int32");
                AssertMethod(module, "CasselGames.Diplomatic.UI.DiplomaticTradeSheetDetailContentsUI", "SetData", "System.Void",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                    "CasselGames.Diplomatic.UI.TypeTradeSheetCategory",
                    "CasselGames.Diplomatic.UI.TypeTradeSheet");
            }
        }

        [Fact]
        public void CharacterInfoStorageContractRemainsStable()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertField(module, "CharacterInfo", "Index", "System.Int32");
                AssertField(module, "CharacterInfo", "Category", "System.Int32");
                AssertField(module, "CharacterInfo", "Name", "System.String");
                AssertField(module, "CharacterInfo", "T_Name", "System.String");
                AssertField(module, "CharacterInfo", "EffectValue_A", "System.Single");
                AssertField(module, "CharacterInfo", "EffectValue_B", "System.Single");
                AssertField(module, "CharacterInfo", "Description", "System.String");
                AssertField(module, "DB_Mgr", "m_CharacterDB", "DatabaseCharacter");
            }
        }

        [Fact]
        public void ProsperityDatabaseIsBuiltBeforeCharacterDatabasePostfixRuns()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                var awake = FindType(module, "DB_Mgr").Methods.Single(method => method.Name == "Awake");
                var calls = awake.Body.Instructions
                    .Select(instruction => instruction.Operand as MethodReference)
                    .Where(call => call != null && call.DeclaringType.FullName == "DB_Mgr")
                    .Select(call => call.Name)
                    .ToArray();

                var prosperity = Array.IndexOf(calls, "Prosperity_DB_Setting");
                var character = Array.IndexOf(calls, "Character_DB_Setting");
                Assert.True(prosperity >= 0);
                Assert.True(character >= 0);
                Assert.True(prosperity < character);
            }
        }

        [Fact]
        public void CustomIconConsumersStillUseTheSharedSpriteRegistry()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertCallsLoadSprite(module, "CitizenBuff/RefInfo", "GetIcon", false);
                AssertCallsLoadSprite(module, "GetEffect", "GetRefEffect", false);
                AssertCallsLoadSprite(module, "CC_CitizenSlot", "SlotSet", true);
                AssertCallsLoadSprite(module, "CharStatusTab", "TabSet", true);
                AssertCallsLoadSprite(module, "Char_Tooltip", "CharInfoSet", true);
                AssertCallsLoadSprite(module, "CasselGames.UI.AbilityBuffSlotUI", "SetData", true);
                AssertCallsLoadSprite(module, "CasselGames.UI.AbilityStatusCitizenSlotUI", "SetData", true);
            }
        }

        [Fact]
        public void HarmonyAdapterParameterNamesMatchTheInspectedBuild()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath()))
            {
                AssertMethodParameters(module, "CCMake_Info", ".ctor",
                    new[] { "System.Int32", "System.Boolean" }, new[] { "_grade_max", "_religion_check" });
                AssertMethodParameters(module, "T_Citizen", "MakeCtizen_ByCC",
                    new[] { "UnityEngine.Vector2", "CCMake_Info" }, new[] { "pos", "_info" });
                AssertMethodParameters(module, "GBot", "MakeCitizen",
                    new[] { "UnityEngine.Vector2", "System.Int32" }, new[] { "pos", "_index" });
                AssertMethodParameters(module, "GBot", "FatigueUpate",
                    new[] { "System.Single", "System.Boolean" }, new[] { "value", "_effect" });
                AssertMethodParameters(module, "ElecLine_Info", "AddConnectUseBuild",
                    new[] { "System.Int32", "System.Single" }, new[] { "_id", "_value" });
                AssertMethodParameters(module, "ElecLine_Info", "AddWatt",
                    new[] { "System.Single" }, new[] { "_value" });
                AssertMethodParameters(module, "Building", "WireCheck",
                    new[] { "System.Boolean" }, new[] { "_use" });
                AssertMethodParameters(module, "Building_ElecMasonry", "WireCheck",
                    new[] { "System.Boolean" }, new[] { "_use" });
                AssertMethodParameters(module, "Building_ElecCarrierStation", "WireCheck",
                    new[] { "System.Boolean" }, new[] { "_use" });
                AssertMethodParameters(module, "Building_ElecBandstand", "WireCheck",
                    new[] { "System.Boolean" }, new[] { "_use" });
                AssertMethodParameters(module, "BuildingMgr", "GetFourDir_ElecGroup",
                    new[] { "ElecPort", "System.Boolean" }, new[] { "_port", "_overlap_contain" });
                AssertMethodParameters(module, "BuildingMgr", "DeleteConnectCheck",
                    new[] { "System.Int32", "System.Collections.Generic.List`1<ElecPort>" }, new[] { "_id", "_list_port" });
                AssertMethodParameters(module, "ElecLine_Info", "UseWatt",
                    new[] { "System.Int32", "System.Single" }, new[] { "_useid", "_value" });
                AssertMethodParameters(module, "MasonryInfo", "WorkUpdate",
                    new[] { "System.Single" }, new[] { "d_time" });
                AssertMethodParameters(module, "T_Citizen", "ApplyFoodOrLife_ResAbility",
                    new[] { "TileInfo" }, new[] { "t_info" });
                AssertMethodParameters(module, "Helpers", "Get_MaximumGuestNum",
                    new[] { "BuildingName" }, new[] { "_name" });
                AssertMethodParameters(module, "CasselGames.Diplomatic.Data.DiplomaticCountryResourceData", "TradeCountryToMyKingdomPrice",
                    new[] { "System.Single", "System.Int32" }, new[] { "price", "nowRelations" });
                AssertMethodParameters(module, "CasselGames.Diplomatic.Data.DiplomaticCountryResourceData", "TradeMyKingdomToCountryPrice",
                    new[] { "System.Single", "System.Int32" }, new[] { "price", "nowRelations" });
                AssertMethodParameters(module, "CasselGames.Diplomatic.DiplomaticMgr", "OnTradeResultEvent",
                    new[] { "CasselGames.Diplomatic.Data.TradeResult" }, new[] { "result" });
                AssertMethodParameters(module, "CasselGames.Diplomatic.Data.DiplomaticData", "SetTerrainTotalDistance",
                    new[] { "CasselGames.Diplomatic.Data.DiplomaticWorldTerrainEntity" }, new[] { "tInstance" });
                AssertMethodParameters(module, "CasselGames.Diplomatic.UI.DiplomaticTradeSheetDetailContentsUI", "SetData",
                    new[]
                    {
                        "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                        "CasselGames.Diplomatic.Data.DiplomaticCountryTradeSheetData",
                        "CasselGames.Diplomatic.UI.TypeTradeSheetCategory",
                        "CasselGames.Diplomatic.UI.TypeTradeSheet"
                    },
                    new[] { "cData", "sData", "typeCat", "typeTradeSheet" });
                AssertMethodParameters(module, "T_Citizen", "HungerUpdate",
                    new[] { "System.Single" }, new[] { "value" });
                AssertMethodParameters(module, "BuffIcon", "IconSet",
                    new[] { "UnityEngine.Transform", "BuffInfo" }, new[] { "Tf_parent", "_info" });
                AssertMethodParameters(module, "CitizenBuff/RefInfo", "GetIconAddress",
                    new[] { "System.String", "C_Buff_Category" }, new[] { "_RefName", "_category" });
                AssertMethodParameters(module, "CitizenBuff/RefInfo", "Get_T_Name",
                    new[] { "System.String", "C_Buff_Category", "System.Boolean" }, new[] { "_RefName", "_category", "_setColor" });
                AssertMethodParameters(module, "GameUnit", "ClothesUpdate",
                    new[] { "System.Int32", "System.Boolean" }, new[] { "num", "isCombine" });
                AssertMethodParameters(module, "T_Citizen", "BeAttacked",
                    new[] { "System.Single", "Unit_Attacekd_Tag", "System.Int32" }, new[] { "dmg", "_tag", "_id" });
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

            Assert.False(string.IsNullOrWhiteSpace(ratopiaDir));
            var path = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");
            Assert.True(File.Exists(path), $"Assembly-CSharp.dll not found: {path}");
            return path;
        }

        private static void AssertCallsLoadSprite(
            ModuleDefinition module,
            string typeName,
            string methodName,
            bool requireIconCharLiteral)
        {
            var methods = FindType(module, typeName).Methods
                .Where(method => method.Name == methodName && method.HasBody)
                .ToArray();

            Assert.NotEmpty(methods);
            Assert.Contains(methods, method => method.Body.Instructions.Any(instruction =>
                instruction.Operand is MethodReference call &&
                call.DeclaringType.FullName == "Func" &&
                call.Name == "LoadSprite"));

            if (requireIconCharLiteral)
            {
                Assert.Contains(methods, method => method.Body.Instructions.Any(instruction =>
                    instruction.Operand is string text &&
                    text.StartsWith("Icon_Char", StringComparison.Ordinal)));
            }
        }

        private static void AssertField(ModuleDefinition module, string typeName, string fieldName, string fieldType)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == fieldName);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static void AssertMethod(ModuleDefinition module, string typeName, string methodName, string returnType, params string[] parameterTypes)
        {
            var method = FindType(module, typeName).Methods.Single(item =>
                item.Name == methodName &&
                item.ReturnType.FullName == returnType &&
                item.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));
            Assert.NotNull(method);
        }

        private static void AssertMethodParameters(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string[] parameterTypes,
            string[] parameterNames)
        {
            var method = FindType(module, typeName).Methods.Single(item =>
                item.Name == methodName &&
                item.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));

            Assert.Equal(parameterNames, method.Parameters.Select(parameter => parameter.Name).ToArray());
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return FindTypes(module.Types).Single(type => type.FullName == fullName);
        }

        private static System.Collections.Generic.IEnumerable<TypeDefinition> FindTypes(System.Collections.Generic.IEnumerable<TypeDefinition> types)
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
