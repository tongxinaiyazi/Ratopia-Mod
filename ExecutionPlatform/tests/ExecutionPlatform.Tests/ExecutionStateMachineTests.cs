using System;
using ExecutionPlatform.Core;
using Xunit;

namespace ExecutionPlatform.Tests
{
    public sealed class ExecutionStateMachineTests
    {
        [Fact]
        public void AssignmentPreparesAndImmediatelyRequestsAPathAtTopPriority()
        {
            var machine = new ExecutionStateMachine();

            var action = machine.Assign(buildingId: 42, now: 10f);

            Assert.Equal(ExecutionPhase.Seeking, machine.Phase);
            Assert.Equal(42, machine.BuildingId);
            Assert.Equal(int.MaxValue, ExecutionStateMachine.SchedulingPriority);
            Assert.True(action.HasFlag(ExecutionAction.Prepare));
            Assert.True(action.HasFlag(ExecutionAction.RequestPath));
        }

        [Fact]
        public void SeekingRetriesPathfindingOncePerGameSecond()
        {
            var machine = AssignedAt(4f);

            Assert.Equal(ExecutionAction.None, machine.Tick(4.99f, isValid: true, isAtWorkPosition: false));
            Assert.Equal(ExecutionAction.RequestPath, machine.Tick(5f, isValid: true, isAtWorkPosition: false));
            Assert.Equal(ExecutionAction.None, machine.Tick(5.99f, isValid: true, isAtWorkPosition: false));
            Assert.Equal(ExecutionAction.RequestPath, machine.Tick(6f, isValid: true, isAtWorkPosition: false));
        }

        [Fact]
        public void ArrivalStartsANewOneSecondCountdown()
        {
            var machine = AssignedAt(0f);

            var action = machine.Tick(0.25f, isValid: true, isAtWorkPosition: true);

            Assert.Equal(ExecutionAction.StartCounting, action);
            Assert.Equal(ExecutionPhase.Counting, machine.Phase);
            Assert.Equal(0.25f, machine.CountStartedAt);
        }

        [Fact]
        public void CountdownExecutesAtTheOneSecondBoundary()
        {
            var machine = AssignedAt(0f);
            machine.Tick(0.25f, isValid: true, isAtWorkPosition: true);

            Assert.Equal(ExecutionAction.None, machine.Tick(1.249f, isValid: true, isAtWorkPosition: true));
            Assert.Equal(ExecutionAction.Execute, machine.Tick(1.25f, isValid: true, isAtWorkPosition: true));
            Assert.Equal(ExecutionPhase.Completed, machine.Phase);
        }

        [Fact]
        public void RepeatedTicksAtTheSameScaledTimeActLikePause()
        {
            var machine = AssignedAt(0f);
            machine.Tick(3f, isValid: true, isAtWorkPosition: true);

            for (var index = 0; index < 100; index++)
            {
                Assert.Equal(ExecutionAction.None, machine.Tick(3f, isValid: true, isAtWorkPosition: true));
            }
        }

        [Fact]
        public void InvalidConditionsCancelWithoutExecution()
        {
            var machine = AssignedAt(0f);
            machine.Tick(0.1f, isValid: true, isAtWorkPosition: true);

            var action = machine.Tick(0.9f, isValid: false, isAtWorkPosition: true);

            Assert.Equal(ExecutionAction.Cancel, action);
            Assert.Equal(ExecutionPhase.Cancelled, machine.Phase);
            Assert.False(action.HasFlag(ExecutionAction.Execute));
        }

        [Fact]
        public void LeavingTheWorkPositionRestartsSeekingAndTheFullCountdown()
        {
            var machine = AssignedAt(0f);
            machine.Tick(0.1f, isValid: true, isAtWorkPosition: true);

            Assert.Equal(ExecutionAction.RequestPath, machine.Tick(0.8f, isValid: true, isAtWorkPosition: false));
            Assert.Equal(ExecutionPhase.Seeking, machine.Phase);
            Assert.Equal(ExecutionAction.StartCounting, machine.Tick(2f, isValid: true, isAtWorkPosition: true));
            Assert.Equal(ExecutionAction.None, machine.Tick(2.999f, isValid: true, isAtWorkPosition: true));
            Assert.Equal(ExecutionAction.Execute, machine.Tick(3f, isValid: true, isAtWorkPosition: true));
        }

        [Fact]
        public void RebuildingAfterLoadStartsFromSeekingAndDoesNotPersistCountdown()
        {
            var beforeLoad = AssignedAt(0f);
            beforeLoad.Tick(0.2f, isValid: true, isAtWorkPosition: true);
            beforeLoad.Tick(0.9f, isValid: true, isAtWorkPosition: true);

            var afterLoad = new ExecutionStateMachine();
            afterLoad.Assign(buildingId: 42, now: 20f);
            afterLoad.Tick(20f, isValid: true, isAtWorkPosition: true);

            Assert.Equal(ExecutionAction.None, afterLoad.Tick(20.999f, isValid: true, isAtWorkPosition: true));
            Assert.Equal(ExecutionAction.Execute, afterLoad.Tick(21f, isValid: true, isAtWorkPosition: true));
        }

        [Fact]
        public void CompletionCanOnlyBeEmittedOnce()
        {
            var machine = AssignedAt(0f);
            machine.Tick(0f, isValid: true, isAtWorkPosition: true);

            Assert.Equal(ExecutionAction.Execute, machine.Tick(1f, isValid: true, isAtWorkPosition: true));
            Assert.Equal(ExecutionAction.None, machine.Tick(2f, isValid: true, isAtWorkPosition: true));
        }

        private static ExecutionStateMachine AssignedAt(float now)
        {
            var machine = new ExecutionStateMachine();
            machine.Assign(buildingId: 42, now: now);
            return machine;
        }
    }
}
