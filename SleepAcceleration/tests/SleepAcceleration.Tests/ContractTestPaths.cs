using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SleepAcceleration.Tests
{
    internal static class ContractTestPaths
    {
        internal static string GameAssembly
        {
            get
            {
                var path = Path.Combine(RatopiaDir, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");
                EnsureFile(path);
                return path;
            }
        }

        internal static string PluginAssembly
        {
            get
            {
                var path = Path.Combine(AppContext.BaseDirectory, "SleepAcceleration.dll");
                EnsureFile(path);
                return path;
            }
        }

        internal static string ReadProjectFile(params string[] relativePath)
        {
            var path = relativePath.Aggregate(ProjectRoot, Path.Combine);
            EnsureFile(path);
            return File.ReadAllText(path);
        }

        private static string RatopiaDir => GetMetadata("RatopiaDir");

        private static string ProjectRoot => GetMetadata("ProjectRoot");

        private static string GetMetadata(string key)
        {
            return typeof(ContractTestPaths).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == key)
                .Value;
        }

        private static void EnsureFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Required contract file not found: {path}", path);
            }
        }
    }
}
