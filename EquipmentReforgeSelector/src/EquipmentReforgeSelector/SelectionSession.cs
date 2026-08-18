using System.Collections.Generic;

namespace EquipmentReforgeSelector
{
    public sealed class SelectionSession
    {
        private int _itemIndex;
        private int _level;
        private bool _hasKey;

        public ReforgeCandidate? CurrentSelection { get; private set; }

        public ReforgeCandidate? Update(int itemIndex, int level, IReadOnlyList<ReforgeCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new System.ArgumentNullException(nameof(candidates));
            }

            var reuseSelection = _hasKey && _itemIndex == itemIndex && _level == level;
            _itemIndex = itemIndex;
            _level = level;
            _hasKey = true;

            if (candidates.Count == 0)
            {
                CurrentSelection = null;
                return CurrentSelection;
            }

            if (reuseSelection && CurrentSelection.HasValue &&
                TryFindPreservedCandidate(candidates, CurrentSelection.Value, out var preservedSelection))
            {
                CurrentSelection = preservedSelection;
                return CurrentSelection;
            }

            CurrentSelection = candidates[0];
            return CurrentSelection;
        }

        public bool TrySelect(int candidateIndex, IReadOnlyList<ReforgeCandidate> candidates)
        {
            if (candidates == null)
            {
                throw new System.ArgumentNullException(nameof(candidates));
            }

            if (candidateIndex < 0 || candidateIndex >= candidates.Count)
            {
                return false;
            }

            CurrentSelection = candidates[candidateIndex];
            return true;
        }

        private static bool TryFindPreservedCandidate(
            IReadOnlyList<ReforgeCandidate> candidates,
            ReforgeCandidate previousSelection,
            out ReforgeCandidate candidate)
        {
            for (var index = 0; index < candidates.Count; index++)
            {
                if (previousSelection == candidates[index])
                {
                    candidate = candidates[index];
                    return true;
                }
            }

            var matchingAbilityCount = 0;
            candidate = default(ReforgeCandidate);
            for (var index = 0; index < candidates.Count; index++)
            {
                if (candidates[index].AbilityId == previousSelection.AbilityId)
                {
                    candidate = candidates[index];
                    matchingAbilityCount++;
                }
            }

            if (matchingAbilityCount == 1)
            {
                return true;
            }

            candidate = default(ReforgeCandidate);
            return false;
        }
    }
}
