using System;
using System.Linq;
using System.Reflection;

namespace EquipmentReforgeDodge.Tests
{
    internal static class ContractTestPaths
    {
        public static string GameDirectory { get; } =
            Environment.GetEnvironmentVariable("RATOPIA_DIR")
            ?? throw new InvalidOperationException(
                "未设置 RATOPIA_DIR 环境变量，无法定位鼠托邦游戏目录。请在系统环境变量中配置，或在构建前执行 $env:RATOPIA_DIR = '<游戏目录>'。");

        public static string ProjectRoot =>
            typeof(ContractTestPaths).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
    }
}
