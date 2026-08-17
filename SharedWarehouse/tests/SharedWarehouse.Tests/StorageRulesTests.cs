using System;
using SharedWarehouse.Core;
using Xunit;

namespace SharedWarehouse.Tests
{
    public sealed class StorageRulesTests
    {
        [Theory]
        [InlineData(100, true)]
        [InlineData(181, true)]
        [InlineData(360, false)]
        [InlineData(99, false)]
        public void IsTargetBuilding_accepts_only_normal_and_mini_storage(int buildingName, bool expected)
        {
            Assert.Equal(expected, StorageRules.IsTargetBuilding(buildingName));
        }

        [Fact]
        public void FormatCapacity_uses_infinity_symbol()
        {
            Assert.Equal("7/∞", StorageRules.FormatCapacity(7));
        }

        [Fact]
        public void CapacityOverride_restores_original_value_after_repeated_apply()
        {
            var info = new FakeInfo { Capacity = 5f };
            var registry = Registry();

            registry.Apply(info);
            registry.Apply(info);
            Assert.True(float.IsPositiveInfinity(info.Capacity));

            registry.RestoreAll();
            Assert.Equal(5f, info.Capacity);
        }

        [Fact]
        public void CapacityOverride_does_not_overwrite_a_later_external_change()
        {
            var info = new FakeInfo { Capacity = 5f };
            var registry = Registry();
            registry.Apply(info);
            info.Capacity = 123f;

            registry.RestoreAll();

            Assert.Equal(123f, info.Capacity);
        }

        private static CapacityOverrideRegistry<FakeInfo> Registry()
        {
            return new CapacityOverrideRegistry<FakeInfo>(
                info => info.Capacity,
                (info, value) => info.Capacity = value,
                float.PositiveInfinity,
                value => float.IsPositiveInfinity(value));
        }

        private sealed class FakeInfo
        {
            public float Capacity { get; set; }
        }
    }
}
