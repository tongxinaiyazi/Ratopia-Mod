using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Mono.Cecil;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class GameAssemblyContractTests
    {
        private const string ExpectedAssemblyHash =
            "C94847D858EED368D2082E9715D0C79DD79309631BEF861C6BEBD467306D6E1D";
        private const string ExpectedItemAssetsHash =
            "847D342FF36CD479790B39B6BA0D4159076C9995126E509FDE93961999A016C0";

        [Fact]
        public void Game_code_and_item_assets_match_the_inspected_build()
        {
            Assert.Equal(ExpectedAssemblyHash, Sha256(AssemblyPath));
            Assert.Equal(
                ExpectedItemAssetsHash,
                Sha256(Path.Combine(ContractTestPaths.GameDirectory, "Ratopia_Data", "sharedassets2.assets")));
        }

        [Fact]
        public void Harmony_targets_have_exact_signatures()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                AssertMethod(assembly, "DB_Mgr", "Item_DB_Setting", "System.Void");
                AssertMethod(assembly, "DB_Mgr", "ItemEnhance_DB_Setting", "System.Void");
                AssertMethod(
                    assembly,
                    "Bow_Arrow",
                    "OnTriggerEnter2D",
                    "System.Void",
                    "UnityEngine.Collider2D");
                AssertMethod(assembly, "T_Queen", "Update", "System.Void");
                AssertMethod(assembly, "GameUnit", "GetMaxHP", "System.Single");
                AssertMethod(
                    assembly,
                    "BuildMidUI",
                    "ItemDetail_Open",
                    "System.Void",
                    "ItemInfo",
                    "System.Boolean",
                    "System.Boolean",
                    "System.Int32");
                AssertMethod(
                    assembly,
                    "T_Queen",
                    "ItemEnhance",
                    "System.String",
                    "ItemInfo",
                    "System.Int32",
                    "Res_Ability");
                AssertMethod(
                    assembly,
                    "Helpers",
                    "GetToolTipString",
                    "System.String",
                    "Res_Ability",
                    "System.Single",
                    "System.Boolean");
                AssertMethod(
                    assembly,
                    "Helpers",
                    "GetToolTipString2",
                    "System.String",
                    "Res_Ability",
                    "System.Single");
                AssertMethod(
                    assembly,
                    "AnimalBody",
                    "BeAttacked",
                    "System.Void",
                    "System.Single",
                    "Unit_Attacekd_Tag");
                AssertMethod(
                    assembly,
                    "MapObj",
                    "BeAttacked",
                    "System.Void",
                    "System.Single");
                AssertMethod(
                    assembly,
                    "Building",
                    "BeAttacked",
                    "System.Void",
                    "System.Single",
                    "UnityEngine.Vector2",
                    "Unit_Attacekd_Tag");
                AssertMethod(
                    assembly,
                    "DmgEffect",
                    "SetDmgEffect",
                    "System.Void",
                    "System.Int32",
                    "UnityEngine.Vector3",
                    "UnityEngine.Transform",
                    "System.Boolean",
                    "System.Int32");
            }
        }

        [Fact]
        public void Runtime_fields_match_the_combat_and_catalog_adapters()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                AssertField(assembly, "DB_Mgr", "List_WeaponDB", "System.Collections.Generic.List\u00601<ItemInfo>");
                AssertField(assembly, "DB_Mgr", "m_ItemEnhanceDB", "DatabaseItemEnhance");
                AssertField(assembly, "DatabaseItemEnhance", "_list", "System.Collections.Generic.List\u00601<ItemEnhanceInfo>");
                AssertField(assembly, "ItemInfo", "Index", "System.Int32");
                AssertField(assembly, "ItemInfo", "Name", "System.String");
                AssertField(assembly, "ItemInfo", "m_Type", "System.Int32");
                AssertField(assembly, "ItemInfo", "List_Ability", "System.Collections.Generic.List\u00601<Res_Ability>");
                AssertField(assembly, "ItemInfo", "List_AbilityValue", "System.Collections.Generic.List\u00601<System.Single>");
                AssertField(assembly, "ItemEnhanceInfo", "Type", "System.Int32");
                AssertField(assembly, "ItemEnhanceInfo", "List_Ability1", "System.Collections.Generic.List\u00601<Res_Ability>");
                AssertField(assembly, "ItemEnhanceInfo", "List_AbilityValue1", "System.Collections.Generic.List\u00601<System.Single>");
                AssertField(assembly, "ItemEnhanceInfo", "List_Ability2", "System.Collections.Generic.List\u00601<Res_Ability>");
                AssertField(assembly, "ItemEnhanceInfo", "List_AbilityValue2", "System.Collections.Generic.List\u00601<System.Single>");
                AssertField(assembly, "Bow_Arrow", "m_Master", "GameUnit");
                AssertField(assembly, "Bow_Arrow", "m_Dmg", "System.Single");
                AssertField(assembly, "Bow_Arrow", "IsHit", "System.Boolean");
                AssertField(assembly, "T_Queen", "m_WeaponInfo", "ItemInfo");
                AssertField(
                    assembly,
                    "T_Queen",
                    "Dic_ItemPlusEffect",
                    "System.Collections.Generic.Dictionary\u00602<System.Int32,System.Collections.Generic.List\u00601<ItemPlusInfo>>");
                AssertField(assembly, "T_UnitMgr", "List_AllEnemy", "System.Collections.Generic.List\u00601<GameEnemy>");
                AssertField(assembly, "GameMgr", "_T_UnitMgr", "T_UnitMgr");
                AssertField(assembly, "GameMgr", "_AnimalMgr", "AnimalMgr");
                AssertField(assembly, "GameMgr", "_MapObjMgr", "MapObjMgr");
                AssertField(assembly, "GameMgr", "_BuildingMgr", "BuildingMgr");
                AssertField(assembly, "AnimalMgr", "List_Animal", "System.Collections.Generic.List\u00601<AnimalBody>");
                AssertField(assembly, "MapObjMgr", "List_MapObj", "System.Collections.Generic.List\u00601<MapObj>");
                AssertField(assembly, "BuildingMgr", "List_Building", "System.Collections.Generic.List\u00601<Building>");
                AssertField(assembly, "GameEnemy", "m_EnemyInfo", "EnemyInfo");
                AssertField(assembly, "EnemyInfo", "m_Category", "EnemyCategory");
                AssertField(assembly, "GameUnit", "m_CurHP", "System.Single");
                AssertField(assembly, "GameUnit", "m_MaxHP", "System.Single");
                AssertField(assembly, "AnimalBody", "m_CurHP", "System.Single");
                AssertField(assembly, "AnimalBody", "m_MaxHP", "System.Single");
                AssertField(assembly, "MapObj", "m_CurHp", "System.Single");
                AssertField(assembly, "MapObj", "m_MaxHp", "System.Single");
                AssertField(assembly, "Building", "m_CurHP", "System.Single");
                AssertField(assembly, "Building", "m_MaxHP", "System.Single");
                AssertField(assembly, "Building", "m_Info", "BuildInfo");
                AssertField(assembly, "BuildInfo", "Name", "BuildingName");
            }
        }

        private static string AssemblyPath =>
            Path.Combine(ContractTestPaths.GameDirectory, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");

        private static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
            {
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static void AssertMethod(
            AssemblyDefinition assembly,
            string typeName,
            string methodName,
            string returnType,
            params string[] parameters)
        {
            var method = RequireType(assembly, typeName).Methods.Single(candidate =>
                candidate.Name == methodName &&
                candidate.Parameters.Select(parameter => parameter.ParameterType.FullName)
                    .SequenceEqual(parameters));
            Assert.Equal(returnType, method.ReturnType.FullName);
        }

        private static void AssertField(
            AssemblyDefinition assembly,
            string typeName,
            string fieldName,
            string fieldType)
        {
            Assert.Equal(
                fieldType,
                RequireType(assembly, typeName).Fields.Single(field => field.Name == fieldName).FieldType.FullName);
        }

        private static TypeDefinition RequireType(AssemblyDefinition assembly, string fullName)
        {
            return assembly.MainModule.Types.Single(type => type.FullName == fullName);
        }
    }
}
