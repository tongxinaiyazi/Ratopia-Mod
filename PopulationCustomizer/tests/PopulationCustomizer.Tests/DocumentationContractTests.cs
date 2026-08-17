using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsRequiredBehaviorAndSafety()
        {
            var readme = File.ReadAllText(Path.Combine(GetProjectRoot(), "README.md"));

            Assert.Contains("人口自定义", readme);
            Assert.Contains("BepInEx 5", readme);
            Assert.Contains("0.1.3", readme);
            Assert.Contains("0–999", readme);
            Assert.Contains("点击原版人口数量", readme);
            Assert.Contains("放大镜按钮左侧", readme);
            Assert.Contains("每个存档", readme);
            Assert.Contains("ModsData", readme);
            Assert.Contains("正常保存", readme);
            Assert.Contains("不会删除", readme);
            Assert.Contains("恢复原版", readme);
            Assert.Contains("卸载", readme);
            Assert.Contains("C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D", readme);
        }

        [Fact]
        public void TestingGuideCoversRuntimeAcceptanceMatrix()
        {
            var guide = File.ReadAllText(Path.Combine(GetProjectRoot(), "docs", "TESTING.md"));

            Assert.Contains("鼠民招募", guide);
            Assert.Contains("机器鼠制造", guide);
            Assert.Contains("两轮", guide);
            Assert.Contains("不同存档", guide);
            Assert.Contains("移除", guide);
            Assert.Contains("RatopiaCitizenListUpdateMod", guide);
            Assert.Contains("放大镜按钮左侧", guide);
            Assert.Contains("只出现一次", guide);
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
