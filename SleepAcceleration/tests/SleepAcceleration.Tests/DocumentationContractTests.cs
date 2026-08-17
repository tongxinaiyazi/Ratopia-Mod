using Xunit;

namespace SleepAcceleration.Tests
{
    public sealed class DocumentationContractTests
    {
        [Fact]
        public void ReadmeDocumentsBehaviorCompatibilitySafetyAndRemoval()
        {
            var readme = ContractTestPaths.ReadProjectFile("README.md");

            foreach (var required in new[]
                     {
                         "睡觉加速",
                         "0.1.0",
                         "3 秒",
                         "5 倍速",
                         "所有女王床",
                         "Sleep_bed",
                         "恢复",
                         "玩家主动调速",
                         "本次睡眠",
                         "Ratopia 1.0.0600",
                         "BepInEx 5.4.23.5",
                         "BepInEx/plugins/SleepAcceleration",
                         "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D",
                         "不新增配置",
                         "存档",
                         "卸载",
                         "冲突",
                         "LogOutput.log"
                     })
            {
                Assert.Contains(required, readme);
            }
        }
    }
}
