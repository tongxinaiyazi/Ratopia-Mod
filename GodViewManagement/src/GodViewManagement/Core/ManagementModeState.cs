using System;

namespace GodViewManagement
{
    internal sealed class ManagementModeState
    {
        private object _session;
        private bool _hasObservedSession;

        public bool IsEnabled { get; private set; }

        public bool Toggle()
        {
            IsEnabled = !IsEnabled;
            return IsEnabled;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public bool ObserveSession(object session)
        {
            if (_hasObservedSession && ReferenceEquals(_session, session))
            {
                return false;
            }

            _hasObservedSession = true;
            _session = session;
            Disable();
            return true;
        }

        public void Reset()
        {
            _hasObservedSession = false;
            _session = null;
            Disable();
        }
    }
}
