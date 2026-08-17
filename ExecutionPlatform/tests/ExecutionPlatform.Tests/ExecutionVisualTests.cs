using System.Reflection;
using ExecutionPlatform.Runtime;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class ExecutionVisualTests
    {
        [Fact]
        public void ExecutionBuildingSpritePathUsesPrisonAsset()
        {
            var visuals = GetVisualsType();
            var resolve = visuals.GetMethod(
                "ResolveSpritePath",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(resolve);
            Assert.Equal(
                "GameScene/Map/Building/Building_Prison",
                resolve.Invoke(null, new object[]
                {
                    "GameScene/Map/Building/Building_10001"
                }));
            Assert.Equal(
                "GameScene/Map/Building/Building_WoodLadder",
                resolve.Invoke(null, new object[]
                {
                    "GameScene/Map/Building/Building_WoodLadder"
                }));
        }

        [Fact]
        public void OnlyExecutionBuildingForcesOrdinaryFrame()
        {
            var visuals = GetVisualsType();
            var requiresOrdinaryFrame = visuals.GetMethod(
                "RequiresOrdinaryFrame",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.NotNull(requiresOrdinaryFrame);
            Assert.True((bool)requiresOrdinaryFrame.Invoke(null, new object[]
            {
                new BuildInfo { Name = ExecutionCatalog.RuntimeBuildingName }
            }));
            Assert.False((bool)requiresOrdinaryFrame.Invoke(null, new object[]
            {
                new BuildInfo { Name = BuildingName.Prison }
            }));
        }

        private static System.Type GetVisualsType()
        {
            var type = typeof(ExecutionCatalog).Assembly.GetType(
                "ExecutionPlatform.Runtime.ExecutionVisuals");
            Assert.NotNull(type);
            return type;
        }
    }
}
