using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class PluginSourceContractTests
    {
        [Fact]
        public void Plugin_identity_and_failure_atomic_lifecycle_are_fixed()
        {
            var source = ReadProductionFile("Plugin.cs");
            Assert.Contains("超级弓箭", source);
            Assert.Contains("cn.ratopia.superbow", source);
            Assert.Contains("0.1.2", source);
            Assert.Contains("HideAndDontSave", source);
            Assert.Contains("DontDestroyOnLoad", source);
            Assert.Contains("OrderBy(type => type.FullName", source);
            Assert.Contains("CreateClassProcessor", source);
            Assert.Contains("typeof(DamageDisplayPatch)", source);
            Assert.Contains("UnpatchSelf", source);
            Assert.Contains("OnDestroy", source);
            Assert.DoesNotContain("PatchAll", source);
        }

        [Fact]
        public void Runtime_sources_cover_catalog_tooltips_hit_processing_and_stable_tick()
        {
            var catalog = ReadProductionFile(Path.Combine("Runtime", "RuntimeCatalog.cs"));
            Assert.Contains("QueenBowIndex", catalog);
            Assert.Contains("NobleSwordIndex", catalog);
            Assert.Contains("PairedListAppendPatch<Res_Ability>", catalog);
            Assert.Contains("ListValuePatch", catalog);
            Assert.Contains("Dispose", catalog);
            Assert.Contains("SetReforgeContextSafely", catalog);
            Assert.Contains("ClearReforgeContext", catalog);

            var reforgeContextPatches = ReadProductionFile(
                Path.Combine("Patches", "ReforgeContextPatches.cs"));
            Assert.Contains("BuildMidUI", reforgeContextPatches);
            Assert.Contains("ItemDetail_Open", reforgeContextPatches);
            Assert.Contains("T_Queen", reforgeContextPatches);
            Assert.Contains("ItemEnhance", reforgeContextPatches);
            Assert.Contains("RuntimeCatalog.SetReforgeContextSafely", reforgeContextPatches);
            Assert.Contains("HarmonyPriority(Priority.First)", reforgeContextPatches);

            var databasePatches = ReadProductionFile(Path.Combine("Patches", "DatabasePatches.cs"));
            Assert.Contains("Item_DB_Setting", databasePatches);
            Assert.Contains("ItemEnhance_DB_Setting", databasePatches);

            var bowPatch = ReadProductionFile(Path.Combine("Patches", "BowArrowHitPatch.cs"));
            Assert.Contains("OnTriggerEnter2D", bowPatch);
            Assert.Contains("___m_Master", bowPatch);
            Assert.Contains("___m_Dmg", bowPatch);
            Assert.Contains("___IsHit", bowPatch);
            Assert.Contains("ref BowHitState __state", bowPatch);
            Assert.True(
                bowPatch.IndexOf("ReportArrowPatchInvocation", StringComparison.Ordinal) <
                bowPatch.IndexOf("if (___IsHit", StringComparison.Ordinal),
                "原始箭矢碰撞日志必须在装备和命中过滤前记录。");
            Assert.Contains("ReportSupportedHit", bowPatch);
            Assert.Contains("HitConfirmation.DidTakeDamage", bowPatch);

            var hitState = ReadProductionFile(Path.Combine("Runtime", "BowHitState.cs"));
            Assert.Contains("RuntimeCombatTarget Target", hitState);
            Assert.Contains("HealthBeforeVanilla", hitState);

            var targets = ReadProductionFile(Path.Combine("Runtime", "RuntimeCombatTarget.cs"));
            Assert.Contains("CombatTargetKind.GameUnit", targets);
            Assert.Contains("CombatTargetKind.AnimalBody", targets);
            Assert.Contains("CombatTargetKind.MapObject", targets);
            Assert.Contains("CombatTargetKind.Building", targets);
            Assert.Contains("Helpers.GetGameUnitByCollision", targets);
            Assert.Contains("GetComponent<AnimalBody>", targets);
            Assert.Contains("GetComponent<MapObj>", targets);
            Assert.Contains("GetComponent<Building>", targets);
            Assert.Contains("BuildingName.EnemyNexus", targets);
            Assert.Contains("EnumerateSplashCandidates", targets);
            Assert.Contains("List_AllEnemy", targets);
            Assert.Contains("List_Animal", targets);
            Assert.Contains("List_MapObj", targets);
            Assert.Contains("List_Building", targets);

            var tooltipPatches = ReadProductionFile(Path.Combine("Patches", "TooltipPatches.cs"));
            Assert.Contains("GetToolTipString", tooltipPatches);
            Assert.Contains("GetToolTipString2", tooltipPatches);
            Assert.Contains("TooltipRules.IsBleedMarker", tooltipPatches);

            var tickPatch = ReadProductionFile(Path.Combine("Patches", "RuntimeTickPatch.cs"));
            Assert.Contains("typeof(T_Queen)", tickPatch);
            Assert.Contains("\"Update\"", tickPatch);
            Assert.Contains("Time.time", tickPatch);
            Assert.Contains("RuntimeCatalog.TryApplySafely", tickPatch);

            var damageDisplayPatch = ReadProductionFile(
                Path.Combine("Patches", "DamageDisplayPatch.cs"));
            Assert.Contains("typeof(DmgEffect)", damageDisplayPatch);
            Assert.Contains("SetDmgEffect", damageDisplayPatch);
            Assert.Contains("ref int __0", damageDisplayPatch);
            Assert.Contains("DamageDisplayRuntime.TryGetOverride", damageDisplayPatch);

            var combat = ReadProductionFile(Path.Combine("Runtime", "CombatRuntime.cs"));
            Assert.Contains("_hitEffectsEnabled = false", combat);
            Assert.Contains("_bleedTicksEnabled = false", combat);
            Assert.Contains("ReportCaptureFailure", combat);
            Assert.Contains("BleedTracker<RuntimeCombatTarget>", combat);
            Assert.Contains("target.ApplyDamage", combat);
            Assert.Contains("EnumerateSplashCandidates", combat);
            Assert.Contains("Bleed.ApplyOrRefresh(candidate, Time.time)", combat);
            Assert.Contains("BleedDamageRules.CalculateExact", combat);
            Assert.Contains("DamageDisplayRuntime.Override", combat);

            Assert.Contains("_enabled = false", catalog);
        }

        [Fact]
        public void Project_references_are_overridable_and_never_copied()
        {
            var projectPath = Path.Combine(
                ContractTestPaths.ProjectRoot, "src", "SuperBow", "SuperBow.csproj");
            var project = XDocument.Load(projectPath);
            var ratopiaDir = project.Descendants("RatopiaDir").Single();
            Assert.NotNull(ratopiaDir.Attribute("Condition"));

            var references = project.Descendants("Reference").ToArray();
            Assert.NotEmpty(references);
            Assert.All(references, reference =>
                Assert.Equal("false", reference.Attribute("Private")?.Value ??
                                     reference.Element("Private")?.Value));
            Assert.Contains(references, reference => reference.Attribute("Include")?.Value == "Assembly-CSharp");
            Assert.Contains(references, reference => reference.Attribute("Include")?.Value == "BepInEx");
            Assert.Contains(references, reference => reference.Attribute("Include")?.Value == "0Harmony");
        }

        private static string ReadProductionFile(string relativePath)
        {
            var path = Path.Combine(ContractTestPaths.ProjectRoot, "src", "SuperBow", relativePath);
            Assert.True(File.Exists(path), $"Required production file is missing: {path}");
            return File.ReadAllText(path);
        }
    }
}
