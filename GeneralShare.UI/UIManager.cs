using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
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

        public bool IsDisposed { get; private set; }
        public GraphicsDevice GraphicsDevice { get; }
        public TextureRegion2D GrayscaleRegion { get; set; }
        public TextureRegion2D WhitePixelRegion { get; set; }
        public SamplingMode PreferredSampling { get; set; }
        public Viewport Viewport { get; private set; }

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

        ///// <summary>
        ///// Constructs a <see cref="UIManager"/> instance with a static grayscale texture.
        ///// </summary>
        ///// <param name="device">
        ///// The <see cref="GraphicsDevice"/> to use for creating a
        ///// grayscale texture if the existing static texture is null.
        ///// </param>
        //public UIManager(GraphicsDevice device) : this(device, GetRegion(device))
        //{
        //
        //}

        //private static TextureRegion2D GetRegion(GraphicsDevice device)
        //{
        //    if (_grayscaleRegion == null)
        //    {
        //        var tex = new Texture2D(device, 1, 4);
        //        tex.SetData(new Color[4] { Color.White, Color.White, Color.White, Color.LightGray });
        //        _grayscaleRegion = new TextureRegion2D(tex, 0, 2, 1, 2);
        //    }
        //    return _grayscaleRegion;
        //}

        private void Transforms_Changed(int oldVersion, int newVersion)
        {
            FlagForSort();
        }

        private void Input_TextInput(int character, Keys key)
        {
            if (!IsDisposed)
            {
                if (SelectedElement != null && SelectedElement.IsKeyboardEventTrigger)
                    SelectedElement.TriggerOnTextInput(character, key);
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
            if (IsDisposed == false)
                TransformsNeedSorting = true;
        }

        private void Transform_MarkedDirty(UITransform transform, DirtMarkType type)
        {
            if (type.HasFlags(DirtMarkType.DrawOrder))
                FlagForSort();
        }
        
        public ListArray<UITransform> GetSortedTransformList()
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

        //public IEnumerable<UIElement> GetInputSensitiveElements()
        //{
        //    ListArray<UITransform> transforms = GetSortedTransformList();
        //    for (int i = 0, length = transforms.Count; i < length; i++)
        //    {
        //        UITransform transform = transforms[i];
        //        if (transform.IsActive && transform is UIElement element)
        //            if (IsElementMouseSensitive(element))
        //                yield return element;
        //    }
        //}

        public void Update(GameTime time)
        {
            if (Input.IsAnyMouseDown(out MouseButton tmp))
                SelectedElement = null;

            UpdateTransformsAndTriggerEvents(time);
            TriggerKeyboardEvents(SelectedElement);
        }

        private void UpdateTransformsAndTriggerEvents(GameTime time)
        {
            Viewport freshViewport = GraphicsDevice.Viewport;
            bool viewportChanged = Viewport.EqualsTo(freshViewport);
            Viewport = freshViewport;

            bool anyDown = Input.IsAnyMouseDown(out var down);
            bool anyPressed = Input.IsAnyMousePressed(out var pressed);
            bool anyReleased = Input.IsAnyMouseReleased(out var released);

            var mouseState = Input.NewMouseState;
            var mousePos = mouseState.Position;
            float scroll = Input.MouseScroll;

            // a state used instead of 'break;'ing (the loop) as processing the 
            // mouse hover events "Enter" and "Leave" is important
            bool cursorIntercepted = false;

            ListArray<UITransform> transforms = GetSortedTransformList();
            for (int i = 0, count = transforms.Count; i < count; i++)
            {
                var transform = transforms[i];
                if (viewportChanged)
                    transform.OnViewportChange(freshViewport);

                if (!transform.IsActive)
                    continue;

                transform.Update(time);

                if (transform is UIElement element)
                {
                    if (!IsElementMouseSensitive(element))
                        continue;

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
                        continue;

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
            if(!IsDisposed)
            {
                if (disposing)
                {
                    for (int i = Transforms.Count; i-- > 0;)
                        Transforms[i].FastDispose();

                    Input.TextInput -= Input_TextInput;
                }

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
