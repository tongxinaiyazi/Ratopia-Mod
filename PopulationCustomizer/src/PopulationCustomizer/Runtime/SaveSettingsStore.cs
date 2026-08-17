using PopulationCustomizer.Core;
using Utility.Savable;

namespace PopulationCustomizer.Runtime
{
    internal static class SaveSettingsStore
    {
        internal const string SettingsKey = "cn.ratopia.populationcustomizer.settings";

        internal static LimitSettings LoadCurrent(out bool malformed)
        {
            malformed = false;
            var data = PlayDataMgr.Instance?.m_GameData;
            var modsData = data?.ModsData;
            if (modsData == null || !modsData.HasKey(SettingsKey))
            {
                return LimitSettings.Vanilla;
            }

            var raw = modsData.GetValue<string>(SettingsKey, null);
            if (SettingsCodec.TryDeserialize(raw, out var settings))
            {
                return settings;
            }

            malformed = true;
            return LimitSettings.Vanilla;
        }

        internal static bool TrySaveCurrent(LimitSettings settings)
        {
            var data = PlayDataMgr.Instance?.m_GameData;
            if (data == null)
            {
                return false;
            }

            if (data.ModsData == null)
            {
                data.ModsData = SavableData.Create();
            }

            var serialized = SettingsCodec.Serialize(settings);
            data.ModsData.AddData(SettingsKey, serialized);
            var stored = data.ModsData.GetValue<string>(SettingsKey, null);
            return string.Equals(stored, serialized, System.StringComparison.Ordinal);
        }

        internal static bool TryRemoveCurrent()
        {
            var data = PlayDataMgr.Instance?.m_GameData;
            if (data == null)
            {
                return false;
            }

            if (data.ModsData != null && data.ModsData.HasKey(SettingsKey))
            {
                data.ModsData.Remove(SettingsKey);
            }

            return true;
        }
    }
}
