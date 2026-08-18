using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace StrongerWorkDistance.Tests
{
    public sealed class WorkAreaPublicContractTests
    {
        [Fact]
        public void CoreTypesExposeThePlannedApi()
        {
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, "StrongerWorkDistance.dll");
            Assert.True(File.Exists(assemblyPath), $"Plugin assembly not found: {assemblyPath}");

            using (var module = ModuleDefinition.ReadModule(assemblyPath))
            {
                var offset = module.Types.SingleOrDefault(type => type.FullName == "StrongerWorkDistance.Core.WorkOffset");
                var rules = module.Types.SingleOrDefault(type => type.FullName == "StrongerWorkDistance.Core.WorkAreaRules");

                Assert.NotNull(offset);
                Assert.NotNull(rules);
                Assert.Contains(offset.Properties, property => property.Name == "X" && property.PropertyType.FullName == "System.Int32");
                Assert.Contains(offset.Properties, property => property.Name == "Y" && property.PropertyType.FullName == "System.Int32");
                Assert.Contains(offset.Methods, method => method.IsConstructor && method.Parameters.Count == 2);
                Assert.Contains(rules.Methods, method =>
                    method.Name == "CreateExpandedOffsets" &&
                    method.IsStatic &&
                    method.Parameters.Count == 0);
            }
        }

        [Fact]
        public void AtomicListUpdaterExposesThePlannedApi()
        {
            var assemblyPath = Path.Combine(AppContext.BaseDirectory, "StrongerWorkDistance.dll");

            using (var module = ModuleDefinition.ReadModule(assemblyPath))
            {
                var updater = module.Types.SingleOrDefault(type =>
                    type.FullName == "StrongerWorkDistance.Core.AtomicListUpdater");

                Assert.NotNull(updater);
                Assert.Contains(updater.Methods, method =>
                    method.Name == "ReplaceBoth" &&
                    method.IsStatic &&
                    method.HasGenericParameters &&
                    method.Parameters.Count == 3);
            }
        }
    }
}
