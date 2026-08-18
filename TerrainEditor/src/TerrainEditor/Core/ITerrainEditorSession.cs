namespace TerrainEditor.Core
{
    internal interface ITerrainEditorSession
    {
        object Token { get; }

        bool SandboxMode { get; set; }

        float Zoom { get; set; }

        bool PaletteVisible { get; set; }

        void ResetPaletteSelection();
    }
}
