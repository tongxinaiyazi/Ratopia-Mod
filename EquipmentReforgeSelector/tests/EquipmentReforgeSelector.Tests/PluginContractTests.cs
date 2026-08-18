using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void Production_project_uses_overridable_noncopying_Ratopia_references()
        {
            var project = XDocument.Load(ContractTestPaths.ProductionFile("EquipmentReforgeSelector.csproj"));
            var ratopiaDir = project.Descendants("RatopiaDir").Single();
            Assert.Contains("$(RatopiaDir)", project.ToString());
            Assert.Contains("Condition", ratopiaDir.Attributes().Select(attribute => attribute.Name.LocalName));

            var references = project.Descendants("Reference").ToArray();
            Assert.NotEmpty(references);
            Assert.All(references, reference => Assert.Equal("false", reference.Element("Private")?.Value));
            Assert.Contains(references, reference => reference.Attribute("Include")?.Value == "Assembly-CSharp");
            Assert.Contains(references, reference => reference.Attribute("Include")?.Value == "BepInEx");
            Assert.Contains(references, reference => reference.Attribute("Include")?.Value == "0Harmony");
        }

        [Fact]
        public void Plugin_identity_and_failure_atomic_patch_lifecycle_are_fixed()
        {
            var source = ReadRequiredProductionFile("Plugin.cs");
            Assert.Contains("装备重铸自选属性", source);
            Assert.Contains("cn.ratopia.equipmentreforgeselector", source);
            Assert.Contains("0.1.2", source);
            Assert.Contains("OrderBy(type => type.FullName", source);
            Assert.Contains("CreateClassProcessor", source);
            Assert.Contains("UnpatchSelf", source);
            Assert.Contains("RuntimeController.Disable", source);
            Assert.Contains("typeof(SimpleToolTipPatch)", source);
            Assert.Contains("OnDestroy", source);
            Assert.Contains("HideAndDontSave", source);
            Assert.Contains("DontDestroyOnLoad", source);
        }

        [Fact]
        public void Patch_contracts_cover_open_filtering_and_reference_safe_enhance_cleanup()
        {
            var openPatch = ReadRequiredProductionFile("ItemDetailOpenPatch.cs");
            Assert.Contains("ItemDetail_Open", openPatch);
            Assert.Contains("RuntimeEligibility.ShouldShow", openPatch);
            Assert.Contains("private static void Prefix(BuildMidUI __instance, out bool __state)", openPatch);
            Assert.Contains("__instance.Obj_Main.activeInHierarchy", openPatch);
            Assert.Contains("RuntimeController.Open(__instance, _info, _level, __state)", openPatch);
            Assert.Contains("Postfix", openPatch);

            var enhancePatch = ReadRequiredProductionFile("ItemEnhancePatch.cs");
            Assert.Contains("ItemEnhance", enhancePatch);
            Assert.Contains("HarmonyPriority(Priority.Last)", enhancePatch);
            Assert.Contains("ref OverrideState __state", enhancePatch);
            Assert.Contains("Postfix", enhancePatch);
            Assert.Contains("Finalizer", enhancePatch);
            Assert.Contains("return __exception", enhancePatch);
            Assert.Contains("Dispose", enhancePatch);

            var tooltipPatch = ReadRequiredProductionFile("SimpleToolTipPatch.cs");
            Assert.Contains("SimpleToolTipSet", tooltipPatch);
            Assert.Contains("SimpleToolTipList.EnhanceEffect", tooltipPatch);
            Assert.Contains("___m_EffectFrame", tooltipPatch);
            Assert.Contains("OpenInlineSelector", tooltipPatch);
        }

        [Fact]
        public void Runtime_UI_contract_reuses_vanilla_effect_rows_and_has_no_side_panel()
        {
            var source = ReadRequiredProductionFile("InlineReforgeSelectorView.cs");
            Assert.Contains("frame.Txt_Value", source);
            Assert.Contains("InlineCandidatePlan.Create", source);
            Assert.Contains("Helpers.GetToolTipString", source);
            Assert.Contains("FS_P_Right_White", source);
            Assert.Contains("AccessTools.TypeByName(\"Defines\")", source);
            Assert.Contains("Hex_DeepGreen", source);
            Assert.Contains("#1E8A00", source);
            Assert.Contains("Navigation.Mode.Explicit", source);
            Assert.Contains("selectOnUp", source);
            Assert.Contains("selectOnDown", source);
            Assert.Contains("RuntimeController.SelectCandidate(candidateIndex)", source);
            Assert.DoesNotContain("SetSelectedGameObject", source);
            Assert.DoesNotContain(".Select()", source);

            var productionText = string.Join(
                "\n",
                Directory.GetFiles(
                        Path.Combine(ContractTestPaths.RepositoryRoot, "src", "EquipmentReforgeSelector"),
                        "*.cs")
                    .Select(File.ReadAllText));
            Assert.DoesNotContain("EquipmentReforgeSelectorPanel", productionText);
            Assert.DoesNotContain("anchorMin = new Vector2(1f, 0.5f)", productionText);

            var controller = ReadRequiredProductionFile("RuntimeController.cs");
            var coordinator = ReadRequiredProductionFile("PanelStateCoordinator.cs");
            var session = ReadRequiredProductionFile("SelectionSession.cs");
            Assert.Contains("SelectCandidate(int candidateIndex)", controller);
            Assert.Contains("PanelState.TrySelect(candidateIndex)", controller);
            Assert.Contains("TrySelect(int candidateIndex)", coordinator);
            Assert.Contains("_session.TrySelect(candidateIndex, Candidates)", coordinator);
            Assert.Contains("TrySelect(int candidateIndex, IReadOnlyList<ReforgeCandidate> candidates)", session);
            Assert.Contains("previousSelection == candidates[index]", session);
        }

        [Fact]
        public void Inline_view_lifecycle_disables_and_reuses_owned_buttons_without_deferred_destroy_races()
        {
            var panel = ReadRequiredProductionFile("InlineReforgeSelectorView.cs");
            var controller = ReadRequiredProductionFile("RuntimeController.cs");

            Assert.Contains("void OnDisable()", panel);
            Assert.Contains("RuntimeController.ViewDisabled(this)", panel);
            Assert.Contains("InlineReforgeButton", panel);
            Assert.Contains("button.interactable = false", panel);
            Assert.Contains("button.enabled = false", panel);
            Assert.Contains("button.onClick.RemoveAllListeners()", panel);
            Assert.DoesNotContain("Destroy(button)", panel);
            Assert.DoesNotContain("Destroy(gameObject)", panel);
            Assert.Contains("void ViewDisabled(InlineReforgeSelectorView view)", controller);
        }

        [Fact]
        public void Tooltip_refresh_suspends_only_the_view_and_completed_reforge_resets_stale_selection()
        {
            var tooltipPatch = ReadRequiredProductionFile("SimpleToolTipPatch.cs");
            var controller = ReadRequiredProductionFile("RuntimeController.cs");

            Assert.Contains("RuntimeController.SuspendInlineSelector()", tooltipPatch);
            Assert.DoesNotContain("RuntimeController.CloseInlineSelector()", tooltipPatch);
            Assert.Contains("public static void SuspendInlineSelector()", controller);
            Assert.Contains("PanelState.Detach(view)", controller);
            Assert.Contains("PanelState.ResetSession()", controller);
            Assert.Contains("IsDetailHostActive()", controller);
        }

        [Fact]
        public void Inline_candidates_use_full_width_hit_areas_and_visible_number_shortcuts()
        {
            var view = ReadRequiredProductionFile("InlineReforgeSelectorView.cs");

            Assert.Contains("EquipmentReforgeSelectorHitArea", view);
            Assert.Contains("AddComponent<Image>()", view);
            Assert.Contains("AddComponent<LayoutElement>()", view);
            Assert.Contains("ignoreLayout = true", view);
            Assert.Contains("anchorMin = new Vector2(0f", view);
            Assert.Contains("anchorMax = new Vector2(1f", view);
            Assert.Contains("text.raycastTarget = false", view);
            Assert.Contains("KeyCode.Alpha1", view);
            Assert.Contains("KeyCode.Keypad1", view);
            Assert.Contains("CandidateShortcut.TryResolveDigit", view);
            Assert.Contains("（已选择）", view);
        }

        [Fact]
        public void Deferred_old_view_destruction_cannot_disable_a_rebound_row()
        {
            var view = ReadRequiredProductionFile("InlineReforgeSelectorView.cs");

            Assert.Contains("_buttonsDeactivated", view);
            Assert.Contains("if (_buttonsDeactivated)", view);
            Assert.Contains("_buttonsDeactivated = true", view);
        }

        [Fact]
        public void Production_output_contains_no_forbidden_runtime_dependency_copies()
        {
            var output = Path.Combine(ContractTestPaths.RepositoryRoot, "src", "EquipmentReforgeSelector", "bin", "Release", "net472");
            var forbidden = Directory.GetFiles(output, "*.dll")
                .Select(Path.GetFileName)
                .Where(name => !string.Equals(name, "EquipmentReforgeSelector.dll", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.Empty(forbidden);
        }

        private static string ReadRequiredProductionFile(string name)
        {
            var path = ContractTestPaths.ProductionFile(name);
            Assert.True(File.Exists(path), $"Required production file is missing: {path}");
            return File.ReadAllText(path);
        }
    }
}
