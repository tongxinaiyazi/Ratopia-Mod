using System;

namespace GodViewManagement
{
    internal enum BindingDecision
    {
        Accepted,
        Cancelled,
        ModifierOnly,
        Conflict
    }

    internal static class InputBindingRules
    {
        public static BindingDecision Evaluate(string keyName, bool conflicts)
        {
            if (string.Equals(keyName, "Escape", StringComparison.OrdinalIgnoreCase))
            {
                return BindingDecision.Cancelled;
            }

            if (IsModifierOnly(keyName))
            {
                return BindingDecision.ModifierOnly;
            }

            return conflicts ? BindingDecision.Conflict : BindingDecision.Accepted;
        }

        public static bool IsModifierOnly(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                return false;
            }

            return keyName.EndsWith("Shift", StringComparison.OrdinalIgnoreCase)
                || keyName.EndsWith("Ctrl", StringComparison.OrdinalIgnoreCase)
                || keyName.EndsWith("Alt", StringComparison.OrdinalIgnoreCase)
                || keyName.EndsWith("Meta", StringComparison.OrdinalIgnoreCase)
                || keyName.EndsWith("Windows", StringComparison.OrdinalIgnoreCase)
                || keyName.EndsWith("Command", StringComparison.OrdinalIgnoreCase)
                || keyName.EndsWith("Apple", StringComparison.OrdinalIgnoreCase)
                || string.Equals(keyName, "AltGr", StringComparison.OrdinalIgnoreCase);
        }
    }
}
