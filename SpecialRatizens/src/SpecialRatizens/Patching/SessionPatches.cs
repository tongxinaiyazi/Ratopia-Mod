using RatopiaMod;

namespace SpecialRatizens.Patching
{
    internal static class SessionPatches
    {
        public static void TileMgrAllNotUseListClearPostfix()
        {
            Plugin.RunSafely("session.loaded", CustomMOD.SpecialRatizensSessionLoaded);
        }
    }
}
