using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Xunit;

namespace SleepAcceleration.Tests
{
    public sealed class PackagingContractTests
    {
        [Fact]
        public void ProjectReferencesAreNeverCopied()
        {
            var project = ContractTestPaths.ReadProjectFile(
                "src", "SleepAcceleration", "SleepAcceleration.csproj");

            Assert.Equal(5, CountOccurrences(project, "Private=\"false\""));
            Assert.DoesNotContain("<Private>true</Private>", project);
        }

        [Fact]
        public void PackageScriptBuildsWithoutInstallingAndUsesTheExactLayout()
        {
            var script = ContractTestPaths.ReadProjectFile("scripts", "Package.ps1");

            Assert.Contains("InstallAfterBuild=false", script);
            Assert.Contains("SleepAcceleration-v0.1.0-BepInEx5.zip", script);
            Assert.Contains("BepInEx\\plugins\\SleepAcceleration", script);
            Assert.Contains("SleepAcceleration.dll", script);
            Assert.Contains("README.md", script);
            Assert.Contains("Test-RatopiaPackage.ps1", script);
            Assert.Contains("$actualSignature", script);
            Assert.Contains("$expectedSignature", script);
            Assert.Contains("if ($actualSignature -ne $expectedSignature)", script);
        }

        [Fact]
        public void PackageScriptRejectsRuntimeDependencyDllsAndDebugArtifacts()
        {
            var script = ContractTestPaths.ReadProjectFile("scripts", "Package.ps1");

            foreach (var forbidden in new[]
                     {
                         "Assembly-CSharp.dll",
                         "0Harmony.dll",
                         "BepInEx.dll",
                         "UnityEngine.dll",
                         "UnityEngine.CoreModule.dll",
                         "*.pdb"
                     })
            {
                Assert.Contains(forbidden, script);
            }
        }

        [Fact]
        public void ReleaseOutputContainsOnlyThePluginRuntimeDllAtTheExpectedVersion()
        {
            var output = Path.Combine(
                GetProjectRoot(), "src", "SleepAcceleration", "bin", "Release", "net472");
            Assert.True(Directory.Exists(output), $"Release output not found: {output}");

            var dllNames = Directory.GetFiles(output, "*.dll")
                .Select(Path.GetFileName)
                .OrderBy(name => name)
                .ToArray();
            Assert.Equal(new[] { "SleepAcceleration.dll" }, dllNames);

            using (var assembly = AssemblyDefinition.ReadAssembly(
                       Path.Combine(output, "SleepAcceleration.dll")))
            {
                Assert.Equal(new Version(0, 1, 0, 0), assembly.Name.Version);
                var informationalVersion = assembly.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName ==
                    "System.Reflection.AssemblyInformationalVersionAttribute");
                Assert.Equal("0.1.0", informationalVersion.ConstructorArguments[0].Value);
            }
        }

        private static int CountOccurrences(string value, string search)
        {
            var count = 0;
            var index = 0;
            while ((index = value.IndexOf(search, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += search.Length;
            }

            return count;
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
