namespace WireThroughWalls.Core
{
    internal enum SessionAction
    {
        None,
        Reset,
        Initialize
    }

    internal sealed class SessionTracker<TManager> where TManager : class
    {
        private TManager _manager;
        private bool _initialized;

        internal SessionAction Observe(TManager manager, bool isLoading)
        {
            if (manager == null)
            {
                if (_manager == null && !_initialized)
                {
                    return SessionAction.None;
                }

                _manager = null;
                _initialized = false;
                return SessionAction.Reset;
            }

            if (!ReferenceEquals(manager, _manager))
            {
                var replacedExistingSession = _manager != null || _initialized;
                _manager = manager;
                _initialized = false;
                if (replacedExistingSession)
                {
                    return SessionAction.Reset;
                }
            }

            if (isLoading || _initialized)
            {
                return SessionAction.None;
            }

            return SessionAction.Initialize;
        }

        internal void MarkInitialized()
        {
            if (_manager != null)
            {
                _initialized = true;
            }
        }

        internal void MarkInitializationFailed()
        {
            _initialized = false;
        }
    }
}
