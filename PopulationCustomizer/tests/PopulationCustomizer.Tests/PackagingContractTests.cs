using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class PackagingContractTests
    {
        [Fact]
        public void PackagingScriptBuildsWithoutInstallingAndUsesExactWhitelist()
        {
            var script = File.ReadAllText(Path.Combine(GetProjectRoot(), "scripts", "Package.ps1"));

            Assert.Contains("/p:InstallAfterBuild=false", script);
            Assert.Contains("人口自定义-v0.1.3-BepInEx5.zip", script);
            Assert.Contains("BepInEx\\plugins\\PopulationCustomizer", script);
            Assert.Contains("PopulationCustomizer.dll", script);
            Assert.Contains("README.md", script);
            Assert.DoesNotContain("Start-Process", script);

            var project = File.ReadAllText(Path.Combine(
                GetProjectRoot(),
                "src",
                "PopulationCustomizer",
                "PopulationCustomizer.csproj"));
            Assert.Contains("<Version>0.1.3</Version>", project);
            Assert.Contains("<AssemblyVersion>0.1.3.0</AssemblyVersion>", project);
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
