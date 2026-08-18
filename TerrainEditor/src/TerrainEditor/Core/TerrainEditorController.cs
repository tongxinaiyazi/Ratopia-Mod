using System;

namespace TerrainEditor.Core
{
    internal sealed class TerrainEditorController
    {
        private readonly ITerrainEditorGateway _gateway;
        private EditorSnapshot _snapshot;

        public TerrainEditorController(ITerrainEditorGateway gateway)
        {
            _gateway = gateway;
        }

        public bool IsEnabled { get; private set; }

        public EditorTransition Tick(EditorInput input)
        {
            if (IsEnabled)
            {
                if (!ReferenceEquals(_snapshot.SessionToken, _gateway.SessionToken)
                    || !_gateway.IsReady
                    || input.TogglePressed
                    || input.EscapePressed)
                {
                    return Exit();
                }

                return EditorTransition.None;
            }

            if (!input.TogglePressed || !_gateway.IsReady || _gateway.IsGameMenuOpen)
            {
                return EditorTransition.None;
            }

            var session = _gateway.CaptureSession();
            _snapshot = new EditorSnapshot(
                session,
                session.SandboxMode,
                session.Zoom,
                _gateway.TimeScale);
            IsEnabled = true;

            try
            {
                session.SandboxMode = true;
                session.Zoom = 20f;
                _gateway.TimeScale = 0.3f;
                session.PaletteVisible = true;
                return EditorTransition.Entered;
            }
            catch
            {
                try
                {
                    Exit();
                }
                catch
                {
                    // Preserve the entry failure after making a best-effort rollback.
                }

                throw;
            }
        }

        public EditorTransition Exit()
        {
            if (!IsEnabled || _snapshot == null)
            {
                return EditorTransition.None;
            }

            Exception firstError = null;
            var session = _snapshot.Session;
            TryCleanup(session.ResetPaletteSelection, ref firstError);
            TryCleanup(() => session.PaletteVisible = false, ref firstError);
            TryCleanup(() => session.SandboxMode = _snapshot.SandboxMode, ref firstError);
            TryCleanup(() => session.Zoom = _snapshot.Zoom, ref firstError);
            TryCleanup(() => _gateway.TimeScale = _snapshot.TimeScale, ref firstError);

            IsEnabled = false;
            _snapshot = null;

            if (firstError != null)
            {
                throw firstError;
            }

            return EditorTransition.Exited;
        }

        private static void TryCleanup(Action action, ref Exception firstError)
        {
            try
            {
                action();
            }
            catch (Exception error)
            {
                if (firstError == null)
                {
                    firstError = error;
                }
            }
        }

        private sealed class EditorSnapshot
        {
            public EditorSnapshot(ITerrainEditorSession session, bool sandboxMode, float zoom, float timeScale)
            {
                Session = session;
                SandboxMode = sandboxMode;
                Zoom = zoom;
                TimeScale = timeScale;
            }

            public ITerrainEditorSession Session { get; }

            public object SessionToken => Session.Token;

            public bool SandboxMode { get; }

            public float Zoom { get; }

            public float TimeScale { get; }
        }
    }
}
