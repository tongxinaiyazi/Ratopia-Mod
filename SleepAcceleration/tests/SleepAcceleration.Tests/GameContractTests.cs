using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace SleepAcceleration.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesTheInspectedBuild()
        {
            using (var stream = File.OpenRead(ContractTestPaths.GameAssembly))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void QueenBedAndSleepingEnumsMatchTheInspectedBuild()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                AssertEnumValue(module, "BuildAbility", "QueenBed", 62);
                AssertEnumValue(module, "CharState", "Queen_Action", 12);
                AssertEnumValue(module, "AniState", "Sleep_bed", 31);
            }
        }

        [Fact]
        public void RuntimeTargetsAndSpeedApisKeepTheirSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                var queen = module.Types.Single(type => type.FullName == "T_Queen");
                var update = AssertMethod(queen, "Update", "System.Void");
                Assert.True(update.IsPrivate);
                Assert.False(update.IsStatic);

                var systemManager = module.Types.Single(type => type.FullName == "SystemMgr");
                var setTimeScale = AssertMethod(systemManager, "SetTimeScale", "System.Void", "System.Single");
                var applyUserSpeed = AssertMethod(
                    systemManager,
                    "ApplyUserGameSpeed",
                    "System.Void",
                    "System.Single");
                Assert.True(setTimeScale.IsPublic);
                Assert.True(applyUserSpeed.IsPublic);

                AssertPublicField(module, "GameUnit", "m_AniState", "AniState");
                AssertPublicField(module, "GameUnit", "m_CharState", "CharState");
                AssertPublicField(module, "PlayDataMgr", "m_UserGameSpeed", "System.Single");
            }
        }

        [Fact]
        public void OriginalQueenBedInteractionEntersTheExpectedActionAndSleepFlow()
        {
            using (var module = ModuleDefinition.ReadModule(ContractTestPaths.GameAssembly))
            {
                var queen = module.Types.Single(type => type.FullName == "T_Queen");
                var update = AssertMethod(queen, "Update", "System.Void");
                AssertCalls(update, "T_Queen", "BedInteraction");
                AssertReadsField(update, "BuildInfo", "Ability");
                AssertLoadsInteger(update, 62);

                var bedInteraction = AssertMethod(queen, "BedInteraction", "System.Void", "Building");
                AssertCalls(bedInteraction, "GameUnit", "SetCharState");
                AssertLoadsInteger(bedInteraction, 12);
                AssertCalls(bedInteraction, "T_Queen", "FatigueUpdateC");
            }
        }

        private static void AssertEnumValue(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            int expected)
        {
            var field = module.Types.Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.Equal(expected, Convert.ToInt32(field.Constant));
        }

        private static MethodDefinition AssertMethod(
            TypeDefinition type,
            string name,
            string returnType,
            params string[] parameterTypes)
        {
            var method = type.Methods.Single(item =>
                item.Name == name &&
                item.Parameters.Select(parameter => parameter.ParameterType.FullName)
                    .SequenceEqual(parameterTypes));
            Assert.Equal(returnType, method.ReturnType.FullName);
            return method;
        }

        private static void AssertPublicField(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            string fieldType)
        {
            var field = module.Types.Single(type => type.FullName == typeName)
                .Fields.Single(item => item.Name == fieldName);
            Assert.True(field.IsPublic);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static void AssertCalls(
            MethodDefinition method,
            string declaringType,
            string methodName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is MethodReference called &&
                called.DeclaringType.FullName == declaringType &&
                called.Name == methodName);
        }

        private static void AssertReadsField(
            MethodDefinition method,
            string declaringType,
            string fieldName)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.Operand is FieldReference field &&
                field.DeclaringType.FullName == declaringType &&
                field.Name == fieldName);
        }

        private static void AssertLoadsInteger(MethodDefinition method, int value)
        {
            Assert.Contains(method.Body.Instructions, instruction =>
                instruction.OpCode.Code >= Mono.Cecil.Cil.Code.Ldc_I4_M1 &&
                instruction.OpCode.Code <= Mono.Cecil.Cil.Code.Ldc_I4 &&
                ReadLoadedInteger(instruction) == value);
        }

        private static int ReadLoadedInteger(Mono.Cecil.Cil.Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Mono.Cecil.Cil.Code.Ldc_I4_M1: return -1;
                case Mono.Cecil.Cil.Code.Ldc_I4_0: return 0;
                case Mono.Cecil.Cil.Code.Ldc_I4_1: return 1;
                case Mono.Cecil.Cil.Code.Ldc_I4_2: return 2;
                case Mono.Cecil.Cil.Code.Ldc_I4_3: return 3;
                case Mono.Cecil.Cil.Code.Ldc_I4_4: return 4;
                case Mono.Cecil.Cil.Code.Ldc_I4_5: return 5;
                case Mono.Cecil.Cil.Code.Ldc_I4_6: return 6;
                case Mono.Cecil.Cil.Code.Ldc_I4_7: return 7;
                case Mono.Cecil.Cil.Code.Ldc_I4_8: return 8;
                case Mono.Cecil.Cil.Code.Ldc_I4_S:
                case Mono.Cecil.Cil.Code.Ldc_I4:
                    return Convert.ToInt32(instruction.Operand);
                default:
                    return int.MinValue;
            }
        }
    }
}
