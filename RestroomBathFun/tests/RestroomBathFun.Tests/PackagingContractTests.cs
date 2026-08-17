using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Xunit;

namespace RestroomBathFun.Tests
{
    public sealed class PackagingContractTests
    {
        [Fact]
        public void ProjectReferencesAreNeverCopied()
        {
            var project = ContractTestPaths.ReadProjectFile(
                "src", "RestroomBathFun", "RestroomBathFun.csproj");

            Assert.Equal(5, CountOccurrences(project, "Private=\"false\""));
            Assert.DoesNotContain("<Private>true</Private>", project);
        }

        [Fact]
        public void PackageScriptBuildsWithoutInstallingAndUsesTheExactLayout()
        {
            var script = ContractTestPaths.ReadProjectFile("scripts", "Package.ps1");

            Assert.Contains("InstallAfterBuild=false", script);
            Assert.Contains("卫生间澡堂加乐趣-v1.0.0-BepInEx5.zip", script);
            Assert.Contains("BepInEx\\plugins\\RestroomBathFun", script);
            Assert.Contains("RestroomBathFun.dll", script);
            Assert.Contains("README.md", script);
            Assert.Contains("Test-RatopiaPackage.ps1", script);
            Assert.Contains("Test-RatopiaNexusDeliverables.ps1", script);
            Assert.Contains("$actualSignature = (($actualEntries | Sort-Object) -join \"`n\")", script);
            Assert.Contains("$expectedSignature = (($expectedEntries | Sort-Object) -join \"`n\")", script);
            Assert.Contains("if ($actualSignature -ne $expectedSignature)", script);
        }

        [Fact]
        public void PackageScriptRejectsRuntimeDependencyDlls()
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
        public void ReleaseOutputContainsOnlyThePluginRuntimeDll()
        {
            var output = Path.Combine(
                GetProjectRoot(), "src", "RestroomBathFun", "bin", "Release", "net472");
            Assert.True(Directory.Exists(output), $"Release output not found: {output}");

            var dllNames = Directory.GetFiles(output, "*.dll")
                .Select(Path.GetFileName)
                .OrderBy(name => name)
                .ToArray();
            Assert.Equal(new[] { "RestroomBathFun.dll" }, dllNames);

            using (var assembly = AssemblyDefinition.ReadAssembly(
                       Path.Combine(output, "RestroomBathFun.dll")))
            {
                Assert.Equal(new Version(1, 0, 0, 0), assembly.Name.Version);
                var informationalVersion = assembly.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName ==
                    "System.Reflection.AssemblyInformationalVersionAttribute");
                Assert.Equal("1.0.0", informationalVersion.ConstructorArguments[0].Value);
            }
        }

        [Fact]
        public void CoverIsAnUploadReadyLandscapePng()
        {
            var path = Path.Combine(GetProjectRoot(), "release-assets", "4-封面.png");
            Assert.True(File.Exists(path), $"Cover not found: {path}");

            var bytes = File.ReadAllBytes(path);
            Assert.True(bytes.Length > 24, "Cover is too small to be a PNG.");
            Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes.Take(8));
            Assert.True(ReadBigEndianInt32(bytes, 16) >= 1280, "Cover width must be at least 1280.");
            Assert.True(ReadBigEndianInt32(bytes, 20) >= 720, "Cover height must be at least 720.");
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) |
                   (bytes[offset + 1] << 16) |
                   (bytes[offset + 2] << 8) |
                   bytes[offset + 3];
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
