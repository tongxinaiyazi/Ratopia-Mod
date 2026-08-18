using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsInstallationBehaviorRiskAndRemoval()
        {
            var path = Path.Combine(GetProjectRoot(), "README.md");
            Assert.True(File.Exists(path), $"README missing: {path}");
            var text = File.ReadAllText(path);

            Assert.Contains("更强大的工作距离", text);
            Assert.Contains("BepInEx 5.4.23.5", text);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", text);
            Assert.Contains("横向 2 格", text);
            Assert.Contains("4 格高", text);
            Assert.Contains("安装", text);
            Assert.Contains("卸载", text);
            Assert.Contains("存档", text);
            Assert.Contains("BepInEx/LogOutput.log", text.Replace('\\', '/'));
            Assert.DoesNotContain(@"E:\steam\steamapps\common\Ratopia", text, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetProjectRoot()
        {
            return typeof(DocumentationContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
