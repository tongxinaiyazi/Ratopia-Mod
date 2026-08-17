using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace UnlimitedTradeAgreements.Tests
{
    public sealed class ReleaseContractTests
    {
        [Fact]
        public void ProjectDefaultsToNoInstallAndDoesNotCopyRuntimeReferences()
        {
            var path = TestPaths.RequireFile(Path.Combine(
                TestPaths.ProjectRoot,
                "src",
                "UnlimitedTradeAgreements",
                "UnlimitedTradeAgreements.csproj"));
            var project = XDocument.Load(path);
            var properties = project.Descendants("PropertyGroup").Descendants().ToArray();
            Assert.Contains(properties, property =>
                property.Name.LocalName == "InstallAfterBuild" && property.Value == "false");

            foreach (var name in new[]
                     {
                         "Assembly-CSharp", "BepInEx", "0Harmony",
                         "UnityEngine.CoreModule", "UnityEngine.UI", "Unity.TextMeshPro"
                     })
            {
                var reference = project.Descendants("Reference").Single(item =>
                    (string)item.Attribute("Include") == name);
                Assert.Equal("false", (string)reference.Attribute("Private"));
            }
        }

        [Fact]
        public void ReadmeDocumentsScopeInstallConflictSaveRemovalAndLogs()
        {
            var text = File.ReadAllText(TestPaths.RequireFile(
                Path.Combine(TestPaths.ProjectRoot, "README.md")));
            foreach (var required in new[]
                     {
                         "只解除贸易协议数量限制", "安装", "研究与贸易优化",
                         "存档", "卸载", "BepInEx\\LogOutput.log"
                     })
            {
                Assert.Contains(required, text);
            }
        }

        [Fact]
        public void PackageScriptPinsVersionHashAndExactStagePath()
        {
            var text = File.ReadAllText(TestPaths.RequireFile(Path.Combine(
                TestPaths.ProjectRoot,
                "scripts",
                "Package.ps1")));
            Assert.Contains("贸易站去除最大队列限制-v0.1.0-BepInEx5.zip", text);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", text);
            Assert.Contains("BepInEx\\plugins\\UnlimitedTradeAgreements", text);
            Assert.Contains("InstallAfterBuild=false", text);
        }

        [Fact]
        public void ReleaseArchiveContainsOnlyPluginAndReadmeWhenPresent()
        {
            var archivePath = Path.Combine(
                TestPaths.ProjectRoot,
                "dist",
                "贸易站去除最大队列限制-v0.1.0-BepInEx5.zip");
            if (!File.Exists(archivePath))
            {
                return;
            }

            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var files = archive.Entries
                    .Where(entry => !string.IsNullOrEmpty(entry.Name))
                    .Select(entry => entry.FullName.Replace('\\', '/'))
                    .OrderBy(item => item)
                    .ToArray();
                Assert.Equal(new[]
                {
                    "BepInEx/plugins/UnlimitedTradeAgreements/UnlimitedTradeAgreements.dll",
                    "README.md"
                }, files);
                Assert.DoesNotContain(files, path =>
                    path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("0Harmony.dll", StringComparison.OrdinalIgnoreCase) ||
                    path.IndexOf("UnityEngine", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    path.IndexOf("BepInEx.dll", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }
    }
}
