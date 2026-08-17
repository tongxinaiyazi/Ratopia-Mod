using System;

namespace ExecutionPlatform.Core
{
    internal enum ExecutionPhase
    {
        None,
        Seeking,
        Counting,
        Completed,
        Cancelled
    }

    [Flags]
    internal enum ExecutionAction
    {
        None = 0,
        Prepare = 1,
        RequestPath = 2,
        StartCounting = 4,
        Execute = 8,
        Cancel = 16
    }

    internal sealed class ExecutionStateMachine
    {
        internal const int SchedulingPriority = int.MaxValue;
        internal const float WorkDurationSeconds = 1f;
        internal const float PathRetrySeconds = 1f;

        private float _nextPathRequestAt;

        internal ExecutionPhase Phase { get; private set; }

        internal int BuildingId { get; private set; } = -1;

        internal float CountStartedAt { get; private set; }

        internal ExecutionAction Assign(int buildingId, float now)
        {
            BuildingId = buildingId;
            Phase = ExecutionPhase.Seeking;
            CountStartedAt = 0f;
            _nextPathRequestAt = now + PathRetrySeconds;
            return ExecutionAction.Prepare | ExecutionAction.RequestPath;
        }

        internal ExecutionAction Tick(float now, bool isValid, bool isAtWorkPosition)
        {
            if (Phase == ExecutionPhase.None ||
                Phase == ExecutionPhase.Completed ||
                Phase == ExecutionPhase.Cancelled)
            {
                return ExecutionAction.None;
            }

            if (!isValid)
            {
                Phase = ExecutionPhase.Cancelled;
                return ExecutionAction.Cancel;
            }

            if (Phase == ExecutionPhase.Seeking)
            {
                if (isAtWorkPosition)
                {
                    Phase = ExecutionPhase.Counting;
                    CountStartedAt = now;
                    return ExecutionAction.StartCounting;
                }

                if (now >= _nextPathRequestAt)
                {
                    _nextPathRequestAt = now + PathRetrySeconds;
                    return ExecutionAction.RequestPath;
                }

                return ExecutionAction.None;
            }

            if (!isAtWorkPosition)
            {
                Phase = ExecutionPhase.Seeking;
                CountStartedAt = 0f;
                _nextPathRequestAt = now + PathRetrySeconds;
                return ExecutionAction.RequestPath;
            }

            if (now >= CountStartedAt + WorkDurationSeconds)
            {
                Phase = ExecutionPhase.Completed;
                return ExecutionAction.Execute;
            }

            return ExecutionAction.None;
        }

        internal ExecutionAction Cancel()
        {
            if (Phase == ExecutionPhase.None ||
                Phase == ExecutionPhase.Completed ||
                Phase == ExecutionPhase.Cancelled)
            {
                return ExecutionAction.None;
            }

            Phase = ExecutionPhase.Cancelled;
            return ExecutionAction.Cancel;
        }
    }
}
