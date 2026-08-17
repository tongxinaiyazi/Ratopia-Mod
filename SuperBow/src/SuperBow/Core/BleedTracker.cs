using System;
using System.Collections.Generic;

namespace SuperBow.Core
{
    public sealed class BleedTick<TTarget>
    {
        public BleedTick(TTarget target, float fraction)
        {
            Target = target;
            Fraction = fraction;
        }

        public TTarget Target { get; }

        public float Fraction { get; }
    }

    public sealed class BleedTracker<TTarget>
    {
        private const float TimeTolerance = 0.0001f;
        private readonly Dictionary<TTarget, BleedState> _states =
            new Dictionary<TTarget, BleedState>();

        public int Count => _states.Count;

        public void ApplyOrRefresh(TTarget target, float now)
        {
            if (ReferenceEquals(target, null))
            {
                return;
            }

            if (_states.TryGetValue(target, out var state) &&
                now <= state.ExpiresAt + TimeTolerance)
            {
                state.ExpiresAt = now + SuperBowConstants.BleedDuration;
                return;
            }

            _states[target] = new BleedState(
                now + SuperBowConstants.BleedTickInterval,
                now + SuperBowConstants.BleedDuration);
        }

        public IReadOnlyList<BleedTick<TTarget>> Advance(
            float now,
            Func<TTarget, bool> isValid,
            Func<TTarget, bool> isBoss)
        {
            if (isValid == null)
            {
                throw new ArgumentNullException(nameof(isValid));
            }

            if (isBoss == null)
            {
                throw new ArgumentNullException(nameof(isBoss));
            }

            var ticks = new List<BleedTick<TTarget>>();
            var targets = new List<TTarget>(_states.Keys);
            foreach (var target in targets)
            {
                if (!_states.TryGetValue(target, out var state))
                {
                    continue;
                }

                if (!isValid(target))
                {
                    _states.Remove(target);
                    continue;
                }

                while (state.NextTick <= now + TimeTolerance &&
                       state.NextTick <= state.ExpiresAt + TimeTolerance)
                {
                    ticks.Add(new BleedTick<TTarget>(
                        target,
                        isBoss(target)
                            ? SuperBowConstants.BossBleedFraction
                            : SuperBowConstants.NormalBleedFraction));
                    state.NextTick += SuperBowConstants.BleedTickInterval;
                }

                if (now >= state.ExpiresAt - TimeTolerance &&
                    state.NextTick > state.ExpiresAt + TimeTolerance)
                {
                    _states.Remove(target);
                }
            }

            return ticks;
        }

        public void Clear()
        {
            _states.Clear();
        }

        private sealed class BleedState
        {
            public BleedState(float nextTick, float expiresAt)
            {
                NextTick = nextTick;
                ExpiresAt = expiresAt;
            }

            public float NextTick { get; set; }

            public float ExpiresAt { get; set; }
        }
    }
}
