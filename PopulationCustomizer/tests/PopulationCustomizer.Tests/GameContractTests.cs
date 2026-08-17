using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace PopulationCustomizer.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesInspectedBuild()
        {
            using (var stream = File.OpenRead(GetAssemblyPath("Assembly-CSharp.dll")))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void HarmonyTargetsKeepExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath("Assembly-CSharp.dll")))
            {
                AssertMethod(module, "ProsperityUI", "GetMaxCitizenCount", "System.Int32");
                AssertMethod(module, "SystemMgr", "GetGBotMaxCount", "System.Int32");
                AssertMethod(module, "TileMgr", "Awake", "System.Void");
                AssertMethod(module, "CasselGames.UI.StatisticsCitizenListUI", "Initialize", "System.Void");
            }
        }

        [Fact]
        public void RuntimeFieldsKeepInspectedContract()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath("Assembly-CSharp.dll")))
            {
                AssertField(module, "D_Data", "ModsData", "Utility.Savable.SavableData");
                AssertProperty(module, "PlayDataMgr", "m_GameData", "D_Data");
                AssertField(module, "CitizenUI", "Txt_Num", "TMPro.TextMeshProUGUI");
                AssertField(module, "CasselGames.UI.StatisticsCitizenListUI", "_filterBtn", "UnityEngine.UI.Button");
                AssertField(module, "CasselGames.UI.StatisticsCitizenListUI", "_searchBtn", "UnityEngine.UI.Button");
                AssertField(module, "T_UnitMgr", "List_Citizen", "System.Collections.Generic.List`1<T_Citizen>");
                AssertField(module, "T_UnitMgr", "List_GBot", "System.Collections.Generic.List`1<GBot>");
                AssertProperty(module, "CasselGames.Input.InputMgr", "NowActionMapKey", "System.String");
                AssertMethod(module, "CasselGames.Input.InputMgr", "SetActionMap", "System.Void", "System.String");
                AssertMethod(module, "CasselGames.Input.InputMgr", "SetDefaultActionMap", "System.Void");
            }
        }

        [Fact]
        public void SavableDataKeepsRequiredApi()
        {
            using (var module = ModuleDefinition.ReadModule(GetAssemblyPath("Utility.Savable.SavableData.dll")))
            {
                AssertMethod(module, "Utility.Savable.SavableData", "Create", "Utility.Savable.SavableData");
                AssertMethod(module, "Utility.Savable.SavableData", "AddData", "System.Void", "System.String", "System.Object");
                AssertMethod(module, "Utility.Savable.SavableData", "HasKey", "System.Boolean", "System.String");
                AssertMethod(module, "Utility.Savable.SavableData", "Remove", "System.Void", "System.String");
            }
        }

        private static string GetAssemblyPath(string fileName)
        {
            var ratopiaDir = Environment.GetEnvironmentVariable("RATOPIA_DIR");
            if (string.IsNullOrWhiteSpace(ratopiaDir))
            {
                ratopiaDir = typeof(GameContractTests).Assembly
                    .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .Cast<AssemblyMetadataAttribute>()
                    .Single(attribute => attribute.Key == "RatopiaDir")
                    .Value;
            }

            Assert.False(string.IsNullOrWhiteSpace(ratopiaDir));
            var path = Path.Combine(ratopiaDir, "Ratopia_Data", "Managed", fileName);
            Assert.True(File.Exists(path), $"Game assembly not found: {path}");
            return path;
        }

        private static TypeDefinition FindType(ModuleDefinition module, string fullName)
        {
            return module.Types.Single(type => type.FullName == fullName);
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
                item.Parameters.Select(parameter => parameter.ParameterType.FullName).SequenceEqual(parameterTypes));
            Assert.NotNull(method);
        }

        private static void AssertField(ModuleDefinition module, string typeName, string name, string fieldType)
        {
            var field = FindType(module, typeName).Fields.Single(item => item.Name == name);
            Assert.Equal(fieldType, field.FieldType.FullName);
        }

        private static void AssertProperty(ModuleDefinition module, string typeName, string name, string propertyType)
        {
            var property = FindType(module, typeName).Properties.Single(item => item.Name == name);
            Assert.Equal(propertyType, property.PropertyType.FullName);
        }
    }
}
