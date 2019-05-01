using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UIManager : IDisposable
    {
        public delegate void TransformListSortedDelegate();

        public event TransformListSortedDelegate TransformListSorted;

        //private static TextureRegion2D _grayscaleRegion;

        internal bool IsDisposing { get; private set; }
        public bool IsDisposed { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }
        public TextureRegion2D GrayscaleRegion { get; set; }
        public TextureRegion2D WhitePixelRegion { get; set; }
        public SamplingMode PreferredSampling { get; set; }
        public Viewport Viewport => GraphicsDevice.Viewport;

        public bool TransformsNeedSorting { get; private set; }
        public ListArray<UITransform> Transforms { get; }

        /// <summary>
        /// The currently selected <see cref="UIElement"/> (<see cref="UIElement.IsSelectable"/>
        /// needs to be <see langword="true"/>) that receives keyboard and events.
        /// </summary>
        public UIElement SelectedElement { get; set; }

        /// <summary>
        /// Constructs a <see cref="UIManager"/> instance using an existing grayscale region.
        /// </summary>
        /// <param name="grayscaleRegion">
        /// A <see cref="TextureRegion2D"/> containing at least 1x1 white pixel
        /// at (X:0,Y:0) and a 1x1 gray pixel at (X:0,Y:1) 
        /// (this can be larger as linear filtering can stretch the texture).
        /// This texture is mostly used for better shading on elements.
        /// </param>
        public UIManager(GraphicsDevice device, TextureRegion2D grayscaleRegion, TextureRegion2D whitePixelRegion)
        {
            GrayscaleRegion = grayscaleRegion ?? throw new ArgumentNullException(nameof(grayscaleRegion));
            if (GrayscaleRegion.Width < 1) throw new ArgumentException(nameof(grayscaleRegion));
            if (GrayscaleRegion.Height < 2) throw new ArgumentException(nameof(grayscaleRegion));

            WhitePixelRegion = whitePixelRegion ?? throw new ArgumentNullException(nameof(whitePixelRegion));
            if (WhitePixelRegion.Width < 1) throw new ArgumentException(nameof(whitePixelRegion));
            if (WhitePixelRegion.Height < 1) throw new ArgumentException(nameof(whitePixelRegion));

            GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));

            Transforms = new ListArray<UITransform>();
            Transforms.Changed += Transforms_Changed;
            PreferredSampling = SamplingMode.LinearClamp;

            Input.TextInput += Input_TextInput;
        }

        private void Transforms_Changed(int oldVersion, int newVersion)
        {
            FlagForSort();
        }

        private void Input_TextInput(TextInputEvent data)
        {
            if (!IsDisposed)
            {
                if (SelectedElement != null && SelectedElement.IsKeyboardEventTrigger)
                    SelectedElement.TriggerOnTextInput(data);
            }
        }

        public void Add(UITransform transform)
        {
            transform.Manager = this;
            Transforms.Add(transform);
            transform.OnMarkedDirty += Transform_MarkedDirty;
            FlagForSort();
        }

        public bool GetElement(float x, float y, out UIElement output)
        {
            for (int i = 0, count = Transforms.Count; i < count; i++)
            {
                UITransform transform = Transforms[i];
                if (transform.IsActive && transform is UIElement element)
                {
                    if (element.Boundaries.Contains(new PointF(x, y)))
                    {
                        output = element;
                        return true;
                    }
                }
            }
            output = null;
            return false;
        }

        public bool GetElement(Vector2 position, out UIElement output)
        {
            return GetElement(position.X, position.Y, out output);
        }

        public UIElement GetElement(float x, float y)
        {
            GetElement(x, y, out var element);
            return element;
        }

        public UIElement GetElement(Vector2 position)
        {
            return GetElement(position.X, position.Y);
        }

        public bool Remove(UITransform transform)
        {
            int index = Transforms.FindIndex((x) => x == transform);
            if (index != -1)
            {
                Transforms.GetAndRemoveAt(index).OnMarkedDirty -= Transform_MarkedDirty;
                return true;
            }
            return false;
        }

        public void RemoveRange(IEnumerable<UITransform> transforms)
        {
            foreach (var transform in transforms)
                Remove(transform);
        }

        private void FlagForSort()
        {
            if (!IsDisposed)
                TransformsNeedSorting = true;
        }

        private void Transform_MarkedDirty(UITransform transform, DirtMarkType type)
        {
            if (type.HasFlags(DirtMarkType.DrawOrder))
                FlagForSort();
        }

        public ListArray<UITransform> SortAndGetTransforms()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UIManager));

            if (TransformsNeedSorting)
            {
                Transforms.Sort(UIDrawOrderComparer.Instance);
                TransformListSorted?.Invoke();
                TransformsNeedSorting = false;
            }
            return Transforms;
        }

        public bool IsElementMouseSensitive(UIElement element)
        {
            return element.IsIntercepting || element.IsMouseEventTrigger;
        }

        public void Update(GameTime time)
        {
            if (Input.IsAnyMouseDown(out _))
                SelectedElement = null;

            UpdateTransformsAndTriggerEvents(time);
            TriggerKeyboardEvents(SelectedElement);
        }

        public void ViewportChanged(Viewport viewport)
        {
            foreach (var transform in SortAndGetTransforms())
                transform.OnViewportChanged(viewport);
        }

        private void UpdateTransformsAndTriggerEvents(GameTime time)
        {
            bool anyDown = Input.IsAnyMouseDown(out var down);
            bool anyPressed = Input.IsAnyMousePressed(out var pressed);
            bool anyReleased = Input.IsAnyMouseReleased(out var released);

            var mouseState = Input.NewMouseState;
            var mousePos = mouseState.Position;
            float scroll = Input.MouseScroll;

            // a state used instead of breaking the loop as processing 
            // the hover events "Enter" and "Leave" is important
            bool cursorIntercepted = false;

            foreach (var transform in SortAndGetTransforms())
            {
                if (!transform.IsActive)
                    continue;

                if (transform is UIElement element)
                {
                    if (!IsElementMouseSensitive(element))
                        goto EndOfLoop;

                    bool lastHoveredOver = element.IsHovered;
                    element.IsHovered = element.Boundaries.Contains(mousePos);
                    if (element.IsMouseEventTrigger && element.IsHovered != lastHoveredOver)
                    {
                        if (element.IsHovered && !lastHoveredOver)
                            element.TriggerOnMouseEnter(mouseState);
                        else
                            element.TriggerOnMouseLeave(mouseState);
                    }

                    if (cursorIntercepted || !element.IsHovered)
                        goto EndOfLoop;

                    if (element.IsMouseEventTrigger)
                    {
                        element.TriggerOnMouseHover(mouseState);

                        if (anyDown)
                            element.TriggerOnMouseDown(mouseState, pressed);

                        if (anyPressed)
                            element.TriggerOnMousePress(mouseState, pressed);

                        if (anyReleased)
                            element.TriggerOnMouseRelease(mouseState, released);
                    }

                    if (scroll != 0)
                        element.TriggerOnScroll(scroll);

                    if (element.IsSelectable)
                        if (Input.IsAnyMouseDown(out var temp))
                            SelectedElement = element;

                    if (element.IsIntercepting)
                        cursorIntercepted = true; // instead of a 'break;'
                }

                // use a label+goto to update transform after invoking events to 
                // give the transform fresh data one frame earlier
                EndOfLoop:
                transform.Update(time);
            }
        }

        private void TriggerKeyboardEvents(UIElement element)
        {
            if (element == null)
                return;

            if (!element.IsKeyboardEventTrigger)
                return;

            var keysHeld = Input.KeysHeld;
            var keysDown = Input.KeysDown;
            var keysPressed = Input.KeysPressed;
            var keysReleased = Input.KeysReleased;

            for (int h = 0; h < keysHeld.Count; h++)
            {
                var hk = keysHeld[h];
                element.TriggerOnKeyRepeat(hk.Key, hk.Time);
            }

            for (int d = 0; d < keysDown.Count; d++)
                element.TriggerOnKeyDown(keysDown[d]);

            for (int p = 0; p < keysPressed.Count; p++)
                element.TriggerOnKeyPress(keysPressed[p]);

            for (int r = 0; r < keysReleased.Count; r++)
                element.TriggerOnKeyRelease(keysReleased[r]);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposing = true;

                if (disposing)
                {
                    for (int i = Transforms.Count; i-- > 0;)
                        Transforms[i].Dispose();
                    Transforms.Clear();

                    Input.TextInput -= Input_TextInput;
                }

                IsDisposing = false;
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UIManager()
        {
            Dispose(false);
        }
    }
}