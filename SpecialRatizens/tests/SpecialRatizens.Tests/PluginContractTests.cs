using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Mono.Cecil;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginIdentityIsStable()
        {
            Assert.Equal("cn.ratopia.specialratizens", Plugin.PluginGuid);
            Assert.Equal("特殊鼠鼠", Plugin.PluginName);
            Assert.Equal("0.1.4", Plugin.PluginVersion);
        }

        [Fact]
        public void DataRootIsResolvedBesideThePluginAssembly()
        {
            var location = Path.Combine("X:\\mods\\RenamedFolder", "SpecialRatizens.dll");

            Assert.Equal(
                Path.Combine("X:\\mods\\RenamedFolder", "Data"),
                PluginDataPaths.ResolveDataRoot(location));
        }

        [Fact]
        public void PatchRegistryContainsOnlyTheSpecialRatizensWhitelist()
        {
            var source = File.ReadAllText(Path.Combine(
                GetProjectRoot(), "src", "SpecialRatizens", "Patching", "PatchRegistry.cs"));
            var names = Regex.Matches(source, "(?:Prefix|Postfix)\\(\\\"([^\\\"]+)\\\"")
                .Cast<Match>()
                .Select(match => match.Groups[1].Value)
                .ToArray();

            var expected = new[]
            {
                "data.character-db",
                "session.loaded",
                "generation.list",
                "generation.candidate-constructor",
                "generation.default-trait-boundary",
                "generation.citizen-created",
                "power.robot-created",
                "power.robot-fatigue",
                "power.connect-building",
                "power.add-watt",
                "power.wire-check-building",
                "power.wire-check-masonry",
                "power.wire-check-carrier",
                "power.wire-check-bandstand",
                "power.four-direction-grid",
                "power.delete-connect",
                "power.quantum-grid",
                "industry.work-prefix",
                "industry.work-postfix",
                "industry.food-life",
                "industry.guest-capacity",
                "economy.import-price",
                "economy.export-price",
                "economy.trade-result",
                "economy.distance",
                "economy.agreement-count",
                "economy.detail-price",
                "citizen.job",
                "combat.sword-attack",
                "combat.citizen-attacked",
                "state.food-total",
                "state.pdi",
                "state.hunger",
                "state.buff-icon",
                "state.icon-address",
                "state.display-name",
                "state.description",
                "appearance.default-clothes",
                "appearance.work-clothes"
            };

            Assert.Equal(expected, names);
            Assert.Contains("data.character-db", names);
            Assert.Contains("generation.candidate-constructor", names);
            Assert.Contains("combat.citizen-attacked", names);
            Assert.Contains("power.quantum-grid", names);
            Assert.Contains("appearance.default-clothes", names);
            Assert.Contains("session.loaded", names);
            Assert.DoesNotContain(names, item => item.Contains("warehouse"));
            Assert.DoesNotContain(names, item => item.Contains("utopia"));
            Assert.DoesNotContain(names, item => item.Contains("queen"));
            Assert.Contains("typeof(LegacyPatchAdapters)", source);
            Assert.DoesNotContain("typeof(CustomMOD)", source);
        }

        [Fact]
        public void BuiltAssemblyHasExactlyOneBepInPluginEntry()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var pluginTypes = module.Types
                    .Where(type => type.CustomAttributes.Any(attribute =>
                        attribute.AttributeType.FullName == "BepInEx.BepInPlugin"))
                    .Select(type => type.FullName)
                    .ToArray();

                Assert.Equal(new[] { "SpecialRatizens.Plugin" }, pluginTypes);

                var dormantHarmonyAttributes = module.Types
                    .SelectMany(type => type.Methods)
                    .SelectMany(method => method.CustomAttributes)
                    .Where(attribute => attribute.AttributeType.Namespace == "HarmonyLib")
                    .Select(attribute => attribute.AttributeType.FullName)
                    .ToArray();

                Assert.Empty(dormantHarmonyAttributes);
            }
        }

        [Fact]
        public void LegacyAdapterSurfaceMatchesTheAuditedSpecialFeatureHooks()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var actual = module.Types
                    .Single(type => type.FullName == "SpecialRatizens.Patching.LegacyPatchAdapters")
                    .Methods
                    .Where(method => method.IsPublic && method.IsStatic)
                    .Select(method => method.Name)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToArray();
                var expected = new[]
                {
                    "DB_Mgr_Character_DB_Setting",
                    "CitizenCaveUI_MakeCitizenList",
                    "CCMake_Info",
                    "CCMake_Info_MakeCharacterList",
                    "T_Citizen_MakeCtizen_ByCC",
                    "GBot_MakeCitizen",
                    "GBot_FatigueUpate",
                    "ElecLine_Info_AddConnectUseBuild",
                    "ElecLine_Info_AddWatt",
                    "Building_WireCheck",
                    "Building_ElecMasonry_WireCheck",
                    "Building_ElecCarrierStation_WireCheck",
                    "Building_ElecBandstand_WireCheck",
                    "BuildingMgr_GetFourDir_ElecGroup",
                    "BuildingMgr_DeleteConnectCheck",
                    "ElecLine_Info_UseWatt",
                    "MasonryInfo_WorkUpdate_Prefix",
                    "MasonryInfo_WorkUpdate_Postfix",
                    "T_Citizen_ApplyFoodOrLife_ResAbility",
                    "Helpers_Get_MaximumGuestNum",
                    "DiplomaticCountryResourceData_TradeCountryToMyKingdomPrice",
                    "DiplomaticCountryResourceData_TradeMyKingdomToCountryPrice",
                    "DiplomaticMgr_OnTradeResultEvent_BGNYQY",
                    "DiplomaticData_SetTerrainTotalDistance",
                    "DiplomaticCountryData_MaxTradeAgreementCount",
                    "DiplomaticTradeSheetDetailContentsUI_SetData",
                    "T_Citizen_JobSet",
                    "T_Citizen_SwdAtk_Call",
                    "T_Citizen_BeAttacked",
                    "FoodUI_AllFood_Update",
                    "GameUnit_UpdatePDI_Post",
                    "T_Citizen_HungerUpdate",
                    "BuffIcon_IconSet",
                    "RefInfo_GetIconAddress",
                    "RefInfo_Get_T_Name",
                    "CitizenBuff_RefInfo_GetDescript",
                    "T_Citizen_DefaultClothesUpdate",
                    "GameUnit_ClothesUpdate"
                }.OrderBy(item => item, StringComparer.Ordinal).ToArray();

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ProjectCannotInstallAfterBuildAndUsesBepInExFiveAssembly()
        {
            var project = File.ReadAllText(Path.Combine(GetProjectRoot(), "src", "SpecialRatizens", "SpecialRatizens.csproj"));

            Assert.Contains("BepInEx.dll", project);
            Assert.DoesNotContain("BepInEx.Core.dll", project);
            Assert.DoesNotContain("InstallPlugin", project);
            Assert.DoesNotContain("AfterTargets=\"Build\"", project);
        }

        [Fact]
        public void LegacyCoreCannotSelfRegisterAndUsesTheTestedSelectionEngine()
        {
            var legacy = File.ReadAllText(Path.Combine(
                GetProjectRoot(), "src", "SpecialRatizens", "Legacy", "CustomMOD.cs"));

            Assert.DoesNotContain("[BepInPlugin", legacy);
            Assert.DoesNotContain("CreateAndPatchAll", legacy);
            Assert.DoesNotMatch(new Regex(@"(?m)^\s*\[Harmony"), legacy);
            Assert.Contains("SpecialSelectionEngine.Select", legacy);
        }

        private static string GetProjectRoot()
        {
            return typeof(PluginContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
