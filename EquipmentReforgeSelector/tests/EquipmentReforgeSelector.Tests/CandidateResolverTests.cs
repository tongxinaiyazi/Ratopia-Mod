using System;
using System.Linq;
using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class CandidateResolverTests
    {
        [Fact]
        public void Resolver_type_is_available_to_adapters()
        {
            var resolverType = Type.GetType("EquipmentReforgeSelector.CandidateResolver, EquipmentReforgeSelector");

            Assert.NotNull(resolverType);
        }

        [Fact]
        public void Resolver_exposes_a_candidate_resolution_operation()
        {
            var resolverType = Type.GetType("EquipmentReforgeSelector.CandidateResolver, EquipmentReforgeSelector");
            var methods = resolverType.GetMethods().Where(method => method.Name == "Resolve");

            Assert.Single(methods);
        }

        [Fact]
        public void Resolve_pairs_abilities_and_values_by_index_in_source_order()
        {
            var resolution = CandidateResolver.Resolve(1, 99, new[] { 10, 11 }, new[] { 1.25f, 2.5f });

            Assert.True(resolution.IsAvailable);
            Assert.Equal(
                new[] { new ReforgeCandidate(10, 1.25f), new ReforgeCandidate(11, 2.5f) },
                resolution.Candidates);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public void Resolve_marks_unsupported_levels_as_unavailable(int level)
        {
            var resolution = CandidateResolver.Resolve(level, 99, new[] { 10 }, new[] { 1.25f });

            Assert.False(resolution.IsAvailable);
            Assert.Empty(resolution.Candidates);
        }

        [Fact]
        public void Resolve_marks_count_mismatches_as_unavailable_without_returning_a_partial_list()
        {
            var resolution = CandidateResolver.Resolve(1, 99, new[] { 10, 11 }, new[] { 1.25f });

            Assert.False(resolution.IsAvailable);
            Assert.Empty(resolution.Candidates);
        }

        [Fact]
        public void Resolve_excludes_only_the_current_ability_and_preserves_unrelated_duplicates()
        {
            var resolution = CandidateResolver.Resolve(2, 10, new[] { 10, 11, 11, 12 }, new[] { 1f, 2f, 3f, 4f });

            Assert.True(resolution.IsAvailable);
            Assert.Equal(
                new[] { new ReforgeCandidate(11, 2f), new ReforgeCandidate(11, 3f), new ReforgeCandidate(12, 4f) },
                resolution.Candidates);
        }

        [Fact]
        public void Resolve_returns_an_available_empty_result_when_every_candidate_is_current()
        {
            var resolution = CandidateResolver.Resolve(1, 10, new[] { 10 }, new[] { 1f });

            Assert.True(resolution.IsAvailable);
            Assert.Empty(resolution.Candidates);
        }
    }
}
