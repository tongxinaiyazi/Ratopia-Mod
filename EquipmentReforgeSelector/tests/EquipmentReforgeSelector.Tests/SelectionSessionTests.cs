using System;
using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class SelectionSessionTests
    {
        [Fact]
        public void Selection_session_type_is_available_to_adapters()
        {
            var sessionType = Type.GetType("EquipmentReforgeSelector.SelectionSession, EquipmentReforgeSelector");

            Assert.NotNull(sessionType);
        }

        [Fact]
        public void New_item_and_level_key_selects_the_first_candidate()
        {
            var session = new SelectionSession();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };

            var selected = session.Update(4, 1, candidates);

            Assert.Equal(new ReforgeCandidate(10, 1f), selected);
        }

        [Fact]
        public void Reusing_a_key_preserves_a_still_valid_selected_ability()
        {
            var session = new SelectionSession();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };
            session.Update(4, 1, candidates);
            Assert.True(session.TrySelect(1, candidates));

            var selected = session.Update(4, 1, new[] { new ReforgeCandidate(10, 3f), new ReforgeCandidate(11, 4f) });

            Assert.Equal(new ReforgeCandidate(11, 4f), selected);
        }

        [Fact]
        public void Changing_the_item_or_level_key_selects_the_first_candidate()
        {
            var session = new SelectionSession();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };
            session.Update(4, 1, candidates);
            Assert.True(session.TrySelect(1, candidates));

            var selected = session.Update(4, 2, candidates);

            Assert.Equal(new ReforgeCandidate(10, 1f), selected);
        }

        [Fact]
        public void Stale_selected_ability_for_the_same_key_falls_back_to_the_first_candidate()
        {
            var session = new SelectionSession();
            var candidates = new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) };
            session.Update(4, 1, candidates);
            Assert.True(session.TrySelect(1, candidates));

            var selected = session.Update(4, 1, new[] { new ReforgeCandidate(10, 3f), new ReforgeCandidate(12, 4f) });

            Assert.Equal(new ReforgeCandidate(10, 3f), selected);
        }

        [Fact]
        public void Empty_candidates_clear_the_selection()
        {
            var session = new SelectionSession();
            session.Update(4, 1, new[] { new ReforgeCandidate(10, 1f) });

            var selected = session.Update(4, 1, new ReforgeCandidate[0]);

            Assert.Null(selected);
            Assert.Null(session.CurrentSelection);
        }

        [Fact]
        public void Selecting_the_second_duplicate_ability_uses_its_exact_candidate_value()
        {
            var session = new SelectionSession();
            var candidates = new[] { new ReforgeCandidate(11, 2f), new ReforgeCandidate(11, 3f) };
            session.Update(4, 1, candidates);

            Assert.True(session.TrySelect(1, candidates));
            Assert.Equal(new ReforgeCandidate(11, 3f), session.CurrentSelection);
        }

        [Fact]
        public void Changing_item_index_at_the_same_level_clears_the_old_selection_and_defaults_to_the_first_candidate()
        {
            var session = new SelectionSession();
            session.Update(4, 1, new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) });
            Assert.True(session.TrySelect(1, new[] { new ReforgeCandidate(10, 1f), new ReforgeCandidate(11, 2f) }));

            var selected = session.Update(5, 1, new[] { new ReforgeCandidate(12, 4f), new ReforgeCandidate(13, 5f) });

            Assert.Equal(new ReforgeCandidate(12, 4f), selected);
        }
    }
}
