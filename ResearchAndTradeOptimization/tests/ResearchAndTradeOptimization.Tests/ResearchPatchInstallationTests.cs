using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Xunit;

namespace ResearchAndTradeOptimization.Tests
{
    public sealed class ResearchPatchInstallationTests
    {
        [Fact]
        public void ResearchTranspilersRewriteTheInspectedGameMethods()
        {
            var ratopiaDir = GetRatopiaDir();
            var managedDir = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed");
            var coreDir = Path.Combine(ratopiaDir, "BepInEx", "core");
            ResolveEventHandler resolver = (sender, arguments) =>
                ResolveAssembly(arguments.Name, managedDir, coreDir);
            AppDomain.CurrentDomain.AssemblyResolve += resolver;

            try
            {
                var game = Assembly.LoadFrom(
                    Path.Combine(managedDir, "Assembly-CSharp.dll"));
                var plugin = Assembly.LoadFrom(Path.Combine(
                    AppContext.BaseDirectory,
                    "ResearchAndTradeOptimization.dll"));
                var cases = new[]
                {
                    new[]
                    {
                        "Tech_RPInfo",
                        "UpgradBtn",
                        "ResearchAndTradeOptimization.Patches.ResearchQueueLimitPatch"
                    },
                    new[]
                    {
                        "ResearchUI",
                        "UpdateUpgradeNode",
                        "ResearchAndTradeOptimization.Patches.ResearchProgressPatch"
                    },
                    new[]
                    {
                        "Tech_RPInfo",
                        "RemoveUpgradeNode",
                        "ResearchAndTradeOptimization.Patches.ResearchRefundPatch"
                    }
                };

                foreach (var item in cases)
                {
                    var declaringType = game.GetType(item[0], throwOnError: true);
                    var original = declaringType.GetMethod(
                        item[1],
                        BindingFlags.Instance |
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);
                    Assert.NotNull(original);
                    var patchType = plugin.GetType(item[2], throwOnError: true);
                    var transpiler = patchType.GetMethod(
                        "Transpiler",
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);
                    Assert.NotNull(transpiler);

                    var dynamicMethod = new System.Reflection.Emit.DynamicMethod(
                        "ResearchTranspilerProbe",
                        typeof(void),
                        Type.EmptyTypes,
                        declaringType);
                    var instructions = PatchProcessor.GetOriginalInstructions(
                        original,
                        dynamicMethod.GetILGenerator());
                    var rewritten = (System.Collections.Generic.IEnumerable<CodeInstruction>)
                        transpiler.Invoke(null, new object[] { instructions });
                    Assert.NotEmpty(rewritten);
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolver;
            }
        }

        private static Assembly ResolveAssembly(
            string displayName,
            params string[] directories)
        {
            var fileName = new AssemblyName(displayName).Name + ".dll";
            foreach (var directory in directories)
            {
                var path = Path.Combine(directory, fileName);
                if (File.Exists(path))
                {
                    return Assembly.LoadFrom(path);
                }
            }

            return null;
        }

        private static string GetRatopiaDir()
        {
            var ratopiaDir = Environment.GetEnvironmentVariable("RATOPIA_DIR");
            if (!string.IsNullOrWhiteSpace(ratopiaDir))
            {
                return ratopiaDir;
            }

            return typeof(ResearchPatchInstallationTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "RatopiaDir")
                .Value;
        }
    }
}
