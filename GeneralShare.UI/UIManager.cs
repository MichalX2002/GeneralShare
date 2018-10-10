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

        private static TextureRegion2D _grayscaleRegion;
        private UIElement __selectedElement;
        internal Viewport _lastViewport;

        public bool IsDisposed { get; private set; }
        public object SyncRoot { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }
        public TextureRegion2D GrayscaleRegion { get; set; }
        public TextureRegion2D WhitePixelRegion { get; set; }
        public SamplingMode PreferredSamplingMode { get; set; }
        public Viewport Viewport => _lastViewport;
        
        public bool TransformsNeedSorting { get; private set; }
        public ListArray<UITransform> Transforms { get; }

        /// <summary>
        /// The currently selected <see cref="UIElement"/> that (primarily) receives
        /// keyboard events (use <see cref="UIElement.IsSelected"/> to easily check if
        /// an element is selected in conjunction with <see cref="UIElement.IsSelectable"/>).
        /// </summary>
        public UIElement SelectedElement
        {
            get
            {
                lock (SyncRoot)
                    return __selectedElement;
            }
            set
            {
                lock (SyncRoot)
                {
                    if (__selectedElement != null)
                        __selectedElement.IsSelected = false;
                    __selectedElement = value;
                }
            }
        }

        /// <summary>
        /// Constructs a <see cref="UIManager"/> instance using an existing grayscale region.
        /// </summary>
        /// <param name="grayscaleRegion">
        /// A <see cref="TextureRegion2D"/> containing at least 1x1 white pixel
        /// at (X:0,Y:0) and a 1x1 gray pixel at (X:0,Y:1) 
        /// (this can be larger as linear filtering can stretch the texture).
        /// This texture is mostly used for better shading on elements.
        /// </param>
        public UIManager(GraphicsDevice device, TextureRegion2D grayscaleRegion)
        {
            GrayscaleRegion = grayscaleRegion ?? throw new ArgumentNullException(nameof(grayscaleRegion));
            if (GrayscaleRegion.Width < 1) throw new ArgumentException();
            if (GrayscaleRegion.Height < 2) throw new ArgumentException();

            WhitePixelRegion = new TextureRegion2D(grayscaleRegion.Texture, 0, 0, 1, 1);
            GraphicsDevice = device;

            SyncRoot = new object();
            Transforms = new ListArray<UITransform>();
            Transforms.Changed += Transforms_Changed;
            PreferredSamplingMode = SamplingMode.LinearClamp;

            Input.TextInput += Input_TextInput;
        }

        /// <summary>
        /// Constructs a <see cref="UIManager"/> instance with a static grayscale texture.
        /// </summary>
        /// <param name="device">
        /// The <see cref="GraphicsDevice"/> to use for creating a
        /// grayscale texture if the existing static texture is null.
        /// </param>
        public UIManager(GraphicsDevice device) : this(device, GetRegion(device))
        {
        }

        private static TextureRegion2D GetRegion(GraphicsDevice device)
        {
            if (_grayscaleRegion == null)
            {
                Color[] colors = new Color[4];
                for (int i = 0; i < 3; i++)
                {
                    colors[i] = Color.White;
                }
                colors[3] = Color.LightGray;

                var tex = new Texture2D(device, 1, 4);
                tex.SetData(colors);
                _grayscaleRegion = new TextureRegion2D(tex, 0, 2, 1, 2);
            }

            return _grayscaleRegion;
        }

        private void Transforms_Changed(int oldVersion, int newVersion)
        {
            FlagForSort();
        }

        private void Input_TextInput(TextInputEventArgs e)
        {
            if (IsDisposed == false)
            {
                lock (SyncRoot)
                {
                    if (SelectedElement != null)
                        SelectedElement.TriggerOnTextInput(e);
                }
            }
        }

        private void FlagForSort()
        {
            if (IsDisposed == false)
                TransformsNeedSorting = true;
        }

        public void SortTransforms()
        {
            lock (SyncRoot)
            {
                if (TransformsNeedSorting)
                {
                    Transforms.Sort(new UIDrawOrderComparer());
                    TransformListSorted?.Invoke();
                    TransformsNeedSorting = false;
                }
            }
        }

        public bool GetElement(float x, float y, out UIElement output)
        {
            lock (SyncRoot)
            {
                for (int i = 0, count = Transforms.Count; i < count; i++)
                {
                    UITransform item = Transforms[i];
                    if (item is UIElement element && item.IsActive)
                    {
                        if (element.Boundaries.Contains(new PointF(x, y)))
                        {
                            output = element;
                            return true;
                        }
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

        public bool GetElement(Point point, out UIElement output)
        {
            return GetElement(point.X, point.Y, out output);
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

        private void Transform_MarkedDirty(DirtMarkType type)
        {
            if (type.HasAnyFlag(DirtMarkType.DrawOrder))
                FlagForSort();
        }

        public void Add(UITransform transform)
        {
            lock (SyncRoot)
            {
                Transforms.Add(transform);
                transform.MarkedDirty += Transform_MarkedDirty;
                FlagForSort();
            }
        }

        public bool Remove(UITransform transform)
        {
            lock (SyncRoot)
            {
                int index = Transforms.FindIndex((x) => x == transform);
                if (index == -1)
                {
                    return false;
                }
                else
                {
                    Transforms[index].MarkedDirty -= Transform_MarkedDirty;
                    Transforms.RemoveAt(index);
                    
                    return true;
                }
            }
        }

        public ListArray<UITransform> GetSortedTransforms()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UIManager));

            lock (SyncRoot)
            {
                SortTransforms();
                return Transforms;
            }
        }

        public IEnumerable<UIElement> GetInputSensitiveElements()
        {
            lock (SyncRoot)
            {
                ListArray<UITransform> transforms = GetSortedTransforms();
                for (int i = 0, length = transforms.Count; i < length; i++)
                {
                    UITransform transform = transforms[i];
                    if (transform.IsEnabled == false)
                        continue;

                    if (transform is UIElement element)
                        if (IsElementInputSensitive(element))
                            yield return element;
                }
            }
        }

        public bool IsElementInputSensitive(UIElement element)
        {
            return element.IsIntercepting || element.TriggerMouseEvents;
        }

        public void Update(GameTime time)
        {
            lock (SyncRoot)
            {
                UpdateTransformsAndTriggerMouseEvents(time);
                TriggerKeyboardEvents(SelectedElement);
            }
        }

        private void UpdateTransformsAndTriggerMouseEvents(GameTime time)
        {
            Viewport freshViewport = GraphicsDevice.Viewport;
            bool viewportChanged = Viewport.EqualsTo(freshViewport);
            _lastViewport = freshViewport;

            bool anyDown = Input.IsAnyMouseDown(out var down);
            bool anyPressed = Input.IsAnyMousePressed(out var pressed);
            bool anyReleased = Input.IsAnyMouseReleased(out var released);

            MouseState mouseState = Input.NewMouseState;
            Point mousePos = mouseState.Position;

            bool cursorIntercepted = false;
            // a state used instead of 'break;'ing (the loop) as processing the 
            // mouse hover events "Enter" and "Leave" is pretty important

            ListArray<UITransform> transforms = GetSortedTransforms();
            for (int i = 0, count = transforms.Count; i < count; i++)
            {
                var transform = transforms[i];
                if (viewportChanged)
                    transform.OnViewportChange(freshViewport);

                if (transform.IsActive == false)
                    continue;

                transform.Update(time);
                if (transform is UIElement element)
                {
                    element.IsSelected = false;
                    if (!IsElementInputSensitive(element))
                        continue;

                    lock (element.SyncRoot)
                    {
                        bool lastHovering = element.IsHoveredOver;
                        element.IsHoveredOver = element.Boundaries.Contains(mousePos);

                        if (element.IsHoveredOver != lastHovering)
                        {
                            if (element.IsHoveredOver && lastHovering == false)
                                element.TriggerOnMouseEnter(mouseState);
                            else
                                element.TriggerOnMouseLeave(mouseState);
                        }

                        if (cursorIntercepted || !element.IsHoveredOver)
                            continue;

                        if (element.TriggerMouseEvents)
                        {
                            element.TriggerOnMouseHover(mouseState);

                            if (anyDown)
                                element.TriggerOnMouseDown(mouseState, pressed);

                            if (anyPressed)
                                element.TriggerOnMousePress(mouseState, pressed);

                            if (anyReleased)
                                element.TriggerOnMouseRelease(mouseState, released);
                        }

                        if (element.IsSelectable)
                        {
                            if (Input.IsAnyMouseDown(out var temp))
                                SelectedElement = element;
                        }

                        if (element.IsIntercepting == true)
                            cursorIntercepted = true; // instead of a 'break;'
                    }
                }
            }
        }

        private void TriggerKeyboardEvents(UIElement element)
        {
            if (element == null)
                return;

            lock (element.SyncRoot)
            {
                element.IsSelected = true;
                if (element.TriggerKeyEvents)
                {
                    var keysDown = Input.KeysDown;
                    var keysHeld = Input.KeysHeld;
                    var keysPressed = Input.KeysPressed;
                    var keysReleased = Input.KeysReleased;

                    for (int d = 0; d < keysDown.Count; d++)
                        element.TriggerOnKeyDown(keysDown[d]);

                    for (int h = 0; h < keysHeld.Count; h++)
                        element.TriggerOnKeyHeld(keysHeld[h]);

                    for (int p = 0; p < keysPressed.Count; p++)
                        element.TriggerOnKeyPress(keysPressed[p]);

                    for (int r = 0; r < keysReleased.Count; r++)
                        element.TriggerOnKeyRelease(keysReleased[r]);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(IsDisposed == false)
            {
                if (disposing)
                {
                    lock (SyncRoot)
                    {
                        for (int i = Transforms.Count; i-- > 0;)
                        {
                            Transforms[i]?.Dispose();
                        }

                        Input.TextInput -= Input_TextInput;
                    }
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
