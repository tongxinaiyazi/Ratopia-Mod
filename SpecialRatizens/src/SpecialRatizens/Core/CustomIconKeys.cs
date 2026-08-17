using System;

namespace SpecialRatizens.Core
{
    internal static class CustomIconKeys
    {
        private const string TraitPrefix = "SpecialRatizens.Icon.";

        internal static string ForTrait(string traitName)
        {
            if (string.IsNullOrWhiteSpace(traitName))
            {
                throw new ArgumentException("特殊能力名称不能为空。", nameof(traitName));
            }

            return TraitPrefix + traitName.Trim();
        }

        internal static string ForCharacterIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "特性索引不能为负数。");
            }

            return $"Icon_Char{index}";
        }
    }
}
