using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Xunit;

namespace EquipmentReforgeDodge.Tests
{
    /// <summary>
    /// 用 Mono.Cecil 校验被补丁的游戏方法/字段签名，防止游戏更新后补丁静默失效。
    /// </summary>
    public sealed class GameAssemblyContractTests
    {
        [Fact]
        public void Harmony_targets_have_exact_signatures()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                AssertMethod(assembly, "DB_Mgr", "Awake", "System.Void");
                AssertMethod(
                    assembly,
                    "T_Queen",
                    "ResAbil_Value_Calculate",
                    "System.Void",
                    "Res_Ability",
                    "System.Single&");
                AssertMethod(
                    assembly,
                    "Helpers",
                    "GetToolTipString2",
                    "System.String",
                    "Res_Ability",
                    "System.Single");
                AssertMethod(
                    assembly,
                    "T_Queen",
                    "GetEnhancValue",
                    "System.Single",
                    "Res_Ability");
            }
        }

        [Fact]
        public void Runtime_fields_match_the_injector_adapters()
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath))
            {
                AssertField(assembly, "DB_Mgr", "m_ItemEnhanceDB", "DatabaseItemEnhance");
                AssertField(assembly, "DB_Mgr", "List_AccessoryDB", "System.Collections.Generic.List\u00601<ItemInfo>");
                AssertField(assembly, "DatabaseItemEnhance", "_list", "System.Collections.Generic.List\u00601<ItemEnhanceInfo>");
                AssertField(assembly, "ItemInfo", "m_Type", "System.Int32");
                AssertField(assembly, "ItemInfo", "Category", "ItemCategory");
                AssertField(assembly, "ItemEnhanceInfo", "Type", "System.Int32");
                AssertField(assembly, "ItemEnhanceInfo", "List_Ability1", "System.Collections.Generic.List\u00601<Res_Ability>");
                AssertField(assembly, "ItemEnhanceInfo", "List_AbilityValue1", "System.Collections.Generic.List\u00601<System.Single>");
                AssertField(assembly, "ItemEnhanceInfo", "List_Ability2", "System.Collections.Generic.List\u00601<Res_Ability>");
                AssertField(assembly, "ItemEnhanceInfo", "List_AbilityValue2", "System.Collections.Generic.List\u00601<System.Single>");
                AssertField(assembly, "GameUnit", "m_Dodge", "System.Single");

                var dodgeField = RequireType(assembly, "Res_Ability").Fields.Single(field => field.Name == "Dodge");
                Assert.True(dodgeField.HasConstant, "Res_Ability.Dodge 应为常量枚举值。");
                Assert.Equal(200, dodgeField.Constant);
            }
        }

        private static string AssemblyPath =>
            Path.Combine(ContractTestPaths.GameDirectory, "Ratopia_Data", "Managed", "Assembly-CSharp.dll");

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
