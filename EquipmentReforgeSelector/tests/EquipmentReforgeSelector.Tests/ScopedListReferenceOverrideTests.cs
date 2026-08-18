using System;
using System.Collections.Generic;
using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class ScopedListReferenceOverrideTests
    {
        [Fact]
        public void Scoped_override_type_is_available_to_adapters()
        {
            var overrideType = Type.GetType("EquipmentReforgeSelector.ScopedListReferenceOverride`2, EquipmentReforgeSelector");

            Assert.NotNull(overrideType);
        }

        [Fact]
        public void Scoped_override_exposes_a_constructor_for_two_list_references()
        {
            var overrideType = Type.GetType("EquipmentReforgeSelector.ScopedListReferenceOverride`2, EquipmentReforgeSelector");

            var constructor = Assert.Single(overrideType.GetConstructors());
            Assert.Equal(6, constructor.GetParameters().Length);
        }

        [Fact]
        public void Scope_replaces_both_references_with_singletons_and_restores_the_exact_original_references()
        {
            IList<int> abilities = new List<int> { 1, 2 };
            IList<float> values = new List<float> { 3f, 4f };
            var originalAbilities = abilities;
            var originalValues = values;

            var scope = new ScopedListReferenceOverride<int, float>(
                () => abilities, replacement => abilities = replacement,
                () => values, replacement => values = replacement,
                9, 10f);

            Assert.True(scope.IsApplied);
            Assert.Equal(new[] { 9 }, abilities);
            Assert.Equal(new[] { 10f }, values);

            scope.Dispose();

            Assert.Same(originalAbilities, abilities);
            Assert.Same(originalValues, values);
        }

        [Fact]
        public void Dispose_is_idempotent()
        {
            IList<int> abilities = new List<int> { 1 };
            IList<float> values = new List<float> { 2f };
            var originalAbilities = abilities;
            var originalValues = values;
            var scope = new ScopedListReferenceOverride<int, float>(
                () => abilities, replacement => abilities = replacement,
                () => values, replacement => values = replacement,
                9, 10f);

            scope.Dispose();
            scope.Dispose();

            Assert.Same(originalAbilities, abilities);
            Assert.Same(originalValues, values);
        }

        [Fact]
        public void Nested_scopes_restore_the_immediately_outer_references_then_the_original_references()
        {
            IList<int> abilities = new List<int> { 1 };
            IList<float> values = new List<float> { 2f };
            var originalAbilities = abilities;
            var originalValues = values;
            var outer = new ScopedListReferenceOverride<int, float>(
                () => abilities, replacement => abilities = replacement,
                () => values, replacement => values = replacement,
                9, 10f);
            var outerAbilities = abilities;
            var outerValues = values;
            var inner = new ScopedListReferenceOverride<int, float>(
                () => abilities, replacement => abilities = replacement,
                () => values, replacement => values = replacement,
                11, 12f);

            inner.Dispose();

            Assert.Same(outerAbilities, abilities);
            Assert.Same(outerValues, values);

            outer.Dispose();

            Assert.Same(originalAbilities, abilities);
            Assert.Same(originalValues, values);
        }

        [Fact]
        public void Scope_reports_failure_when_a_reference_cannot_be_replaced()
        {
            IList<int> abilities = new List<int> { 1 };
            IList<float> values = new List<float> { 2f };
            var originalAbilities = abilities;
            var originalValues = values;

            var scope = new ScopedListReferenceOverride<int, float>(
                () => abilities, replacement => { },
                () => values, replacement => values = replacement,
                9, 10f);

            Assert.False(scope.IsApplied);
            Assert.Same(originalAbilities, abilities);
            Assert.Same(originalValues, values);
        }
    }
}
