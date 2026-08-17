namespace PopulationCustomizer.Core
{
    internal sealed class LimitSettings
    {
        internal LimitSettings(bool citizenEnabled, int citizenLimit, bool ratronEnabled, int ratronLimit)
        {
            CitizenEnabled = citizenEnabled;
            CitizenLimit = citizenLimit;
            RatronEnabled = ratronEnabled;
            RatronLimit = ratronLimit;
        }

        internal bool CitizenEnabled { get; }

        internal int CitizenLimit { get; }

        internal bool RatronEnabled { get; }

        internal int RatronLimit { get; }

        internal static LimitSettings Vanilla => new LimitSettings(false, 0, false, 0);
    }
}
