using System.Collections.Generic;
using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class PanelStateCoordinatorTests
    {
        [Fact]
        public void Detaching_a_view_then_rebinding_the_same_context_preserves_the_exact_selection()
        {
            var coordinator = new PanelStateCoordinator();
            var firstPanel = new RecordingPanelSink();
            var secondPanel = new RecordingPanelSink();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };

            coordinator.Attach(firstPanel);
            coordinator.Refresh(4, 1, candidates, firstPanel);
            Assert.True(coordinator.TrySelect(1));

            Assert.True(coordinator.Detach(firstPanel));
            coordinator.Attach(secondPanel);
            coordinator.Refresh(4, 1, candidates, secondPanel);

            Assert.Equal(new ReforgeCandidate(11, 2f), coordinator.CurrentSelection);
        }

        [Fact]
        public void Clearing_then_reopening_uses_a_fresh_panel_and_selection_session()
        {
            var coordinator = new PanelStateCoordinator();
            var firstPanel = new RecordingPanelSink();
            var secondPanel = new RecordingPanelSink();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };

            coordinator.Attach(firstPanel);
            coordinator.Refresh(4, 1, candidates, firstPanel);
            Assert.True(coordinator.TrySelect(1));

            coordinator.Clear();
            Assert.Null(coordinator.CurrentPanel);
            Assert.Null(coordinator.CurrentSelection);
            Assert.Empty(coordinator.Candidates);

            coordinator.Attach(secondPanel);
            coordinator.Refresh(4, 1, candidates, secondPanel);

            Assert.Same(secondPanel, coordinator.CurrentPanel);
            Assert.Equal(new ReforgeCandidate(10, 1f), coordinator.CurrentSelection);
            Assert.False(coordinator.Detach(firstPanel));
            Assert.Same(secondPanel, coordinator.CurrentPanel);
        }

        [Fact]
        public void Resetting_the_session_keeps_the_current_view_but_clears_candidates_and_selection()
        {
            var coordinator = new PanelStateCoordinator();
            var panel = new RecordingPanelSink();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };
            coordinator.Attach(panel);
            coordinator.Refresh(4, 1, candidates, panel);
            Assert.True(coordinator.TrySelect(1));

            coordinator.ResetSession();

            Assert.Same(panel, coordinator.CurrentPanel);
            Assert.Empty(coordinator.Candidates);
            Assert.Null(coordinator.CurrentSelection);
        }

        [Fact]
        public void Failed_refresh_renders_empty_rows_before_showing_the_fallback_warning()
        {
            var coordinator = new PanelStateCoordinator();
            var panel = new RecordingPanelSink();
            coordinator.Attach(panel);
            coordinator.Refresh(
                4,
                1,
                new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) },
                panel);
            panel.Events.Clear();

            coordinator.RefreshFailed("候选数据已失效", panel);

            Assert.Empty(coordinator.Candidates);
            Assert.Null(coordinator.CurrentSelection);
            Assert.Equal(
                new[] { "render:0:none", "status:使用原版随机：候选数据已失效:warning" },
                panel.Events);
        }

        [Fact]
        public void Selecting_the_second_duplicate_ability_keeps_the_exact_candidate_through_the_coordinator()
        {
            var coordinator = new PanelStateCoordinator();
            var panel = new RecordingPanelSink();
            var candidates = new[] { new ReforgeCandidate(11, 2f), new ReforgeCandidate(11, 3f) };
            coordinator.Attach(panel);
            coordinator.Refresh(4, 1, candidates, panel);

            Assert.True(coordinator.TrySelect(1));
            Assert.Equal(new ReforgeCandidate(11, 3f), coordinator.CurrentSelection);
        }

        private sealed class RecordingPanelSink : IPanelStateSink
        {
            public List<string> Events { get; } = new List<string>();

            public void Render(IReadOnlyList<ReforgeCandidate> candidates, ReforgeCandidate? selected)
            {
                Events.Add($"render:{candidates.Count}:{(selected.HasValue ? selected.Value.AbilityId.ToString() : "none")}");
            }

            public void ShowStatus(string message, bool warning)
            {
                Events.Add($"status:{message}:{(warning ? "warning" : "normal")}");
            }
        }
    }
}
