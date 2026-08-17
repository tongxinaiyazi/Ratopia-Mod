using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class AppearanceContractTests
    {
        [Fact]
        public void SpecialRatizenAppearanceValidatesBeforeInstallingTheCombinedSkin()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "UpdateUnitCustomSkin");
                Assert.True(CallsInOrder(method, "ClearSkins", "AssembleData", "HasRequiredAppearance"));
                Assert.Contains("RecoverUnitSkin", CalledMethodNames(method));
                Assert.Contains("RenderCombinedSkin", CalledMethodNames(method));

                var render = GetLegacyMethod(module, "RenderCombinedSkin");
                Assert.Equal(new[] { "SkinSet", "UpdateCombinedSkin" }, CalledMethodNames(render));
            }
        }

        [Fact]
        public void CandidateBuildsPreviewWithoutApplyingToBoundSkeleton()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var candidate = GetLegacyMethod(module, "CCMake_Info");
                Assert.True(CallsWithBoolean(candidate, "RegisterCustomSkin", false));

                var update = GetLegacyMethod(module, "UpdateUnitCustomSkin");
                Assert.Contains(update.Parameters, parameter =>
                    parameter.Name == "applyToSkeleton" &&
                    parameter.ParameterType.FullName == "System.Boolean");

                var render = GetLegacyMethod(module, "RenderCombinedSkin");
                Assert.Contains(render.Parameters, parameter =>
                    parameter.Name == "applyToSkeleton" &&
                    parameter.ParameterType.FullName == "System.Boolean");
                Assert.True(UpdateCombinedSkinIsGuarded(render));
            }
        }

        [Fact]
        public void LiveCitizenStillAppliesTheCombinedSkin()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                Assert.True(CallsWithBoolean(
                    GetLegacyMethod(module, "AddSpecialCitizen"),
                    "RegisterCustomSkin",
                    true));
            }
        }

        [Fact]
        public void SpecialTemplateUsesTheSkinObjectsRuntimeGender()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "RegisterCustomSkin");

                Assert.Contains(method.Body.Instructions, instruction =>
                    (instruction.OpCode.Code == Code.Ldfld || instruction.OpCode.Code == Code.Ldflda) &&
                    instruction.Operand is FieldReference field &&
                    field.DeclaringType.FullName == "Sp_SkinInfo" &&
                    field.Name == "m_Gender");
                Assert.DoesNotContain(method.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference call &&
                    call.Name == "get_UnitGender");
            }
        }

        [Fact]
        public void SpecialCitizenSynchronizesOnlyTheSkinGenderBeforeRegistration()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var method = GetLegacyMethod(module, "AddSpecialCitizen");

                Assert.True(CopiesCitizenGenderToSkinInfoBeforeRegistering(method));
                Assert.DoesNotContain(method.Body.Instructions, instruction =>
                    instruction.OpCode.Code == Code.Stfld &&
                    instruction.Operand is FieldReference field &&
                    field.DeclaringType.FullName == "GameUnit" &&
                    field.Name == "m_Gender");
            }
        }

        [Fact]
        public void LegacyGenderInputIsTrimmedBeforeEnumParsing()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var setter = module.Types
                    .Single(type => type.FullName == "RatopiaMod.CustomSpecialUnit")
                    .Methods.Single(method => method.Name == "set_UnitGender");

                Assert.Contains(setter.Body.Instructions, instruction =>
                    instruction.Operand is MethodReference call &&
                    call.DeclaringType.FullName == "System.String" &&
                    call.Name == "Trim");
            }
        }

        [Fact]
        public void FailedSpecialAppearanceHasSnapshotAndDefaultRecoveryPaths()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var recovery = GetLegacyMethod(module, "RecoverUnitSkin");
                var calls = CalledMethodNames(recovery);

                Assert.True(CallsInOrder(recovery, "SelectRecovery", "ClearSkins", "ClearOverrideSkin"));
                Assert.Contains("SetStyles", calls);
                Assert.Contains("SetStyleOverride", calls);
                Assert.Contains("AssembleDefaultSkin", calls);
                Assert.Contains("AssembleData", calls);
                Assert.Contains("RenderCombinedSkin", calls);
                Assert.Contains("HasRequiredAppearance", calls);
            }
        }

        [Fact]
        public void OrdinaryCitizensNeverEnterTheCustomSkinPipeline()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var loadCalls = CalledMethodNames(GetLegacyMethod(module, "LoadCitizenDatas"));
                Assert.DoesNotContain("TryGetCitizenCustomSkin", loadCalls);
                Assert.DoesNotContain("UpdateUnitSpineDress", loadCalls);

                var clothes = GetLegacyMethod(module, "UpdateClothes");
                Assert.True(CallsInOrder(clothes, "CitizenIsSpecialUnit", "UpdateUnitSpineDress"));
                Assert.DoesNotContain(clothes.Body.Instructions, instruction =>
                    instruction.Operand is FieldReference field && field.Name == "CitizenCustomSkins");
            }
        }

        [Fact]
        public void SessionResetClearsAllAppearanceRuntimeState()
        {
            using (var module = ModuleDefinition.ReadModule(typeof(SpecialDataCatalog).Assembly.Location))
            {
                var reset = GetLegacyMethod(module, "ResetSpecialRatizensSession");

                Assert.True(LoadsFieldThenCalls(reset, "CitizenCustomSkins", "Clear"));
                Assert.True(LoadsFieldThenCalls(reset, "EditingCustomSkinIndex", "Clear"));
                Assert.True(StoresStaticField(reset, "OpenedCitizenInfo"));
                Assert.True(StoresStaticField(reset, "OpenedSpcialCitizen"));
                Assert.True(StoresStaticField(reset, "EditingCustomSkins"));
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

        private static bool CopiesCitizenGenderToSkinInfoBeforeRegistering(MethodDefinition method)
        {
            var instructions = method.Body.Instructions;
            for (var index = 0; index < instructions.Count - 5; index++)
            {
                if (instructions[index].OpCode.Code == Code.Ldarg_1 &&
                    instructions[index + 1].OpCode.Code == Code.Ldfld &&
                    instructions[index + 1].Operand is FieldReference skinInfo &&
                    skinInfo.DeclaringType.FullName == "GameUnit" &&
                    skinInfo.Name == "m_SkinInfo" &&
                    instructions[index + 2].OpCode.Code == Code.Ldarg_1 &&
                    instructions[index + 3].OpCode.Code == Code.Ldfld &&
                    instructions[index + 3].Operand is FieldReference citizenGender &&
                    citizenGender.DeclaringType.FullName == "GameUnit" &&
                    citizenGender.Name == "m_Gender" &&
                    instructions[index + 4].OpCode.Code == Code.Stfld &&
                    instructions[index + 4].Operand is FieldReference skinGender &&
                    skinGender.DeclaringType.FullName == "Sp_SkinInfo" &&
                    skinGender.Name == "m_Gender")
                {
                    return instructions.Skip(index + 5).Any(instruction =>
                        instruction.Operand is MethodReference call &&
                        call.Name == "RegisterCustomSkin");
                }
            }

            return false;
        }

        private static bool CallsWithBoolean(MethodDefinition method, string callName, bool expected)
        {
            var instructions = method.Body.Instructions;
            for (var index = 1; index < instructions.Count; index++)
            {
                if (!(instructions[index].Operand is MethodReference call) ||
                    call.Name != callName ||
                    call.Parameters.Count == 0 ||
                    call.Parameters[call.Parameters.Count - 1].ParameterType.FullName != "System.Boolean")
                {
                    continue;
                }

                if (TryGetBooleanConstant(instructions[index - 1], out var actual) && actual == expected)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetBooleanConstant(Instruction instruction, out bool value)
        {
            if (instruction.OpCode.Code == Code.Ldc_I4_0)
            {
                value = false;
                return true;
            }

            if (instruction.OpCode.Code == Code.Ldc_I4_1)
            {
                value = true;
                return true;
            }

            value = false;
            return false;
        }

        private static bool UpdateCombinedSkinIsGuarded(MethodDefinition method)
        {
            var instructions = method.Body.Instructions;
            var updateIndex = instructions
                .Select((instruction, index) => new { instruction, index })
                .Where(item =>
                    item.instruction.Operand is MethodReference call &&
                    call.Name == "UpdateCombinedSkin")
                .Select(item => item.index)
                .DefaultIfEmpty(-1)
                .Single();
            if (updateIndex < 0)
            {
                return false;
            }

            return instructions.Take(updateIndex).Any(instruction =>
                instruction.OpCode.FlowControl == FlowControl.Cond_Branch);
        }

        private static MethodDefinition GetLegacyMethod(ModuleDefinition module, string name)
        {
            return module.Types
                .Single(type => type.FullName == "RatopiaMod.CustomMOD")
                .Methods
                .Single(method => method.Name == name);
        }
    }
}
