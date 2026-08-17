using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace RatopiaMod.YunQing.All.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void PackagedReadmeContainsNoMachineSpecificAbsolutePaths()
        {
            var projectRoot = Path.GetFullPath(
                typeof(DocumentationContractTests).Assembly
                    .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .Cast<AssemblyMetadataAttribute>()
                    .Single(attribute => attribute.Key == "ProjectRoot")
                    .Value);
            var readme = File.ReadAllText(Path.Combine(projectRoot, "README.md"));

            Assert.False(
                Regex.IsMatch(readme, @"[A-Za-z]:\\"),
                "README must not expose a machine-specific absolute Windows path.");
            Assert.Contains("尚未进行游戏内运行验证", readme);
            Assert.Contains("CheatPanelLocalizer", readme);
        }
    }
}
