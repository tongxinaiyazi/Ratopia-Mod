using EquipmentReforgeSelector;
using Xunit;

namespace EquipmentReforgeSelector.Tests
{
    public sealed class CandidateNavigationPlanTests
    {
        [Fact]
        public void Rows_have_explicit_vertical_neighbors_without_an_initial_focus_target()
        {
            var plan = CandidateNavigationPlan.Create(3);

            Assert.Null(plan.InitialFocusIndex);
            Assert.Null(plan.Rows[0].UpIndex);
            Assert.Equal(1, plan.Rows[0].DownIndex);
            Assert.Equal(0, plan.Rows[1].UpIndex);
            Assert.Equal(2, plan.Rows[1].DownIndex);
            Assert.Equal(1, plan.Rows[2].UpIndex);
            Assert.Null(plan.Rows[2].DownIndex);
        }
    }
}
