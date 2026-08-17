using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace UnlimitedTradeAgreements.Tests
{
    internal static class TestPaths
    {
        internal static string ProjectRoot => GetMetadata("ProjectRoot");

        internal static string PluginAssembly => Path.Combine(
            ProjectRoot,
            "src",
            "UnlimitedTradeAgreements",
            "bin",
            "Release",
            "net472",
            "UnlimitedTradeAgreements.dll");

        internal static string GameAssembly => Path.Combine(
            GetMetadata("RatopiaDir"),
            "Ratopia_Data",
            "Managed",
            "Assembly-CSharp.dll");

        internal static string RequireFile(string path)
        {
            Assert.True(File.Exists(path), $"Required file not found: {path}");
            return path;
        }

        private static string GetMetadata(string key)
        {
            var value = typeof(TestPaths).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == key)
                .Value;
            return Path.GetFullPath(value);
        }
    }
}
