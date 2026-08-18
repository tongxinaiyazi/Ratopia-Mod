using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class CandidateShortcutTests
    {
        [Theory]
        [InlineData(1, 3, 0)]
        [InlineData(2, 3, 1)]
        [InlineData(3, 3, 2)]
        [InlineData(9, 9, 8)]
        public void Visible_digit_maps_to_zero_based_candidate(int digit, int count, int expected)
        {
            Assert.True(CandidateShortcut.TryResolveDigit(digit, count, out var actual));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1, 3)]
        [InlineData(0, 3)]
        [InlineData(4, 3)]
        [InlineData(10, 10)]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        public void Out_of_range_digit_is_ignored(int digit, int count)
        {
            Assert.False(CandidateShortcut.TryResolveDigit(digit, count, out _));
        }
    }
}
