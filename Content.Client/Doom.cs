﻿using System;
using ManagedDoom;
using Robust.Client;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Content.Client
{
    public sealed class DoomCommand : IConsoleCommand
    {
        public string Command => "doom";
        public string Description => "666";
        public string Help => "666";

        public async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var clyde = IoCManager.Resolve<IClyde>();
            var uiMgr = IoCManager.Resolve<IUserInterfaceManager>();
            var window = await clyde.CreateWindow(new WindowCreateParameters {Width = 640, Height = 480});

            var root = uiMgr.CreateWindowRoot(window);

            window.DisposeOnClose = true;

            root.AddChild(new DoomControl((640, 400), () => window.Dispose()));
        }
    }

    internal sealed class DoomControl : Control
    {
        private readonly Action _onExit;
        private const int DoomFps = 35;
        private const double DoomSpf = 1f / DoomFps;

        [Dependency] private readonly IClyde _clyde = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IBaseClient _baseClient = default!;
        private readonly IRenderTexture _renderTarget;
        private readonly DoomApplication _application;
        private readonly Stopwatch _stopwatch;
        private double _lastTime;

        public DoomControl(Vector2i size, Action onExit)
        {
            _onExit = onExit;
            IoCManager.InjectDependencies(this);

            CanKeyboardFocus = true;
            KeyboardFocusOnClick = true;

            SetSize = size;
            MouseFilter = MouseFilterMode.Stop;

            _renderTarget = _clyde.CreateRenderTarget(size, RenderTargetColorFormat.Rgba8);

            _application = new DoomApplication(new CommandLineArgs(new []{"-warp", "E1M1"}), _renderTarget);
            _stopwatch = Stopwatch.StartNew();
        }

        protected override void ExitedTree()
        {
            base.ExitedTree();

            _application.Dispose();
        }

        public override void DrawInternal(IRenderHandle renderHandle)
        {
            var elapsed = _stopwatch.Elapsed.TotalSeconds;
            while (elapsed - _lastTime > DoomSpf)
            {
                _lastTime += DoomSpf;
                _application.DoEvents();
                var res = _application.UpdateAndRender(renderHandle);
                if (res == UpdateResult.Completed)
                {
                    _onExit();
                }
            }

            renderHandle.DrawingHandleScreen.DrawTexture(_renderTarget.Texture, Vector2.Zero);
        }

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            _application.KeyPressed(null, args);
        }

        protected override void KeyBindUp(GUIBoundKeyEventArgs args)
        {
            base.KeyBindUp(args);

            _application.KeyReleased(null, args);
        }

        protected override void KeyboardFocusEntered()
        {
            base.KeyboardFocusEntered();

            Console.WriteLine("ENTERED");

            _inputManager.Contexts.SetActiveContext("doom");
        }

        protected override void KeyboardFocusExited()
        {
            base.KeyboardFocusExited();

            Console.WriteLine("EXITED");

            if (_baseClient.RunLevel == ClientRunLevel.InGame || _baseClient.RunLevel == ClientRunLevel.InGame)
            {
                EntitySystem.Get<InputSystem>().SetEntityContextActive();
            }
            else
            {
                _inputManager.Contexts.SetActiveContext("common");
            }
        }
    }
}
