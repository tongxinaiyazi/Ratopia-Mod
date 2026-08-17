using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class PackagingSourceContractTests
    {
        [Fact]
        public void PackageScriptBuildsWithoutInstallingAndStagesOnlyReleaseFiles()
        {
            var path = Path.Combine(GetProjectRoot(), "scripts", "Package.ps1");
            Assert.True(File.Exists(path), $"Package script missing: {path}");
            var text = File.ReadAllText(path);

            Assert.Contains("/p:InstallAfterBuild=false", text);
            Assert.Contains("BepInEx\\plugins\\StrongerWorkDistance", text);
            Assert.Contains("StrongerWorkDistance.dll", text);
            Assert.Contains("README.md", text);
            Assert.Contains("更强大的工作距离-v0.1.0-BepInEx5.zip", text);
            Assert.DoesNotContain(@"E:\steam\steamapps\common\Ratopia", text, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetProjectRoot()
        {
            return typeof(PackagingSourceContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
