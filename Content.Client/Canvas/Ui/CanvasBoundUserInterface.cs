using Content.Client.UserInterface;
using Content.Shared.Ame.Components;
using Content.Shared.Canvas;
using Content.Shared.Medical.CrewMonitoring;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using static Content.Shared.Canvas.SharedCanvasComponent;
using static Robust.Client.UserInterface.Controls.MenuBar;
using System.ComponentModel;
using Color = Robust.Shared.Maths.Color;

namespace Content.Client.Canvas.Ui
{
    public sealed class CanvasBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IPrototypeManager _protoManager = default!;

        [ViewVariables]
        private CanvasWindow? _window;

        public CanvasBoundUserInterface(EntityUid owner, object uiKey) : base(owner, (Enum)uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<CanvasWindow>();
            _window.OnColorSelected += SelectColor;
            _window.OnSelected += Select;
            _window.OnFinalize += Finalize;
            _window.OnClose += Close;
            PopulateCanvas(Owner);
            _window.OpenCentered();
        }

        private void PopulateCanvas(EntityUid uid)
        {
            // Example: A set of predefined colors
            var colors = new List<Color>
            {
                Color.Red,
                Color.Blue,
                Color.Green,
                Color.Yellow,
                Color.Cyan,
                Color.Magenta,
                new Color(1.0f, 0.65f, 0.0f), // Orange
                new Color(0.75f, 0.0f, 0.75f), // Purple
                new Color(0.33f, 0.55f, 0.2f), // Teal
                Color.LightGray,
                Color.DarkGray,
                Color.Black,
                Color.White // Ensure white is last for consistency
            };

            EntMan.TryGetComponent<CanvasComponent>(Owner, out var canvasComponent);
            var paintingCode = canvasComponent?.PaintingCode;
            if (paintingCode != null)
                _window?.SetPaintingCode(paintingCode);
            var artist = canvasComponent?.Artist;
            if (!string.IsNullOrEmpty(artist))
            {
                _window?.SetArtist(artist);
            }
            _window?.PopulateColorSelector(colors);
            _window?.PopulatePaintingGrid();
        }


        public override void OnProtoReload(PrototypesReloadedEventArgs args)
        {
            base.OnProtoReload(args);

            //if (!args.WasModified<DecalPrototype>())
            //    return;

            //PopulateCanvas();
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            base.ReceiveMessage(message);

            if (_window is null || message is not CanvasUsedMessage canvasMessage)
                return;

            _window.AdvanceState(canvasMessage.DrawnDecal);
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);


            var castState = (CanvasBoundUserInterfaceState)state;

            _window?.UpdateState(castState);
        }

        public void Select(string state)
        {
            SendMessage(new CanvasSelectMessage(state));
        }

        public void Finalize(string state)
        {
            SendMessage(new CanvasFinalizeMessage(state));
        }

        public void SelectColor(Color color)
        {
            SendMessage(new CanvasColorMessage(color));
        }
    }
}
