using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class PackagingContractTests
    {
        [Fact]
        public void PackagingScriptBuildsWithoutInstallingAndOnlyStagesInsideProject()
        {
            var script = File.ReadAllText(Path.Combine(GetProjectRoot(), "scripts", "Package.ps1"));

            Assert.Contains("/p:InstallAfterBuild=false", script);
            Assert.Contains("dist", script);
            Assert.Contains("BepInEx\\plugins\\SpecialRatizens", script);
            Assert.Contains("SpecialRatizens.dll", script);
            Assert.DoesNotContain("Start-Process", script);
            Assert.DoesNotContain("Join-Path $ratopiaRoot 'BepInEx\\plugins", script);
        }

        [Fact]
        public void ReadmeStatesCompatibilityContentsAndSaveRisk()
        {
            var readme = File.ReadAllText(Path.Combine(GetProjectRoot(), "README.md"));

            Assert.Contains("BepInEx 5", readme);
            Assert.Contains("12", readme);
            Assert.Contains("24", readme);
            Assert.Contains("备份", readme);
            Assert.Contains("卸载", readme);
            Assert.Contains("未进行游戏内实机验证", readme);
        }

        [Fact]
        public void ReleaseVersionIsConsistentAcrossProjectDocumentationAndPackageName()
        {
            var root = GetProjectRoot();
            var project = File.ReadAllText(Path.Combine(root, "src", "SpecialRatizens", "SpecialRatizens.csproj"));
            var readme = File.ReadAllText(Path.Combine(root, "README.md"));
            var package = File.ReadAllText(Path.Combine(root, "scripts", "Package.ps1"));

            Assert.Contains("<Version>0.1.4</Version>", project);
            Assert.Contains("<AssemblyVersion>0.1.4.0</AssemblyVersion>", project);
            Assert.Contains("<FileVersion>0.1.4.0</FileVersion>", project);
            Assert.Contains("# 特殊鼠鼠 v0.1.4", readme);
            Assert.Contains("特殊鼠鼠 v0.1.4 已加载", readme);
            Assert.Contains("特殊鼠鼠-v0.1.4-BepInEx5.zip", package);
        }

        private static string GetProjectRoot()
        {
            return typeof(PackagingContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
