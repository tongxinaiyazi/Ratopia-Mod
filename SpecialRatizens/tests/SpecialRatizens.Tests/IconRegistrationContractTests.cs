using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpecialRatizens.Tests
{
    public sealed class IconRegistrationContractTests
    {
        [Fact]
        public void RegistrationUsesTheConfiguredPluginIconDirectory()
        {
            var body = MethodBody("static void RegisterCustomInfoIcon(CharacterInfo info)");

            Assert.Contains("Path.Combine(CustomDataPath, \"Icon\"", body);
            Assert.DoesNotContain("CustomSetting_Data/Icon", body);
        }

        [Fact]
        public void RegistrationUsesAnOwnedIdempotentPrimaryAndIndexKey()
        {
            var body = MethodBody("static void RegisterCustomInfoIcon(CharacterInfo info)");

            Assert.Contains("CustomIconKeys.ForTrait(info.Name)", body);
            Assert.Contains("CustomIconKeys.ForCharacterIndex(info.Index)", body);
            Assert.Contains("DBMgr.GetCharacterInfo(info.Index)", body);
            Assert.Contains("sprites[iconKey] = sprite", body);
            Assert.Contains("sprites[indexKey] = sprite", body);
            Assert.DoesNotContain("if (DicSprits.ContainsKey(spriteKey))", body);
            Assert.True(body.IndexOf("DBMgr.GetCharacterInfo(info.Index)", StringComparison.Ordinal) <
                        body.IndexOf("sprites[indexKey] = sprite", StringComparison.Ordinal));
        }

        [Fact]
        public void BuffIconUsesTheRegisteredCustomTraitKey()
        {
            var body = MethodBody("public static void BuffIcon_IconSet(BuffIcon __instance, BuffInfo _info)");

            Assert.Contains("TryGetCustomCharInfo(_info.ReferenceName, out CustomCharInfo customInfo)", body);
            Assert.Contains("Func.Instance.LoadSprite(customInfo.iconKey)", body);
            Assert.DoesNotContain("$\"Icon_{_info.Name}\"", body);
        }

        private static string MethodBody(string signature)
        {
            var source = File.ReadAllText(Path.Combine(
                GetProjectRoot(), "src", "SpecialRatizens", "Legacy", "CustomMOD.cs"));
            var signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(signatureIndex >= 0, $"Method signature not found: {signature}");
            var openBrace = source.IndexOf('{', signatureIndex);
            Assert.True(openBrace >= 0);

            var depth = 0;
            for (var index = openBrace; index < source.Length; index++)
            {
                if (source[index] == '{') depth++;
                if (source[index] != '}') continue;
                depth--;
                if (depth == 0)
                {
                    return source.Substring(openBrace, index - openBrace + 1);
                }
            }

            throw new InvalidDataException($"Method body is incomplete: {signature}");
        }

        private static string GetProjectRoot()
        {
            return typeof(IconRegistrationContractTests).Assembly
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>()
                .Single(attribute => attribute.Key == "ProjectRoot")
                .Value;
        }
    }
}
