namespace TerrainEditor.Core
{
    internal readonly struct EditorInput
    {
        public EditorInput(bool togglePressed, bool escapePressed)
        {
            TogglePressed = togglePressed;
            EscapePressed = escapePressed;
        }

        public bool TogglePressed { get; }

        public bool EscapePressed { get; }
    }
}
