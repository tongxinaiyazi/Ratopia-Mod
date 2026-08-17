using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace RatopiaMod.YunQing.All.Tests
{
    public sealed class ReleaseOutputContractTests
    {
        [Fact]
        public void PluginReleaseOutputContainsNoPrivateRuntimeCopiesOrPdb()
        {
            var projectRoot = Path.GetFullPath(
                typeof(ReleaseOutputContractTests).Assembly
                    .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .Cast<AssemblyMetadataAttribute>()
                    .Single(attribute => attribute.Key == "ProjectRoot")
                    .Value);
            var outputDirectory = Path.Combine(
                projectRoot,
                "src",
                "YunQingAll",
                "bin",
                "Release",
                "net472");

            Assert.True(Directory.Exists(outputDirectory), $"Release output missing: {outputDirectory}");
            Assert.True(File.Exists(Path.Combine(outputDirectory, "YunQingAll.dll")));

            var files = Directory.GetFiles(outputDirectory, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .ToArray();
            var forbidden = files.Where(name =>
                    name.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("0Harmony.dll", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("UnityEngine", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("BepInEx", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.Empty(forbidden);
        }
    }
}
