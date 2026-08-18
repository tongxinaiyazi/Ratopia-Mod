using TerrainEditor.Core;
using System;
using Xunit;

namespace TerrainEditor.Tests
{
    public sealed class TerrainEditorControllerTests
    {
        [Fact]
        public void F4EntersEditorWhenEnvironmentIsReady()
        {
            var gateway = new FakeTerrainEditorGateway
            {
                Ready = true,
                Zoom = 8.5f,
                TimeScale = 1.5f,
                SandboxMode = false
            };
            var controller = new TerrainEditorController(gateway);

            var transition = controller.Tick(new EditorInput(togglePressed: true, escapePressed: false));

            Assert.Equal(EditorTransition.Entered, transition);
            Assert.True(controller.IsEnabled);
            Assert.True(gateway.SandboxMode);
            Assert.Equal(20f, gateway.Zoom);
            Assert.Equal(0.3f, gateway.TimeScale);
            Assert.True(gateway.PaletteVisible);
        }

        [Fact]
        public void F4ExitsAndRestoresTheStateCapturedOnEntry()
        {
            var gateway = ReadyGateway();
            var controller = new TerrainEditorController(gateway);
            controller.Tick(ToggleInput());

            var transition = controller.Tick(ToggleInput());

            Assert.Equal(EditorTransition.Exited, transition);
            Assert.False(controller.IsEnabled);
            Assert.False(gateway.PaletteVisible);
            Assert.False(gateway.SandboxMode);
            Assert.Equal(8.5f, gateway.Zoom);
            Assert.Equal(1.5f, gateway.TimeScale);
            Assert.Equal(1, gateway.ResetPaletteCalls);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void F4DoesNotEnterWhenEnvironmentIsUnavailableOrMenuIsOpen(bool ready, bool menuOpen)
        {
            var gateway = ReadyGateway();
            gateway.Ready = ready;
            gateway.MenuOpen = menuOpen;
            var controller = new TerrainEditorController(gateway);

            var transition = controller.Tick(ToggleInput());

            Assert.Equal(EditorTransition.None, transition);
            Assert.False(controller.IsEnabled);
            Assert.False(gateway.PaletteVisible);
        }

        [Fact]
        public void EscapeExitsOnlyWhenEditorIsEnabled()
        {
            var gateway = ReadyGateway();
            var controller = new TerrainEditorController(gateway);

            Assert.Equal(EditorTransition.None, controller.Tick(EscapeInput()));
            controller.Tick(ToggleInput());

            Assert.Equal(EditorTransition.Exited, controller.Tick(EscapeInput()));
            Assert.False(controller.IsEnabled);
        }

        [Fact]
        public void SessionChangeClosesEditorAndRestoresCapturedState()
        {
            var gateway = ReadyGateway();
            var controller = new TerrainEditorController(gateway);
            controller.Tick(ToggleInput());

            var oldSession = gateway.ActiveSession;
            var newSession = new FakeTerrainEditorSession
            {
                Zoom = 12f,
                SandboxMode = true,
                PaletteVisible = false
            };
            gateway.ActiveSession = newSession;
            gateway.TimeScale = 2f;

            var transition = controller.Tick(default);

            Assert.Equal(EditorTransition.Exited, transition);
            Assert.False(oldSession.PaletteVisible);
            Assert.False(oldSession.SandboxMode);
            Assert.Equal(8.5f, oldSession.Zoom);
            Assert.False(newSession.PaletteVisible);
            Assert.True(newSession.SandboxMode);
            Assert.Equal(12f, newSession.Zoom);
            Assert.Equal(1.5f, gateway.TimeScale);
        }

        [Fact]
        public void RepeatedExitIsIdempotent()
        {
            var gateway = ReadyGateway();
            var controller = new TerrainEditorController(gateway);
            controller.Tick(ToggleInput());

            Assert.Equal(EditorTransition.Exited, controller.Exit());
            Assert.Equal(EditorTransition.None, controller.Exit());
            Assert.Equal(1, gateway.ResetPaletteCalls);
        }

        [Fact]
        public void FailedEntryRollsBackEveryStateAlreadyChanged()
        {
            var gateway = ReadyGateway();
            gateway.ThrowWhenShowingPalette = true;
            var controller = new TerrainEditorController(gateway);

            Assert.Throws<InvalidOperationException>(() => controller.Tick(ToggleInput()));
            Assert.False(controller.IsEnabled);
            Assert.False(gateway.PaletteVisible);
            Assert.False(gateway.SandboxMode);
            Assert.Equal(8.5f, gateway.Zoom);
            Assert.Equal(1.5f, gateway.TimeScale);
        }

        [Fact]
        public void ExitContinuesRestoringStateWhenPaletteResetFails()
        {
            var gateway = ReadyGateway();
            var controller = new TerrainEditorController(gateway);
            controller.Tick(ToggleInput());
            gateway.ThrowWhenResettingPalette = true;

            Assert.Throws<InvalidOperationException>(() => controller.Exit());
            Assert.False(controller.IsEnabled);
            Assert.False(gateway.PaletteVisible);
            Assert.False(gateway.SandboxMode);
            Assert.Equal(8.5f, gateway.Zoom);
            Assert.Equal(1.5f, gateway.TimeScale);
        }

        private static FakeTerrainEditorGateway ReadyGateway()
        {
            return new FakeTerrainEditorGateway
            {
                Ready = true,
                ActiveSession = new FakeTerrainEditorSession
                {
                    Zoom = 8.5f,
                    SandboxMode = false
                },
                TimeScale = 1.5f
            };
        }

        private static EditorInput ToggleInput()
        {
            return new EditorInput(togglePressed: true, escapePressed: false);
        }

        private static EditorInput EscapeInput()
        {
            return new EditorInput(togglePressed: false, escapePressed: true);
        }
    }

    internal sealed class FakeTerrainEditorGateway : ITerrainEditorGateway
    {
        public bool Ready { get; set; }
        public bool MenuOpen { get; set; }
        public FakeTerrainEditorSession ActiveSession { get; set; } = new FakeTerrainEditorSession();

        public bool IsReady => Ready;
        public bool IsGameMenuOpen => MenuOpen;
        public object SessionToken => ActiveSession.Token;
        public bool SandboxMode { get => ActiveSession.SandboxMode; set => ActiveSession.SandboxMode = value; }
        public float Zoom { get => ActiveSession.Zoom; set => ActiveSession.Zoom = value; }
        public float TimeScale { get; set; }
        public bool PaletteVisible { get => ActiveSession.PaletteVisible; set => ActiveSession.PaletteVisible = value; }
        public bool ThrowWhenShowingPalette { get => ActiveSession.ThrowWhenShowingPalette; set => ActiveSession.ThrowWhenShowingPalette = value; }
        public bool ThrowWhenResettingPalette { get => ActiveSession.ThrowWhenResettingPalette; set => ActiveSession.ThrowWhenResettingPalette = value; }
        public int ResetPaletteCalls => ActiveSession.ResetPaletteCalls;

        public ITerrainEditorSession CaptureSession()
        {
            return ActiveSession;
        }

        public void ResetPaletteSelection()
        {
            ActiveSession.ResetPaletteSelection();
        }
    }

    internal sealed class FakeTerrainEditorSession : ITerrainEditorSession
    {
        private bool _paletteVisible;

        public object Token { get; } = new object();
        public bool SandboxMode { get; set; }
        public float Zoom { get; set; }
        public bool ThrowWhenShowingPalette { get; set; }
        public bool ThrowWhenResettingPalette { get; set; }
        public int ResetPaletteCalls { get; private set; }

        public bool PaletteVisible
        {
            get => _paletteVisible;
            set
            {
                if (value && ThrowWhenShowingPalette)
                {
                    throw new InvalidOperationException("palette show failed");
                }

                _paletteVisible = value;
            }
        }

        public void ResetPaletteSelection()
        {
            ResetPaletteCalls++;
            if (ThrowWhenResettingPalette)
            {
                throw new InvalidOperationException("palette reset failed");
            }
        }
    }
}
