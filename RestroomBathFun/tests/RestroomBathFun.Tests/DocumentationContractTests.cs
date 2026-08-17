using Xunit;

namespace RestroomBathFun.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsBehaviorConfigurationAndVerificationLimits()
        {
            var readme = ContractTestPaths.ReadProjectFile("README.md");

            foreach (var required in new[]
                     {
                         "卫生间澡堂加乐趣",
                         "BepInEx 5.4.23.5",
                         "BepInEx/plugins/RestroomBathFun",
                         "BepInEx/config/cn.ratopia.restroombathfun.cfg",
                         "ToiletFunReward",
                         "BathsFunReward",
                         "普通卫生间",
                         "电动卫生间不生效",
                         "修改后重启游戏",
                         "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D",
                         "卸载",
                         "存档",
                         "冲突",
                         "LogOutput.log",
                         "未进行游戏内实机验收",
                         "1.0.0"
                     })
            {
                Assert.Contains(required, readme);
            }
        }

        [Fact]
        public void NexusCopyIsBilingualAndTruthful()
        {
            var summary = ContractTestPaths.ReadProjectFile(
                "release-assets", "2-简介.txt");
            var description = ContractTestPaths.ReadProjectFile(
                "release-assets", "3-双语完整介绍.txt");

            Assert.Contains("25", summary);
            Assert.Contains("30", summary);
            Assert.Contains("普通卫生间", summary);
            Assert.Contains("restroom", summary.ToLowerInvariant());
            Assert.Contains("[b]English[/b]", description);
            Assert.Contains("[b]中文[/b]", description);
            Assert.Contains("Electric toilets are not affected", description);
            Assert.Contains("电动卫生间不生效", description);
            Assert.Contains("not been tested in-game", description);
            Assert.Contains("未进行游戏内实机验收", description);
        }
    }
}
