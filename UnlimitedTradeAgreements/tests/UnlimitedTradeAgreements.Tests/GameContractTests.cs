using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace UnlimitedTradeAgreements.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedHash =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyMatchesTheInspectedBuild()
        {
            using (var stream = File.OpenRead(TestPaths.RequireFile(TestPaths.GameAssembly)))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedHash, actual);
            }
        }

        [Fact]
        public void HarmonyTargetsAndUiFieldsKeepTheirExactContracts()
        {
            using (var module = ModuleDefinition.ReadModule(TestPaths.RequireFile(TestPaths.GameAssembly)))
            {
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "IsFullTradeAgreement",
                    "System.Boolean");
                AssertMethod(module,
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData",
                    "GetGoodsTradeCount",
                    "System.Int32");
                AssertMethod(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI",
                    "UpdateSlot",
                    "System.Void",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData");
                AssertMethod(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "Refresh",
                    "System.Void");

                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI",
                    "_newSlotUI",
                    "CasselGames.Diplomatic.UI.DiplomaticTradeSlotUI");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_country",
                    "CasselGames.Diplomatic.Data.DiplomaticCountryData");
                AssertPrivateField(module,
                    "CasselGames.Diplomatic.UI.DiplomaticWorldDetailUI",
                    "_tradeAgreementValueText",
                    "TMPro.TextMeshProUGUI");
            }
        }

        [Fact]
        public void UpdateSlotContainsExactlyOneSevenSlotLoopBoundary()
        {
            using (var module = ModuleDefinition.ReadModule(TestPaths.RequireFile(TestPaths.GameAssembly)))
            {
                var method = FindType(module,
                        "CasselGames.Diplomatic.UI.DiplomaticTradeLayoutUI")
                    .Methods.Single(item => item.Name == "UpdateSlot");
                var matches = new List<Instruction>();
                for (var index = 0; index < method.Body.Instructions.Count - 1; index++)
                {
                    if (method.Body.Instructions[index].OpCode.Code == Code.Ldc_I4_7 &&
                        (method.Body.Instructions[index + 1].OpCode.Code == Code.Blt ||
                         method.Body.Instructions[index + 1].OpCode.Code == Code.Blt_S))
                    {
                        matches.Add(method.Body.Instructions[index]);
                    }
                }

                Assert.Single(matches);
            }
        }

        private static void AssertMethod(
            ModuleDefinition module,
            string typeName,
            string methodName,
            string returnType,
            params string[] parameterTypes)
        {
            var method = FindType(module, typeName).Methods.Single(item =>
                item.Name == methodName &&
                item.ReturnType.FullName == returnType &&
                item.Parameters.Select(parameter => parameter.ParameterType.FullName)
                    .SequenceEqual(parameterTypes));
            Assert.NotNull(method);
        }

        private static void AssertPrivateField(
            ModuleDefinition module,
            string typeName,
            string fieldName,
            string fieldType)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == fieldName);
            Assert.True(field.IsPrivate);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return FindTypes(module.Types).Single(type => type.FullName == fullName);
        }

        private static IEnumerable<TypeDefinition> FindTypes(IEnumerable<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                yield return type;
                foreach (var nested in FindTypes(type.NestedTypes))
                {
                    yield return nested;
                }
            }
        }
    }
}
