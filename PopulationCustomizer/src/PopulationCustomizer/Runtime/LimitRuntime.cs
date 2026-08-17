using PopulationCustomizer.Core;

namespace PopulationCustomizer.Runtime
{
    internal static class LimitRuntime
    {
        private static LimitSettings _current = LimitSettings.Vanilla;

        internal static LimitSettings Current => _current;

        internal static int LastVanillaCitizenLimit { get; private set; }

        internal static int LastVanillaRatronLimit { get; private set; }

        internal static void Apply(LimitSettings settings)
        {
            _current = settings ?? LimitSettings.Vanilla;
        }

        internal static void Reset()
        {
            _current = LimitSettings.Vanilla;
            LastVanillaCitizenLimit = 0;
            LastVanillaRatronLimit = 0;
        }

        internal static int ResolveCitizen(int vanillaLimit)
        {
            LastVanillaCitizenLimit = vanillaLimit;
            return LimitRules.Resolve(vanillaLimit, _current.CitizenEnabled, _current.CitizenLimit);
        }

        internal static int ResolveRatron(int vanillaLimit)
        {
            LastVanillaRatronLimit = vanillaLimit;
            return LimitRules.Resolve(vanillaLimit, _current.RatronEnabled, _current.RatronLimit);
        }
    }
}
