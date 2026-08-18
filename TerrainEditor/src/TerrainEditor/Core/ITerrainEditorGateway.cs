namespace TerrainEditor.Core
{
    internal interface ITerrainEditorGateway
    {
        bool IsReady { get; }

        bool IsGameMenuOpen { get; }

        object SessionToken { get; }

        float TimeScale { get; set; }

        ITerrainEditorSession CaptureSession();
    }
}
