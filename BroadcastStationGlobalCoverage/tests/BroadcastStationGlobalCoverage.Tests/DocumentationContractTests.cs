using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace BroadcastStationGlobalCoverage.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsInstallSafetyCompatibilityAndManualAcceptance()
        {
            var readme = ReadProjectFile("README.md");

            foreach (var required in new[]
                     {
                         "广播站信号覆盖全图",
                         "BepInEx 5.4.23.5",
                         "BepInEx/plugins/BroadcastStationGlobalCoverage",
                         "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D",
                         "卸载",
                         "存档",
                         "电视",
                         "不改变居民使用电视的服务距离",
                         "不修改广播站的电路范围",
                         "0.1.1",
                         "LogOutput.log",
                         "冲突",
                         "人工验收"
                     })
            {
                Assert.Contains(required, readme);
            }
        }

        private static string ReadProjectFile(params string[] relativePath)
        {
            var root = typeof(DocumentationContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
            var path = relativePath.Aggregate(root, Path.Combine);
            Assert.True(File.Exists(path), $"Required file not found: {path}");
            return File.ReadAllText(path);
        }
    }
}
