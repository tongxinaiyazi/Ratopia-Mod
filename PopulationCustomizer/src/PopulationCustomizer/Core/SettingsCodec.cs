using System;
using System.Globalization;

namespace PopulationCustomizer.Core
{
    internal static class SettingsCodec
    {
        internal static string Serialize(LimitSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return string.Join(
                "|",
                "v1",
                settings.CitizenEnabled ? "1" : "0",
                settings.CitizenLimit.ToString(CultureInfo.InvariantCulture),
                settings.RatronEnabled ? "1" : "0",
                settings.RatronLimit.ToString(CultureInfo.InvariantCulture));
        }

        internal static bool TryDeserialize(string text, out LimitSettings settings)
        {
            settings = LimitSettings.Vanilla;

            if (text == null)
            {
                return false;
            }

            var segments = text.Split('|');
            if (segments.Length != 5 || segments[0] != "v1")
            {
                return false;
            }

            if (!TryParseEnabled(segments[1], out var citizenEnabled) ||
                !LimitRules.TryParse(segments[2], out var citizenLimit) ||
                !TryParseEnabled(segments[3], out var ratronEnabled) ||
                !LimitRules.TryParse(segments[4], out var ratronLimit))
            {
                return false;
            }

            settings = new LimitSettings(citizenEnabled, citizenLimit, ratronEnabled, ratronLimit);
            return true;
        }

        private static bool TryParseEnabled(string text, out bool enabled)
        {
            if (text == "0")
            {
                enabled = false;
                return true;
            }

            if (text == "1")
            {
                enabled = true;
                return true;
            }

            enabled = false;
            return false;
        }
    }
}
