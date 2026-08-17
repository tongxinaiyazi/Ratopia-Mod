using System;

namespace EquipmentReforgeSelector
{
    internal sealed class OverrideState : IDisposable
    {
        private readonly ScopedListReferenceOverride<Res_Ability, float> _scope;

        public OverrideState(ScopedListReferenceOverride<Res_Ability, float> scope, ReforgeCandidate candidate, int itemIndex, int level)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Candidate = candidate;
            ItemIndex = itemIndex;
            Level = level;
        }

        public ReforgeCandidate Candidate { get; }

        public int ItemIndex { get; }

        public int Level { get; }

        public bool IsApplied => _scope.IsApplied;

        public bool UiDirty { get; set; }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
