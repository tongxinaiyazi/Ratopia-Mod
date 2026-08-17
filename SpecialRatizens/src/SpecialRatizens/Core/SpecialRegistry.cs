using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecialRatizens.Core
{
    internal sealed class SpecialRegistry
    {
        private readonly List<SpecialCandidateState> _candidates = new List<SpecialCandidateState>();
        private readonly Dictionary<string, SpecialCandidateState> _byTrait =
            new Dictionary<string, SpecialCandidateState>(StringComparer.Ordinal);

        public IReadOnlyList<SpecialCandidateState> Candidates => _candidates;

        public void Reload(IEnumerable<SpecialRatizenDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            _candidates.Clear();
            _byTrait.Clear();
            foreach (var definition in definitions.Where(item => item.IsUnlocked))
            {
                var candidate = new SpecialCandidateState(
                    definition.Name,
                    definition.Grade,
                    definition.Probability,
                    false);
                _candidates.Add(candidate);
                AddTrait(definition.Trait1, candidate);
                AddTrait(definition.Trait2, candidate);
            }
        }

        public void RebuildUsedFromTraits(IEnumerable<string> traitNames)
        {
            if (traitNames == null)
            {
                throw new ArgumentNullException(nameof(traitNames));
            }

            foreach (var traitName in traitNames)
            {
                if (_byTrait.TryGetValue(traitName, out var candidate))
                {
                    candidate.IsUsed = true;
                }
            }
        }

        public void ResetSession()
        {
            foreach (var candidate in _candidates)
            {
                candidate.IsUsed = false;
                candidate.ProbabilityBonus = 0;
            }
        }

        private void AddTrait(string traitName, SpecialCandidateState candidate)
        {
            if (_byTrait.ContainsKey(traitName))
            {
                throw new InvalidOperationException($"特性被多个特殊鼠鼠重复引用：{traitName}");
            }
            _byTrait.Add(traitName, candidate);
        }
    }
}
