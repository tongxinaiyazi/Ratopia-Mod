using System.IO;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsCompatibilitySafetyAndUninstallRules()
        {
            var readme = File.ReadAllText(Path.Combine(ContractTestPaths.ProjectRoot, "README.md"));

            Assert.Contains("Ratopia 1.0.0600", readme);
            Assert.Contains("BepInEx 5.4.23.5", readme);
            Assert.Contains("一秒", readme);
            Assert.Contains("游戏完全退出", readme);
            Assert.Contains("专用测试存档", readme);
            Assert.Contains("拆除所有处刑台", readme);
            Assert.Contains("正式存档", readme);
            Assert.Contains("不包含任何游戏资源", readme);
        }
    }
}
