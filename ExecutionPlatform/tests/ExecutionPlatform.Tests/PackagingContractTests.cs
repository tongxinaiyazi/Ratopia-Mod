using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class PackagingContractTests
    {
        [Fact]
        public void GameAndLoaderReferencesAreNeverCopiedToPluginOutput()
        {
            var projectPath = Path.Combine(
                ContractTestPaths.ProjectRoot,
                "src",
                "ExecutionPlatform",
                "ExecutionPlatform.csproj");
            var project = XDocument.Load(projectPath);
            var forbiddenReferences = new[]
            {
                "Assembly-CSharp",
                "BepInEx",
                "0Harmony",
                "UnityEngine",
                "UnityEngine.CoreModule",
                "UnityEngine.AnimationModule"
            };

            foreach (var name in forbiddenReferences)
            {
                var reference = project.Descendants("Reference")
                    .Single(element => string.Equals((string)element.Attribute("Include"), name, StringComparison.Ordinal));
                var copyLocal = (string)reference.Attribute("Private") ?? reference.Element("Private")?.Value;
                Assert.Equal("false", copyLocal);
            }
        }

        [Fact]
        public void ProjectBuildCannotCopyDirectlyIntoTheLiveGame()
        {
            var projectPath = Path.Combine(
                ContractTestPaths.ProjectRoot,
                "src",
                "ExecutionPlatform",
                "ExecutionPlatform.csproj");
            var project = XDocument.Load(projectPath);
            Assert.DoesNotContain(project.Descendants("Target"),
                element => (string)element.Attribute("Name") == "InstallPlugin");
        }

        [Fact]
        public void InstallerRequiresExitBackupAndHashVerification()
        {
            var scriptPath = Path.Combine(ContractTestPaths.ProjectRoot, "scripts", "Install.ps1");
            var script = File.ReadAllText(scriptPath);

            Assert.Contains("Get-Process", script);
            Assert.Contains("TestSavePath", script);
            Assert.Contains("backups", script);
            Assert.Contains("Copy-Item", script);
            Assert.Contains("Get-FileHash", script);
            Assert.Contains("SHA-256", script);
        }

        [Fact]
        public void PackageScriptLocksGameVersionAndWhitelistsExactlyTwoFiles()
        {
            var scriptPath = Path.Combine(ContractTestPaths.ProjectRoot, "scripts", "Package.ps1");
            var script = File.ReadAllText(scriptPath);

            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", script);
            Assert.Contains("/p:InstallAfterBuild=false", script);
            Assert.Contains("BepInEx/plugins/ExecutionPlatform/ExecutionPlatform.dll", script);
            Assert.Contains("README.md", script);
            Assert.Contains("Test-RatopiaPackage.ps1", script);
            Assert.Contains("ExecutionPlatform-v0.1.1-BepInEx5.zip", script);
        }

        [Fact]
        public void SourceTreeContainsNoPackagedGameAssets()
        {
            var source = Path.Combine(ContractTestPaths.ProjectRoot, "src", "ExecutionPlatform");
            var forbiddenExtensions = new[] { ".png", ".jpg", ".jpeg", ".asset", ".prefab", ".bundle" };
            var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories)
                .Where(path => forbiddenExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));

            Assert.Empty(files);
        }
    }
}
