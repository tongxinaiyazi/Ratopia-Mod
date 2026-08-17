using System;
using System.IO;
using SpecialRatizens.Core;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class SpecialDataCatalogTests
    {
        [Fact]
        public void LoadsACompleteCatalogAndResolvesTraitAndIconReferences()
        {
            using (var fixture = CatalogFixture.CreateValid())
            {
                var catalog = SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory);

                Assert.Single(catalog.Ratizens);
                Assert.Equal(2, catalog.Traits.Count);
                Assert.Equal("Rat_A", catalog.Ratizens[0].Trait1);
                Assert.Equal("Rat_B", catalog.Ratizens[0].Trait2);
            }
        }

        [Fact]
        public void RejectsDuplicateRatizenNamesBeforeReturningAnyCatalog()
        {
            using (var fixture = CatalogFixture.CreateValid())
            {
                File.AppendAllText(fixture.UnitsPath, File.ReadAllLines(fixture.UnitsPath)[1] + Environment.NewLine);

                var error = Assert.Throws<InvalidDataException>(() =>
                    SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory));
                Assert.Contains("重复", error.Message);
            }
        }

        [Theory]
        [InlineData("Sideways", "Unlock", "性别")]
        [InlineData("Male", "Maybe", "LockStatus")]
        public void RejectsInvalidEnums(string gender, string lockStatus, string expectedMessage)
        {
            using (var fixture = CatalogFixture.CreateValid(gender, lockStatus))
            {
                var error = Assert.Throws<InvalidDataException>(() =>
                    SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory));
                Assert.Contains(expectedMessage, error.Message);
            }
        }

        [Theory]
        [InlineData(" Male ", "Male")]
        [InlineData(" Female ", "Female")]
        public void TrimsValidGenderValues(string source, string expected)
        {
            using (var fixture = CatalogFixture.CreateValid(source))
            {
                var catalog = SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory);

                Assert.Equal(expected, catalog.Ratizens[0].Gender);
            }
        }

        [Fact]
        public void RejectsNegativeProbabilityAndMissingIcons()
        {
            using (var fixture = CatalogFixture.CreateValid(probability: -1, createSecondIcon: false))
            {
                var error = Assert.Throws<InvalidDataException>(() =>
                    SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory));
                Assert.True(error.Message.Contains("概率") || error.Message.Contains("图标"));
            }
        }

        [Fact]
        public void RejectsTraitsOwnedByMultipleRatizens()
        {
            using (var fixture = CatalogFixture.CreateValid())
            {
                File.AppendAllText(
                    fixture.UnitsPath,
                    "Second,#ffffff,Unlock,Female,1,2,3,4,5,Rat_A,IconA,Rat_B,IconB,10,White,Face_1,,Dress_1,,Hair_1,," +
                    Environment.NewLine);

                var error = Assert.Throws<InvalidDataException>(() =>
                    SpecialDataCatalog.Load(fixture.UnitsPath, fixture.TraitsPath, fixture.IconDirectory));

                Assert.Contains("多个特殊鼠鼠", error.Message);
            }
        }

        private sealed class CatalogFixture : IDisposable
        {
            private CatalogFixture(string root)
            {
                Root = root;
                UnitsPath = Path.Combine(root, "CustomSpecialUnit.csv");
                TraitsPath = Path.Combine(root, "CustomCharInfo.csv");
                IconDirectory = Path.Combine(root, "Icon");
            }

            public string Root { get; }
            public string UnitsPath { get; }
            public string TraitsPath { get; }
            public string IconDirectory { get; }

            public static CatalogFixture CreateValid(
                string gender = "Male",
                string lockStatus = "Unlock",
                int probability = 10,
                bool createSecondIcon = true)
            {
                var fixture = new CatalogFixture(Path.Combine(Path.GetTempPath(), "SpecialRatizensTests", Guid.NewGuid().ToString("N")));
                Directory.CreateDirectory(fixture.IconDirectory);
                File.WriteAllText(fixture.TraitsPath,
                    "Category,Name,T_Name,EffectValue_A,EffectValue_B,Description\n" +
                    "0,Rat_A,A,1,2,First\n" +
                    "1,Rat_B,B,3,4,Second\n");
                File.WriteAllText(fixture.UnitsPath,
                    "name,nameColor,LockStatus,UnitGender,grade,pow,dex,wit,gold,char1,icon1,char2,icon2,probability,skin,face,bread,dress,glasses,hair,hat,makeup\n" +
                    $"Test,#ffffff,{lockStatus},{gender},1,2,3,4,5,Rat_A,IconA,Rat_B,IconB,{probability},White,Face_1,,Dress_1,,Hair_1,,\n");
                File.WriteAllBytes(Path.Combine(fixture.IconDirectory, "IconA.png"), new byte[] { 1 });
                if (createSecondIcon)
                {
                    File.WriteAllBytes(Path.Combine(fixture.IconDirectory, "IconB.png"), new byte[] { 2 });
                }
                return fixture;
            }

            public void Dispose()
            {
                if (Directory.Exists(Root))
                {
                    Directory.Delete(Root, true);
                }
            }
        }
    }
}
