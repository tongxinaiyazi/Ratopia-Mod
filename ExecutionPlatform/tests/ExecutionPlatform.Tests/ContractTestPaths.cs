using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ExecutionPlatform.Tests
{
    internal static class ContractTestPaths
    {
        internal static string GameAssembly => Path.Combine(
            ReadMetadata("RatopiaDir"),
            "Ratopia_Data",
            "Managed",
            "Assembly-CSharp.dll");

        internal static string PluginAssembly => Path.Combine(
            ReadMetadata("ProjectRoot"),
            "src",
            "ExecutionPlatform",
            "bin",
            "Release",
            "net472",
            "ExecutionPlatform.dll");

        internal static string ProjectRoot => ReadMetadata("ProjectRoot");

        private static string ReadMetadata(string key)
        {
            var value = typeof(ContractTestPaths).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == key)
                .Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing assembly metadata: {key}");
            }

            return Path.GetFullPath(value);
        }
    }
}
