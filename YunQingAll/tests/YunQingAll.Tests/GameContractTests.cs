using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace RatopiaMod.YunQing.All.Tests
{
    public sealed class GameContractTests
    {
        private const string ExpectedAssemblySha256 =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";

        [Fact]
        public void AssemblyCSharpMatchesInspectedBuild()
        {
            using (var stream = File.OpenRead(GetGameAssemblyPath()))
            using (var sha = SHA256.Create())
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
                Assert.Equal(ExpectedAssemblySha256, actual);
            }
        }

        [Fact]
        public void HarmonyTargetsKeepExactSignatures()
        {
            using (var module = ModuleDefinition.ReadModule(GetGameAssemblyPath()))
            {
                AssertMethod(module, "Fish", "DrownCheck", "System.Void");
                AssertMethod(module, "Monkfish", "DrownCheck", "System.Void");
                AssertMethod(
                    module,
                    "CasselGames.Diplomatic.Data.DiplomaticExchangeData",
                    "GetRandomTicket",
                    "CasselGames.Diplomatic.Data.DiplomaticExchangeTicketData");
                AssertMethod(
                    module,
                    "CasselGames.Diplomatic.Data.DiplomaticExchangeData",
                    "get_DefaultDarValue",
                    "System.Single");
            }
        }

        [Fact]
        public void ExchangeTicketFieldAndRatePropertyKeepExactContracts()
        {
            using (var module = ModuleDefinition.ReadModule(GetGameAssemblyPath()))
            {
                var exchangeData = FindType(module, "CasselGames.Diplomatic.Data.DiplomaticExchangeData");
                var ticketList = exchangeData.Fields.Single(field => field.Name == "_exchangeTicketList");
                Assert.True(ticketList.IsPrivate);
                Assert.Equal(
                    "System.Collections.Generic.List`1<CasselGames.Diplomatic.Data.DiplomaticExchangeTicketData>",
                    ticketList.FieldType.FullName);

                var ticket = FindType(module, "CasselGames.Diplomatic.Data.DiplomaticExchangeTicketData");
                var exchangeRate = ticket.Properties.Single(property => property.Name == "ExchangeRate");
                Assert.Equal("System.Single", exchangeRate.PropertyType.FullName);
                Assert.NotNull(exchangeRate.GetMethod);
            }
        }

        [Fact]
        public void SourceCompatibilityHelpersStillExist()
        {
            using (var module = ModuleDefinition.ReadModule(GetGameAssemblyPath()))
            {
                AssertMethod(
                    module,
                    "AnimalBody",
                    "BeAttacked",
                    "System.Void",
                    "System.Single",
                    "Unit_Attacekd_Tag");
                AssertMethod(
                    module,
                    "Extensions.Extension",
                    "Shuffle",
                    "System.Void",
                    "System.Collections.Generic.List`1<T>");
            }
        }

        [Fact]
        public void LoaderAssembliesMatchTheMigrationTarget()
        {
            var gameRoot = GetRatopiaDirectory();
            var bepInEx = AssemblyName.GetAssemblyName(
                Path.Combine(gameRoot, "BepInEx", "core", "BepInEx.dll"));
            var harmony = AssemblyName.GetAssemblyName(
                Path.Combine(gameRoot, "BepInEx", "core", "0Harmony.dll"));

            Assert.Equal(new Version(5, 4, 23, 5), bepInEx.Version);
            Assert.Equal(new Version(2, 9, 0, 0), harmony.Version);
        }

        private static string GetGameAssemblyPath()
        {
            var path = Path.Combine(
                GetRatopiaDirectory(),
                "Ratopia_Data",
                "Managed",
                "Assembly-CSharp.dll");
            Assert.True(File.Exists(path), $"Assembly-CSharp.dll not found: {path}");
            return path;
        }

        private static string GetRatopiaDirectory()
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
            Assert.True(Directory.Exists(ratopiaDir), $"Ratopia directory not found: {ratopiaDir}");
            return ratopiaDir;
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
                item.Name == methodName
                && item.ReturnType.FullName == returnType
                && item.Parameters.Select(parameter => parameter.ParameterType.FullName)
                    .SequenceEqual(parameterTypes));
            Assert.NotNull(method);
        }
    }
}
