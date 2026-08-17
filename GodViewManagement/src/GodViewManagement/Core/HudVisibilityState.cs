namespace GodViewManagement
{
    internal sealed class HudVisibilityState
    {
        public bool IsHidden { get; private set; }

        public void Hide()
        {
            IsHidden = true;
        }

        public void Show()
        {
            IsHidden = false;
        }

        public void Reset()
        {
            Show();
        }

        public bool TryToggle(bool shiftPressed, bool togglePressed)
        {
            if (!shiftPressed || !togglePressed)
            {
                return false;
            }

            IsHidden = !IsHidden;
            return true;
        }
    }
}
