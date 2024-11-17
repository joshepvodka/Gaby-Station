using Content.Server.Administration.Logs;
using Content.Server.Popups;
using Content.Shared.Canvas;
using Robust.Shared.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Server.GameObjects;
using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Database;
using Content.Shared.Decals;
using Robust.Shared.Audio;
using System.Linq;
using System.Numerics;
using static Content.Shared.Canvas.SharedCanvasComponent;

namespace Content.Server.Canvas
{
    public sealed class CanvasSystem : SharedCanvasSystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CanvasComponent, ComponentInit>(OnCanvasInit);
            SubscribeLocalEvent<CanvasComponent, CanvasSelectMessage>(OnCanvasBoundUI);
            SubscribeLocalEvent<CanvasComponent, CanvasFinalizeMessage>(OnCanvasBoundFinalize);
            SubscribeLocalEvent<CanvasComponent, CanvasColorMessage>(OnCanvasBoundUIColor);
            SubscribeLocalEvent<CanvasComponent, UseInHandEvent>(OnCanvasUse, before: new[] { typeof(FoodSystem) });
            SubscribeLocalEvent<CanvasComponent, AfterInteractEvent>(OnCanvasAfterInteract, after: new[] { typeof(FoodSystem) });
            SubscribeLocalEvent<CanvasComponent, DroppedEvent>(OnCanvasDropped);
            SubscribeLocalEvent<CanvasComponent, ComponentGetState>(OnCanvasGetState);
        }

        private static void OnCanvasGetState(EntityUid uid, CanvasComponent component, ref ComponentGetState args)
        {
            args.State = new CanvasComponentState(component.Color, component.SelectedState, component.PaintingCode, component.Height, component.Width, component.Artist);
        }

        private void OnCanvasAfterInteract(EntityUid uid, CanvasComponent component, AfterInteractEvent args)
        {
            if (args.Handled || !args.CanReach)
                return;
            Dirty(uid, component);

            _uiSystem.ServerSendUiMessage(uid, SharedCanvasComponent.CanvasUiKey.Key, new CanvasUsedMessage(component.SelectedState));
        }

        private void OnCanvasUse(EntityUid uid, CanvasComponent component, UseInHandEvent args)
        {
            // Open Canvas window if neccessary.
            if (args.Handled)
                return;

            if (!_uiSystem.HasUi(uid, SharedCanvasComponent.CanvasUiKey.Key))
            {
                return;
            }

            _uiSystem.TryToggleUi(uid, SharedCanvasComponent.CanvasUiKey.Key, args.User);

            _uiSystem.SetUiState(uid, SharedCanvasComponent.CanvasUiKey.Key, new CanvasBoundUserInterfaceState(component.SelectedState, component.PaintingCode, component.Color, component.Height, component.Width, component.Artist));
            args.Handled = true;
        }

        private void OnCanvasBoundUI(EntityUid uid, CanvasComponent component, CanvasSelectMessage args)
        {
            // Check if the selected state is valid
            //if (!_prototypeManager.TryIndex<DecalPrototype>(args.State, out var prototype) || !prototype.Tags.Contains("Canvas"))
            //    return;

            component.SelectedState = args.State;
            component.PaintingCode = args.State;
            Dirty(uid, component);
        }

        private void OnCanvasBoundFinalize(EntityUid uid, CanvasComponent component, CanvasFinalizeMessage args)
        {
            Logger.ErrorS("canvas", $"Finalizado {args.State}.");
            component.Artist = args.State;
            Dirty(uid, component);
        }

        private void OnCanvasBoundUIColor(EntityUid uid, CanvasComponent component, CanvasColorMessage args)
        {
            // you still need to ensure that the given color is a valid color
            if (!component.SelectableColor || args.Color == component.Color)
                return;

            component.Color = args.Color;
            Dirty(uid, component);

        }

        private void OnCanvasInit(EntityUid uid, CanvasComponent component, ComponentInit args)
        {
            Dirty(uid, component);
        }

        private void OnCanvasDropped(EntityUid uid, CanvasComponent component, DroppedEvent args)
        {
            // TODO: Use the existing event.
            _uiSystem.CloseUi(uid, SharedCanvasComponent.CanvasUiKey.Key, args.User);
        }

        private void UseUpCanvas(EntityUid uid, EntityUid user)
        {
            _popup.PopupEntity(Loc.GetString("Canvas-interact-used-up-text", ("owner", uid)), user, user);
            EntityManager.QueueDeleteEntity(uid);
        }

        /// <summary>
        /// Generates a dynamic texture based on the CanvasComponent data.
        /// </summary>
        /// <param name="component">The canvas component with art data.</param>
        /// <returns>A generated texture representing the canvas art.</returns>
        //private Texture GenerateCanvasTexture(CanvasComponent component)
        //{
        //    // Canvas dimensions
        //    var width = component.Width;
        //    var height = component.Height;

        //    // Create a blank Image using ImageSharp
        //    using var image = new Image<Rgba32>(width, height);

        //    // Draw on the image using the PaintingCode
        //    for (var x = 0; x < width; x++)
        //    {
        //        for (var y = 0; y < height; y++)
        //        {
        //            // Get the color from the PaintingCode or default
        //            var color = GetColorFromCode(component.PaintingCode[x + y * width]);
        //            image[x, y] = new Rgba32(color.R, color.G, color.B, color.A);
        //        }
        //    }

        //    // Convert the image to a texture
        //    var texture = Texture.LoadFromImage(image, "DynamicCanvas");
        //    return texture;
        //}

        /// <summary>
        /// Converts a character from PaintingCode into a Color.
        /// </summary>
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
                'O' => new Color(1.0f, 0.65f, 0.0f), // Orange
                'P' => new Color(0.75f, 0.0f, 0.75f), // Purple
                'T' => new Color(0.33f, 0.55f, 0.2f), // Teal
                'L' => Color.LightGray,
                'D' => Color.DarkGray,
                'K' => Color.Black,
                _ => Color.White, // Default to white
            };
        }
    }
}
