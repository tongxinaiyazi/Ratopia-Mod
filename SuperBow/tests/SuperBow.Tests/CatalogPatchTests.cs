using System.Collections.Generic;
using SuperBow.Core;
using Xunit;

namespace SuperBow.Tests
{
    public sealed class CatalogPatchTests
    {
        [Fact]
        public void Pair_patch_appends_once_and_dispose_restores_alignment()
        {
            var abilities = new List<int> { 5, 206 };
            var values = new List<float> { 1f, 1f };

            Assert.True(PairedListAppendPatch<int>.TryApply(abilities, values, 215, 1f, out var patch));
            Assert.Equal(new[] { 5, 206, 215 }, abilities);
            Assert.Equal(new[] { 1f, 1f, 1f }, values);

            patch.Dispose();
            patch.Dispose();

            Assert.Equal(new[] { 5, 206 }, abilities);
            Assert.Equal(new[] { 1f, 1f }, values);
        }

        [Fact]
        public void Pair_patch_rejects_duplicate_or_mismatched_lists()
        {
            var duplicateAbilities = new List<int> { 215 };
            var duplicateValues = new List<float> { 1f };
            Assert.False(PairedListAppendPatch<int>.TryApply(
                duplicateAbilities, duplicateValues, 215, 1f, out _));

            Assert.False(PairedListAppendPatch<int>.TryApply(
                new List<int> { 5 }, new List<float>(), 215, 1f, out _));
        }

        [Fact]
        public void Pair_patch_does_not_remove_an_externally_changed_pair()
        {
            var abilities = new List<int> { 5 };
            var values = new List<float> { 1f };
            Assert.True(PairedListAppendPatch<int>.TryApply(abilities, values, 215, 1f, out var patch));

            values[1] = 9f;
            patch.Dispose();

            Assert.Equal(new[] { 5, 215 }, abilities);
            Assert.Equal(new[] { 1f, 9f }, values);
        }

        [Fact]
        public void Value_patch_restores_only_its_own_replacement()
        {
            var values = new List<float> { 2f };
            Assert.True(ListValuePatch.TryApply(values, 0, 3f, out var patch));
            Assert.Equal(3f, values[0]);
            patch.Dispose();
            Assert.Equal(2f, values[0]);

            Assert.True(ListValuePatch.TryApply(values, 0, 3f, out var guardedPatch));
            values[0] = 4f;
            guardedPatch.Dispose();
            Assert.Equal(4f, values[0]);
        }

        [Fact]
        public void Expected_value_patch_refuses_to_overwrite_another_mod()
        {
            var queenAttack = new List<float> { 2f };
            var otherWeaponAttack = new List<float> { 9f };

            Assert.True(ListValuePatch.TryApplyExpected(
                queenAttack, 0, 2f, 3f, out var patch));
            Assert.Equal(3f, queenAttack[0]);
            Assert.Equal(9f, otherWeaponAttack[0]);

            var externallyModifiedQueenAttack = new List<float> { 4f };
            Assert.False(ListValuePatch.TryApplyExpected(
                externallyModifiedQueenAttack, 0, 2f, 3f, out _));
            Assert.Equal(4f, externallyModifiedQueenAttack[0]);

            patch.Dispose();
            Assert.Equal(2f, queenAttack[0]);
        }

        [Fact]
        public void Expected_or_target_patch_accepts_an_unowned_target_value()
        {
            var alreadyTarget = new List<float> { 3f };

            Assert.True(ListValuePatch.TryApplyExpectedOrAlreadySet(
                alreadyTarget, 0, 2f, 3f, out var patch));
            Assert.Null(patch);
            Assert.Equal(3f, alreadyTarget[0]);

            Assert.False(ListValuePatch.TryApplyExpectedOrAlreadySet(
                new List<float> { 4f }, 0, 2f, 3f, out _));
        }
    }
}
