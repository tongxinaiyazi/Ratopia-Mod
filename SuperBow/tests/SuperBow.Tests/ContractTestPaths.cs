using System;
using System.Linq;
using System.Reflection;

namespace SuperBow.Tests
{
    internal static class ContractTestPaths
    {
        public const string GameDirectory = @"E:\steam\steamapps\common\Ratopia";

        public static string ProjectRoot =>
            typeof(ContractTestPaths).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
    }
}
