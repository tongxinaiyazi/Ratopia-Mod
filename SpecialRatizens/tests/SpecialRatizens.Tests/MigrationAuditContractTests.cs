using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class MigrationAuditContractTests
    {
        [Fact]
        public void RuntimeStateRegistryContainsEveryShippedTraitExactlyOnce()
        {
            var shipped = LoadCatalog().Traits
                .Select(item => item.Name)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();

            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var cctor = module.Types
                    .Single(type => type.FullName == "RatopiaMod.CustomMOD")
                    .Methods.Single(method => method.Name == ".cctor");
                var registered = ExtractCustomCharInfoKeys(cctor)
                    .OrderBy(item => item, StringComparer.Ordinal)
                    .ToArray();

                Assert.Equal(24, registered.Length);
                Assert.Equal(shipped, registered);
            }
        }

        [Fact]
        public void LoadingAnotherWorldResetsRuntimeStateBeforeRebuildingCitizens()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var loaded = GetLegacyMethod(module, "SpecialRatizensSessionLoaded");
                Assert.True(CallsInOrder(
                    loaded,
                    "ResetSpecialRatizensSession",
                    "LoadCitizenDatas",
                    "UpdateAllUsedSpecialEffects"));

                var reset = GetLegacyMethod(module, "ResetSpecialRatizensSession");
                Assert.True(LoadsFieldThenCalls(reset, "preValueDic", "Clear"));
                Assert.True(LoadsFieldThenCalls(reset, "CountryCommercialityDatas", "Clear"));
                Assert.True(StoresStaticField(reset, "SuperElecLine"));
                Assert.True(StoresStaticField(reset, "AMJ7_PDI"));
            }
        }

        [Fact]
        public void CharacterDatabasePostfixInitializesTheProsperityBaseline()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "DB_Mgr_Character_DB_Setting");

                Assert.Contains(method.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference call &&
                    call.Name == "LoadProsperityDB" &&
                    call.Parameters.Count == 1 &&
                    call.Parameters[0].ParameterType.FullName == "DB_Mgr");
            }
        }

        [Fact]
        public void NewlyRecruitedSpecialAppliesSelfEffectsAfterRegisteringTraitUsers()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "T_Citizen_MakeCtizen_ByCC");

                Assert.True(CallsInOrder(method, "UpdateCustomCharInfoUser", "UpdateCitizenUsedSpecialEffects"));
            }
        }

        [Fact]
        public void QinLawValidatesTheBaselineBeforeReadingRuntimeLevels()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "SY_QL_Effect");
                var ensureIndex = method.Body.Instructions
                    .Select((instruction, index) => new { instruction, index })
                    .Where(item =>
                        item.instruction.Operand is MethodReference call &&
                        call.Name == "EnsureProsperityBaseline")
                    .Select(item => item.index)
                    .DefaultIfEmpty(-1)
                    .Single();
                var liveListIndex = method.Body.Instructions
                    .Select((instruction, index) => new { instruction, index })
                    .Where(item =>
                        item.instruction.Operand is FieldReference field &&
                        field.Name == "List_ProsperityDB")
                    .Select(item => item.index)
                    .DefaultIfEmpty(-1)
                    .First();

                Assert.True(ensureIndex >= 0);
                Assert.True(liveListIndex >= 0);
                Assert.True(ensureIndex < liveListIndex);
            }
        }

        private static SpecialDataCatalog LoadCatalog()
        {
            var data = Path.Combine(GetProjectRoot(), "Data");
            return SpecialDataCatalog.Load(
                Path.Combine(data, "CustomSpecialUnit.csv"),
                Path.Combine(data, "CustomCharInfo.csv"),
                Path.Combine(data, "Icon"));
        }

        private static IEnumerable<string> ExtractCustomCharInfoKeys(MethodDefinition cctor)
        {
            var instructions = cctor.Body.Instructions;
            for (var index = 0; index < instructions.Count; index++)
            {
                if (instructions[index].OpCode.Code != Code.Ldstr || !(instructions[index].Operand is string key))
                {
                    continue;
                }

                var createsCustomInfo = instructions
                    .Skip(index + 1)
                    .Take(4)
                    .Any(instruction =>
                        instruction.OpCode.Code == Code.Newobj &&
                        instruction.Operand is MethodReference constructor &&
                        constructor.DeclaringType.Name == "CustomCharInfo");
                if (createsCustomInfo)
                {
                    yield return key;
                }
            }
        }

        private static string[] CalledMethodNames(MethodDefinition method)
        {
            return method.Body.Instructions
                .Select(instruction => instruction.Operand as MethodReference)
                .Where(reference => reference != null)
                .Select(reference => reference.Name)
                .ToArray();
        }

        private static bool CallsInOrder(MethodDefinition method, params string[] names)
        {
            var calls = CalledMethodNames(method);
            var position = -1;
            foreach (var name in names)
            {
                position = Array.IndexOf(calls, name, position + 1);
                if (position < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool LoadsFieldThenCalls(MethodDefinition method, string fieldName, string callName)
        {
            var instructions = method.Body.Instructions;
            for (var index = 0; index < instructions.Count - 1; index++)
            {
                if (instructions[index].OpCode.Code == Code.Ldsfld &&
                    instructions[index].Operand is FieldReference field &&
                    field.Name == fieldName &&
                    instructions[index + 1].Operand is MethodReference call &&
                    call.Name == callName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool StoresStaticField(MethodDefinition method, string fieldName)
        {
            return method.Body.Instructions.Any(instruction =>
                instruction.OpCode.Code == Code.Stsfld &&
                instruction.Operand is FieldReference field &&
                field.Name == fieldName);
        }

        private static MethodDefinition GetLegacyMethod(ModuleDefinition module, string name)
        {
            return module.Types
                .Single(type => type.FullName == "RatopiaMod.CustomMOD")
                .Methods.Single(method => method.Name == name);
        }

        private static string GetProjectRoot()
        {
            return typeof(MigrationAuditContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
