using Content.Client.Items;
using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared.Canvas;
using Mono.Cecil.Cil;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Localization;
using Robust.Shared.Timing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using static Content.Shared.Canvas.SharedCanvasComponent;
using Color = Robust.Shared.Maths.Color;
using Robust.Client.Graphics;

namespace Content.Client.Canvas
{
    public sealed class CanvasSystem : SharedCanvasSystem
    {

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CanvasComponent, ComponentHandleState>(OnCanvasHandleState);
            Subs.ItemStatus<CanvasComponent>(ent => new StatusControl(ent));
        }

        private void OnCanvasHandleState(EntityUid uid, CanvasComponent component, ref ComponentHandleState args)
        {
            if (args.Current is not CanvasComponentState state) return;

            component.Color = state.Color;
            component.SelectedState = state.State;
            component.PaintingCode = state.PaintingCode;
            component.Height = state.Height;
            component.Width = state.Width;
            component.Artist = state.Artist;

            component.UIUpdateNeeded = true;

            if (!string.IsNullOrEmpty(component.Artist))
            {
                UpdateSprite(uid, component.PaintingCode, component.Height, component.Width);
            }
        }

        public void UpdateSprite(EntityUid uid, string code,int height = 16, int width = 16)
        {
            Logger.Info($"gerando arte system.");
            if (string.IsNullOrEmpty(code))
                return;
            // Update the sprite or visuals based on the artist
            if (EntityManager.TryGetComponent<SpriteComponent>(uid, out var sprite))
            {
                // Change sprite texture based on artist name
                var texture = GenerateArtistTexture(code, height, width); // Implement this method
                sprite.LayerSetTexture(0, texture); // Assuming layer 0; adjust as needed
            }
        }

        private Texture GenerateArtistTexture(string code, int height = 16, int width = 16)
        {
            const int sizeMultiplier = 2; // size in pixels
            var image = new Image<Rgba32>(width * sizeMultiplier, height * sizeMultiplier);

            // Nested loops for rows and columns
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    // Calculate the index in the code array
                    int index = row * width + col;

                    // Default color is white if index is out of bounds
                    var color = index < code.Length
                        ? GetColorFromCode(code[index])
                        : Color.White;

                    // Fill the corresponding area in the image
                    for (int x = col * sizeMultiplier; x < (col + 1) * sizeMultiplier; x++)
                    {
                        for (int y = row * sizeMultiplier; y < (row + 1) * sizeMultiplier; y++)
                        {
                            image[x, y] = new Rgba32(color.R, color.G, color.B, color.A);
                        }
                    }
                }
            }

            // Convert the image to a texture
            return Texture.LoadFromImage(image, "DynamicCanvas");
        }

        private Color GetColorFromCode(char code)
        {
            return code switch
            {
                'R' => Color.Red,
                'G' => Color.Green,
                'B' => Color.Blue,
                'Y' => Color.Yellow,
                'C' => Color.Cyan,
                'M' => Color.Magenta,
                'O' => new Color(1.0f, 0.65f, 0.0f),
                'P' => new Color(0.75f, 0.0f, 0.75f),
                'T' => new Color(0.33f, 0.55f, 0.2f),
                'L' => Color.LightGray,
                'D' => Color.DarkGray,
                'K' => Color.Black,
                _ => Color.White,
            };
        }

        private sealed class StatusControl : Control
        {
            private readonly CanvasComponent _parent;
            private readonly RichTextLabel _label;

            public StatusControl(CanvasComponent parent)
            {
                _parent = parent;
                _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
                AddChild(_label);

                parent.UIUpdateNeeded = true;
            }

            protected override void FrameUpdate(FrameEventArgs args)
            {
                base.FrameUpdate(args);

                if (!_parent.UIUpdateNeeded)
                {
                    return;
                }

                _parent.UIUpdateNeeded = false;
                _label.SetMarkup(Robust.Shared.Localization.Loc.GetString("Canvas-drawing-label",
                    ("color", _parent.Color),
                    ("state", _parent.SelectedState),
                    ("paintingcode", _parent.PaintingCode),
                    ("height", _parent.Height),
                    ("width", _parent.Width),
                    ("artist", _parent.Artist)));
            }
        }
    }
}
