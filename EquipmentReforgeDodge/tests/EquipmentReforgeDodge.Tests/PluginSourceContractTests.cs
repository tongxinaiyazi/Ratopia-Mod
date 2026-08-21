using System;
using System.IO;
using System.Linq;
using Xunit;

namespace EquipmentReforgeDodge.Tests
{
    /// <summary>
    /// 校验插件源代码结构：补丁类型必须存在且被 Plugin 安装，防止重构后漏装补丁。
    /// </summary>
    public sealed class PluginSourceContractTests
    {
        private static string SourceDirectory =>
            Path.Combine(ContractTestPaths.ProjectRoot, "src", "EquipmentReforgeDodge");

        [Fact]
        public void Plugin_installs_every_patch_type()
        {
            var pluginSource = File.ReadAllText(Path.Combine(SourceDirectory, "Plugin.cs"));
            var patchTypes = Directory
                .EnumerateFiles(Path.Combine(SourceDirectory, "Patches"), "*.cs")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            Assert.NotEmpty(patchTypes);
            foreach (var patchType in patchTypes)
            {
                Assert.Contains($"typeof({patchType})", pluginSource, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void Reforge_values_match_the_design_contract()
        {
            var configSource = File.ReadAllText(
                Path.Combine(SourceDirectory, "Core", "DodgeReforgeConfig.cs"));

            Assert.Contains("Tier1DodgePercent = 20f", configSource, StringComparison.Ordinal);
            Assert.Contains("Tier2DodgePercent = 30f", configSource, StringComparison.Ordinal);
        }

        [Fact]
        public void Injector_only_targets_the_accessory_database()
        {
            var injectorSource = File.ReadAllText(
                Path.Combine(SourceDirectory, "Core", "AccessoryEnhanceInjector.cs"));

            Assert.Contains("List_AccessoryDB", injectorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("List_WeaponDB", injectorSource, StringComparison.Ordinal);
            Assert.DoesNotContain("List_ClothesDB", injectorSource, StringComparison.Ordinal);
        }
    }
}
