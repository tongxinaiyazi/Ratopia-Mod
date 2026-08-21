using System.Collections.Generic;
using EquipmentReforgeDodge.Core;
using Xunit;

namespace EquipmentReforgeDodge.Tests
{
    public sealed class DodgeCandidateRulesTests
    {
        [Fact]
        public void TryAppendDodge_adds_ability_and_value_in_sync()
        {
            var abilities = new List<Res_Ability> { Res_Ability.STR, Res_Ability.DEF };
            var values = new List<float> { 2f, 3f };

            var result = DodgeCandidateRules.TryAppendDodge(abilities, values, 20f);

            Assert.True(result);
            Assert.Equal(new[] { Res_Ability.STR, Res_Ability.DEF, Res_Ability.Dodge }, abilities);
            Assert.Equal(new[] { 2f, 3f, 20f }, values);
        }

        [Fact]
        public void TryAppendDodge_is_idempotent_when_dodge_already_present()
        {
            var abilities = new List<Res_Ability> { Res_Ability.Dodge, Res_Ability.DEF };
            var values = new List<float> { 30f, 3f };

            var result = DodgeCandidateRules.TryAppendDodge(abilities, values, 20f);

            Assert.False(result);
            Assert.Equal(2, abilities.Count);
            Assert.Equal(2, values.Count);
        }

        [Fact]
        public void TryAppendDodge_rejects_mismatched_list_lengths_without_mutating()
        {
            var abilities = new List<Res_Ability> { Res_Ability.STR };
            var values = new List<float>();

            var result = DodgeCandidateRules.TryAppendDodge(abilities, values, 20f);

            Assert.False(result);
            Assert.Single(abilities);
            Assert.Empty(values);
        }

        [Fact]
        public void TryAppendDodge_rejects_null_lists()
        {
            Assert.False(DodgeCandidateRules.TryAppendDodge(null, new List<float>(), 20f));
            Assert.False(DodgeCandidateRules.TryAppendDodge(new List<Res_Ability>(), null, 20f));
        }

        [Fact]
        public void Contains_handles_null_and_missing_entries()
        {
            Assert.False(DodgeCandidateRules.Contains(null, Res_Ability.Dodge));
            Assert.False(DodgeCandidateRules.Contains(
                new List<Res_Ability> { Res_Ability.STR },
                Res_Ability.Dodge));
            Assert.True(DodgeCandidateRules.Contains(
                new List<Res_Ability> { Res_Ability.Dodge },
                Res_Ability.Dodge));
        }
    }
}
