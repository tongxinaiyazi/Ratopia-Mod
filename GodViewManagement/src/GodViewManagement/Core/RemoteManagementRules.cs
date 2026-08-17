namespace GodViewManagement
{
    internal sealed class RemoteClickContext
    {
        public RemoteClickContext(
            bool modeEnabled,
            bool sessionReady,
            bool gameLoading,
            bool settingsOpen,
            bool remotePanelOpen,
            bool otherPanelOpen,
            bool pointerOverUi,
            bool queenSafe,
            bool leftClickPressed)
        {
            ModeEnabled = modeEnabled;
            SessionReady = sessionReady;
            GameLoading = gameLoading;
            SettingsOpen = settingsOpen;
            RemotePanelOpen = remotePanelOpen;
            OtherPanelOpen = otherPanelOpen;
            PointerOverUi = pointerOverUi;
            QueenSafe = queenSafe;
            LeftClickPressed = leftClickPressed;
        }

        public bool ModeEnabled { get; }
        public bool SessionReady { get; }
        public bool GameLoading { get; }
        public bool SettingsOpen { get; }
        public bool RemotePanelOpen { get; }
        public bool OtherPanelOpen { get; }
        public bool PointerOverUi { get; }
        public bool QueenSafe { get; }
        public bool LeftClickPressed { get; }

        public RemoteClickContext With(
            bool? modeEnabled = null,
            bool? sessionReady = null,
            bool? gameLoading = null,
            bool? settingsOpen = null,
            bool? remotePanelOpen = null,
            bool? otherPanelOpen = null,
            bool? pointerOverUi = null,
            bool? queenSafe = null,
            bool? leftClickPressed = null)
        {
            return new RemoteClickContext(
                modeEnabled ?? ModeEnabled,
                sessionReady ?? SessionReady,
                gameLoading ?? GameLoading,
                settingsOpen ?? SettingsOpen,
                remotePanelOpen ?? RemotePanelOpen,
                otherPanelOpen ?? OtherPanelOpen,
                pointerOverUi ?? PointerOverUi,
                queenSafe ?? QueenSafe,
                leftClickPressed ?? LeftClickPressed);
        }
    }

    internal sealed class BuildingCandidate
    {
        public BuildingCandidate(bool isFinished, bool hasConfigurationUi, bool isWallpaper, bool isEnemyNexus)
        {
            IsFinished = isFinished;
            HasConfigurationUi = hasConfigurationUi;
            IsWallpaper = isWallpaper;
            IsEnemyNexus = isEnemyNexus;
        }

        public bool IsFinished { get; }
        public bool HasConfigurationUi { get; }
        public bool IsWallpaper { get; }
        public bool IsEnemyNexus { get; }
    }

    internal static class RemoteManagementRules
    {
        public static bool CanHandleClick(RemoteClickContext context)
        {
            return context != null
                && context.ModeEnabled
                && context.SessionReady
                && !context.GameLoading
                && !context.SettingsOpen
                && !context.RemotePanelOpen
                && !context.OtherPanelOpen
                && !context.PointerOverUi
                && context.QueenSafe
                && context.LeftClickPressed;
        }

        public static bool IsEligibleBuilding(BuildingCandidate candidate)
        {
            return candidate != null
                && candidate.IsFinished
                && candidate.HasConfigurationUi
                && !candidate.IsWallpaper
                && !candidate.IsEnemyNexus;
        }
    }
}
