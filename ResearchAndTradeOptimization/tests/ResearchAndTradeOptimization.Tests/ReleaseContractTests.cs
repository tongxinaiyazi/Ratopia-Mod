using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Xml.Linq;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class ReleaseContractTests
    {
        private static readonly string ProjectRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

        [Fact]
        public void ProjectDefaultsToNoInstallAndNeverCopiesRuntimeDependencies()
        {
            var projectPath = Path.Combine(
                ProjectRoot,
                "src",
                "ResearchAndTradeOptimization",
                "ResearchAndTradeOptimization.csproj");
            var project = XDocument.Load(projectPath);
            var properties = project.Descendants("PropertyGroup").Descendants().ToArray();

            Assert.Contains(properties, property =>
                property.Name.LocalName == "InstallAfterBuild" && property.Value == "false");

            var requiredReferences = new[]
            {
                "Assembly-CSharp",
                "BepInEx",
                "0Harmony",
                "UnityEngine",
                "UnityEngine.CoreModule",
                "UnityEngine.UI",
                "UnityEngine.UIModule",
                "Unity.TextMeshPro"
            };
            foreach (var referenceName in requiredReferences)
            {
                var reference = project.Descendants("Reference").Single(item =>
                    (string)item.Attribute("Include") == referenceName);
                Assert.Equal("false", (string)reference.Attribute("Private"));
            }
        }

        [Fact]
        public void ChineseReadmeDocumentsInstallCompatibilitySaveRiskAndLogs()
        {
            var readme = ReadRequiredFile("README.md");

            Assert.Contains("研究与贸易优化", readme);
            Assert.Contains("BepInEx 5.4.23.5", readme);
            Assert.Contains("Harmony 2.9.0", readme);
            Assert.Contains(".NET Framework 4.7.2", readme);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", readme);
            Assert.Contains("BepInEx\\plugins\\ResearchAndTradeOptimization", readme);
            Assert.Contains("卸载", readme);
            Assert.Contains("存档", readme);
            Assert.Contains("测试存档副本", readme);
            Assert.Contains("BepInEx\\LogOutput.log", readme);
            Assert.Contains("特殊鼠鼠", readme);
            Assert.Contains("完整商品池", readme);
            Assert.Contains("全局公共池", readme);
            Assert.Contains("最多三行", readme);
            Assert.Contains("18", readme);
            Assert.Contains("紧凑商品格", readme);
            Assert.Contains("52×52", readme);
            Assert.Contains("新增贸易", readme);
            Assert.Contains("保持原位", readme);
            Assert.DoesNotContain("下方方向和贸易控件会按原版布局整体下移", readme);
            Assert.DoesNotContain("... (+N)", readme);
            Assert.Contains("无限期", readme);
            Assert.Contains("12", readme);
            Assert.Contains("瓦特资源（4001）", readme);
        }

        [Fact]
        public void ReleaseScriptsAndTestChecklistExist()
        {
            var packageScript = ReadRequiredFile("scripts", "Package.ps1");
            Assert.Contains("InstallAfterBuild=false", packageScript);
            Assert.Contains("研究与贸易优化-v0.3.0-BepInEx5.zip", packageScript);
            Assert.Contains("ResearchAndTradeOptimization.dll", packageScript);

            var testing = ReadRequiredFile("docs", "TESTING.md");
            Assert.Contains("普通研究", testing);
            Assert.Contains("工程研究", testing);
            Assert.Contains("教义研究", testing);
            Assert.Contains("8", testing);
            Assert.Contains("特殊鼠鼠", testing);
            Assert.Contains("保存", testing);
            Assert.Contains("移除本 Mod", testing);
            Assert.Contains("调整", testing);
            Assert.Contains("无限期", testing);
            Assert.Contains("第 12、24、36", testing);
            Assert.Contains("13–17", testing);
            Assert.Contains("8–10", testing);
            Assert.Contains("最多三行", testing);
            Assert.Contains("52×52", testing);
            Assert.Contains("新增贸易", testing);
            Assert.Contains("保持原位", testing);
            Assert.DoesNotContain("... (+N)", testing);

            Assert.True(Directory.Exists(Path.Combine(ProjectRoot, "dist")));
            Assert.True(Directory.Exists(Path.Combine(ProjectRoot, "docs", "superpowers", "specs")));
            Assert.True(Directory.Exists(Path.Combine(ProjectRoot, "docs", "superpowers", "plans")));
        }

        [Fact]
        public void VersionedReleaseArchiveContainsOnlyThePluginAndReadmeWhenPresent()
        {
            var archivePath = Path.Combine(
                ProjectRoot,
                "dist",
                "研究与贸易优化-v0.3.0-BepInEx5.zip");
            if (!File.Exists(archivePath))
            {
                return;
            }

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var files = archive.Entries
                    .Where(entry => !string.IsNullOrEmpty(entry.Name))
                    .Select(entry => entry.FullName.Replace('\\', '/'))
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();

                Assert.Equal(
                    new[]
                    {
                        "BepInEx/plugins/ResearchAndTradeOptimization/ResearchAndTradeOptimization.dll",
                        "README.md"
                    },
                    files);
                Assert.DoesNotContain(files, path =>
                    path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("0Harmony.dll", StringComparison.OrdinalIgnoreCase) ||
                    path.IndexOf("SaveFile", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    path.EndsWith("LogOutput.log", StringComparison.OrdinalIgnoreCase));

                var pluginEntry = archive.GetEntry(
                    "BepInEx/plugins/ResearchAndTradeOptimization/ResearchAndTradeOptimization.dll");
                Assert.NotNull(pluginEntry);
                var builtPluginPath = Path.Combine(
                    ProjectRoot,
                    "src",
                    "ResearchAndTradeOptimization",
                    "bin",
                    "Release",
                    "net472",
                    "ResearchAndTradeOptimization.dll");
                Assert.True(File.Exists(builtPluginPath), $"Built plugin not found: {builtPluginPath}");
                using (var packagedPlugin = pluginEntry.Open())
                using (var builtPlugin = File.OpenRead(builtPluginPath))
                {
                    Assert.Equal(ComputeSha256(builtPlugin), ComputeSha256(packagedPlugin));
                }
            }
        }

        private static string ComputeSha256(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static string ReadRequiredFile(params string[] relativeParts)
        {
            var parts = new[] { ProjectRoot }.Concat(relativeParts).ToArray();
            var path = Path.Combine(parts);
            Assert.True(File.Exists(path), $"Required release file not found: {path}");
            return File.ReadAllText(path);
        }
    }
}
