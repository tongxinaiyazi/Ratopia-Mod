using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class ReleaseOutputContractTests
    {
        [Fact]
        public void PluginReleaseOutputContainsNoPrivateRuntimeCopies()
        {
            var output = Path.Combine(GetProjectRoot(), "src", "PopulationCustomizer", "bin", "Release", "net472");
            Assert.True(Directory.Exists(output), $"Release output missing: {output}");
            Assert.True(File.Exists(Path.Combine(output, "PopulationCustomizer.dll")));

            var files = Directory.GetFiles(output, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .ToArray();
            var forbidden = files.Where(name =>
                    name.Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("0Harmony.dll", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Utility.Savable.SavableData.dll", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("BepInEx", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith("Microsoft.TestPlatform", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.Empty(forbidden);
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
