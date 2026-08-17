using Xunit;

namespace GodViewManagement.Tests
{
    public sealed class RemoteManagementRulesTests
    {
        [Fact]
        public void ClickRequiresSafeUnblockedGodViewSession()
        {
            var allowed = new RemoteClickContext(
                modeEnabled: true,
                sessionReady: true,
                gameLoading: false,
                settingsOpen: false,
                remotePanelOpen: false,
                otherPanelOpen: false,
                pointerOverUi: false,
                queenSafe: true,
                leftClickPressed: true);

            Assert.True(RemoteManagementRules.CanHandleClick(allowed));
            Assert.False(RemoteManagementRules.CanHandleClick(allowed.With(pointerOverUi: true)));
            Assert.False(RemoteManagementRules.CanHandleClick(allowed.With(queenSafe: false)));
            Assert.False(RemoteManagementRules.CanHandleClick(allowed.With(gameLoading: true)));
            Assert.False(RemoteManagementRules.CanHandleClick(allowed.With(otherPanelOpen: true)));
        }

        [Fact]
        public void BuildingMustBeFinishedConfigurableAndNotExcluded()
        {
            Assert.True(RemoteManagementRules.IsEligibleBuilding(new BuildingCandidate(true, true, false, false)));
            Assert.False(RemoteManagementRules.IsEligibleBuilding(new BuildingCandidate(false, true, false, false)));
            Assert.False(RemoteManagementRules.IsEligibleBuilding(new BuildingCandidate(true, false, false, false)));
            Assert.False(RemoteManagementRules.IsEligibleBuilding(new BuildingCandidate(true, true, true, false)));
            Assert.False(RemoteManagementRules.IsEligibleBuilding(new BuildingCandidate(true, true, false, true)));
        }

        [Fact]
        public void OnlyExactRemotePanelContextBlocksQueenActions()
        {
            var state = new RemotePanelState();
            var panel = new object();
            var target = new object();
            state.Open(panel, target);

            Assert.True(state.ShouldBlockQueenAction(panel, target));
            Assert.False(state.ShouldBlockQueenAction(new object(), target));
            Assert.False(state.ShouldBlockQueenAction(panel, new object()));
        }

        [Fact]
        public void ExceptionCleanupRemovesRemoteGuard()
        {
            var state = new RemotePanelState();
            var panel = new object();
            var target = new object();
            state.Open(panel, target);

            state.Clear();

            Assert.False(state.IsOpen);
            Assert.False(state.ShouldBlockQueenAction(panel, target));
        }
    }
}
