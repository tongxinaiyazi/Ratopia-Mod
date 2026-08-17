using System;

namespace GodViewManagement
{
    internal sealed class RemotePanelState
    {
        private object _panel;
        private object _target;

        public bool IsOpen => _panel != null && _target != null;

        public object Target => _target;

        public void Open(object panel, object target)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public bool ShouldBlockQueenAction(object panel, object target)
        {
            return IsOpen && ReferenceEquals(_panel, panel) && ReferenceEquals(_target, target);
        }

        public void Clear()
        {
            _panel = null;
            _target = null;
        }
    }
}
