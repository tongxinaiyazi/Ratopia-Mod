using System;

namespace SuperBow.Runtime
{
    internal static class DamageDisplayRuntime
    {
        [ThreadStatic]
        private static int? _overrideDamage;

        public static IDisposable Override(int displayDamage)
        {
            var previous = _overrideDamage;
            _overrideDamage = displayDamage;
            return new OverrideScope(previous);
        }

        public static bool TryGetOverride(out int displayDamage)
        {
            if (_overrideDamage.HasValue)
            {
                displayDamage = _overrideDamage.Value;
                return true;
            }

            displayDamage = 0;
            return false;
        }

        private sealed class OverrideScope : IDisposable
        {
            private readonly int? _previous;
            private bool _disposed;

            public OverrideScope(int? previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _overrideDamage = _previous;
                _disposed = true;
            }
        }
    }
}
