using System;
using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    internal interface IPanelStateSink
    {
        void Render(IReadOnlyList<ReforgeCandidate> candidates, ReforgeCandidate? selected);

        void ShowStatus(string message, bool warning);
    }

    internal sealed class PanelStateCoordinator
    {
        private static readonly IReadOnlyList<ReforgeCandidate> EmptyCandidates = new ReforgeCandidate[0];

        private SelectionSession _session = new SelectionSession();

        public object CurrentPanel { get; private set; }

        public IReadOnlyList<ReforgeCandidate> Candidates { get; private set; } = EmptyCandidates;

        public ReforgeCandidate? CurrentSelection => _session.CurrentSelection;

        public void Attach(object panel)
        {
            if (panel == null)
            {
                throw new ArgumentNullException(nameof(panel));
            }

            CurrentPanel = panel;
        }

        public bool Detach(object panel)
        {
            if (!ReferenceEquals(CurrentPanel, panel))
            {
                return false;
            }

            CurrentPanel = null;
            return true;
        }

        public void Clear()
        {
            CurrentPanel = null;
            ResetSelection();
        }

        public void ResetSession()
        {
            ResetSelection();
        }

        public void Refresh(
            int itemIndex,
            int level,
            IReadOnlyList<ReforgeCandidate> candidates,
            IPanelStateSink panel)
        {
            RequireCurrentPanel(panel);
            Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
            var selected = _session.Update(itemIndex, level, candidates);
            panel.Render(candidates, selected);
        }

        public void RefreshFailed(string reason, IPanelStateSink panel)
        {
            RequireCurrentPanel(panel);
            ResetSelection();
            panel.Render(Candidates, null);
            panel.ShowStatus("使用原版随机：" + reason, true);
        }

        public bool TrySelect(int candidateIndex)
        {
            return _session.TrySelect(candidateIndex, Candidates);
        }

        private void RequireCurrentPanel(object panel)
        {
            if (!ReferenceEquals(CurrentPanel, panel))
            {
                throw new InvalidOperationException("Panel state updates must target the current panel.");
            }
        }

        private void ResetSelection()
        {
            Candidates = EmptyCandidates;
            _session = new SelectionSession();
        }
    }
}
