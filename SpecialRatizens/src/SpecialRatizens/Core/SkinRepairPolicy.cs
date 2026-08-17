using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecialRatizens.Core
{
    internal enum SkinRecoveryKind
    {
        Snapshot,
        Default
    }

    internal static class SkinRepairPolicy
    {
        internal static readonly string[] RequiredCategories =
        {
            "Skin",
            "Face",
            "Hair",
            "Dress"
        };

        internal static bool HasRequiredAppearance(IDictionary<string, string> skins)
        {
            return skins != null && RequiredCategories.All(category =>
                skins.TryGetValue(category, out string value) &&
                !string.IsNullOrWhiteSpace(value));
        }

        internal static string[] MissingRequiredCategories(IDictionary<string, string> skins)
        {
            return RequiredCategories.Where(category =>
                skins == null ||
                !skins.TryGetValue(category, out string value) ||
                string.IsNullOrWhiteSpace(value)).ToArray();
        }

        internal static SkinRecoveryKind SelectRecovery(IDictionary<string, string> snapshot)
        {
            return HasRequiredAppearance(snapshot)
                ? SkinRecoveryKind.Snapshot
                : SkinRecoveryKind.Default;
        }
    }
}
