namespace PopulationCustomizer.Core
{
    internal static class LimitRules
    {
        internal const int Minimum = 0;
        internal const int Maximum = 999;

        internal static int Resolve(int vanillaLimit, bool customEnabled, int customLimit)
        {
            return customEnabled && IsValid(customLimit) ? customLimit : vanillaLimit;
        }

        internal static bool TryParse(string text, out int value)
        {
            value = 0;

            if (string.IsNullOrEmpty(text) || text.Length > 3)
            {
                return false;
            }

            var parsed = 0;
            for (var index = 0; index < text.Length; index++)
            {
                var character = text[index];
                if (character < '0' || character > '9')
                {
                    return false;
                }

                parsed = (parsed * 10) + (character - '0');
            }

            if (!IsValid(parsed))
            {
                return false;
            }

            value = parsed;
            return true;
        }

        private static bool IsValid(int value)
        {
            return value >= Minimum && value <= Maximum;
        }
    }
}
