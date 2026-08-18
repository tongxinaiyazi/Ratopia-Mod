using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace BroadcastStationGlobalCoverage.Tests
{
    public sealed class PackagingSourceContractTests
    {
        [Fact]
        public void PackageScriptBuildsWithoutInstallingAndUsesTheExactLayout()
        {
            var script = ReadProjectFile("scripts", "Package.ps1");

            Assert.Contains("InstallAfterBuild=false", script);
            Assert.Contains("广播站信号覆盖全图-v0.1.1-BepInEx5.zip", script);
            Assert.Contains("BepInEx\\plugins\\BroadcastStationGlobalCoverage", script);
            Assert.Contains("BroadcastStationGlobalCoverage.dll", script);
            Assert.Contains("README.md", script);
        }

        [Fact]
        public void PackageScriptRejectsRuntimeDependencyDlls()
        {
            var script = ReadProjectFile("scripts", "Package.ps1");

            foreach (var forbidden in new[]
                     {
                         "Assembly-CSharp.dll",
                         "0Harmony.dll",
                         "BepInEx.dll",
                         "UnityEngine.dll",
                         "UnityEngine.CoreModule.dll"
                     })
            {
                Assert.Contains(forbidden, script);
            }
        }

        [Fact]
        public void GameAndLoaderReferencesAreNeverCopied()
        {
            var project = ReadProjectFile(
                "src",
                "BroadcastStationGlobalCoverage",
                "BroadcastStationGlobalCoverage.csproj");
            Assert.Equal(5, CountOccurrences(project, "Private=\"false\""));
            Assert.DoesNotContain("<Private>true</Private>", project);
        }

        private static string ReadProjectFile(params string[] relativePath)
        {
            var path = relativePath.Aggregate(GetProjectRoot(), Path.Combine);
            Assert.True(File.Exists(path), $"Required file not found: {path}");
            return File.ReadAllText(path);
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
            return typeof(PackagingSourceContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
