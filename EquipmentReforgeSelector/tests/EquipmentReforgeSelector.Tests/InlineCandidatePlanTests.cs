using System;
using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class InlineCandidatePlanTests
    {
        [Fact]
        public void Exact_duplicate_ability_value_is_the_only_selected_inline_row()
        {
            var candidates = new[] { new ReforgeCandidate(11, 2f), new ReforgeCandidate(11, 3f) };

            var plan = InlineCandidatePlan.Create(candidates, new ReforgeCandidate(11, 3f));

            Assert.False(plan.Rows[0].IsSelected);
            Assert.True(plan.Rows[1].IsSelected);
            Assert.Equal(1, plan.Rows[1].CandidateIndex);
        }

        [Fact]
        public void Empty_candidates_create_no_inline_rows()
        {
            var plan = InlineCandidatePlan.Create(new ReforgeCandidate[0], null);

            Assert.Empty(plan.Rows);
        }

        [Fact]
        public void Missing_selection_leaves_every_inline_row_unselected()
        {
            var plan = InlineCandidatePlan.Create(
                new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) },
                new ReforgeCandidate(12, 3f));

            Assert.All(plan.Rows, row => Assert.False(row.IsSelected));
        }

        [Fact]
        public void Candidate_indices_preserve_the_source_order()
        {
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };

            var plan = InlineCandidatePlan.Create(candidates, null);

            Assert.Equal(0, plan.Rows[0].CandidateIndex);
            Assert.Equal(candidates[0], plan.Rows[0].Candidate);
            Assert.Equal(1, plan.Rows[1].CandidateIndex);
            Assert.Equal(candidates[1], plan.Rows[1].Candidate);
        }

        [Fact]
        public void Null_candidates_are_rejected()
        {
            Assert.Throws<ArgumentNullException>(() => InlineCandidatePlan.Create(null, null));
        }
    }
}
