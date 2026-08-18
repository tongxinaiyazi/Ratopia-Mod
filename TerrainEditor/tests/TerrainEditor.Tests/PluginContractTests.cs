using System.IO;
using System.Linq;
using Mono.Cecil;
using TerrainEditor.Core;
using Xunit;

namespace TerrainEditor.Tests
{
    public sealed class PluginContractTests
    {
        [Fact]
        public void PluginMetadataAndLifecyclePatchesMatchReleaseContract()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(typeof(TerrainEditorController).Assembly.Location))
            {
                var plugin = assembly.MainModule.GetType("TerrainEditor.Plugin");
                Assert.NotNull(plugin);

                var attribute = plugin.CustomAttributes.Single(
                    item => item.AttributeType.FullName == "BepInEx.BepInPlugin");
                Assert.Equal("cn.ratopia.terraineditor", attribute.ConstructorArguments[0].Value);
                Assert.Equal("地形编辑器", attribute.ConstructorArguments[1].Value);
                Assert.Equal("0.1.0", attribute.ConstructorArguments[2].Value);

                Assert.NotNull(assembly.MainModule.GetType("TerrainEditor.Patches.TileManagerUpdatePatch"));
                Assert.NotNull(assembly.MainModule.GetType("TerrainEditor.Patches.LoadingSceneStartPatch"));
            }
        }

        [Fact]
        public void RuntimeReferencesAreNotCopiedBesidePlugin()
        {
            var directory = Path.GetDirectoryName(typeof(TerrainEditorController).Assembly.Location);
            var forbidden = new[]
            {
                "Assembly-CSharp.dll",
                "BepInEx.dll",
                "0Harmony.dll",
                "UnityEngine.dll",
                "UnityEngine.CoreModule.dll"
            };

            foreach (var file in forbidden)
            {
                Assert.False(File.Exists(Path.Combine(directory, file)), file);
            }
        }
    }
}
