using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Xunit;

namespace BroadcastStationGlobalCoverage.Tests
{
    public sealed class ReleaseOutputContractTests
    {
        [Fact]
        public void ReleaseOutputContainsOnlyThePluginRuntimeDll()
        {
            var output = Path.Combine(
                GetProjectRoot(),
                "src",
                "BroadcastStationGlobalCoverage",
                "bin",
                "Release",
                "net472");
            Assert.True(Directory.Exists(output), $"Release output not found: {output}");

            var dllNames = Directory.GetFiles(output, "*.dll")
                .Select(Path.GetFileName)
                .OrderBy(name => name)
                .ToArray();
            Assert.Equal(new[] { "BroadcastStationGlobalCoverage.dll" }, dllNames);

            using (var assembly = AssemblyDefinition.ReadAssembly(
                       Path.Combine(output, "BroadcastStationGlobalCoverage.dll")))
            {
                Assert.Equal(new Version(0, 1, 1, 0), assembly.Name.Version);
                var informationalVersion = assembly.CustomAttributes.Single(attribute =>
                    attribute.AttributeType.FullName ==
                    "System.Reflection.AssemblyInformationalVersionAttribute");
                Assert.Equal("0.1.1", informationalVersion.ConstructorArguments[0].Value);
            }
        }

        private static string GetProjectRoot()
        {
            return typeof(ReleaseOutputContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
