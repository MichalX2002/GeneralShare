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
        public delegate void ElementListSortedDelegate();

        public event ElementListSortedDelegate ElementListSorted;

        private static TextureRegion2D _grayscaleRegion;
        private UIElement __selectedElement;

        public TextureRegion2D GrayscaleRegion { get; set; }
        public TextureRegion2D WhitePixelRegion { get; set; }

        public bool Disposed { get; private set; }
        public object SyncRoot { get; private set; }
        public SamplingMode PreferredSamplingMode { get; set; }
        public bool ElementsNeedSorting { get; private set; }
        public ListArray<UIElement> Elements { get; }

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
        /// A <see cref="TextureRegion2D"/> containing a 1x1 white pixel
        /// at (X:0,Y:0) and a 1x1 gray pixel at (X:0,Y:1).
        /// </param>
        public UIManager(TextureRegion2D grayscaleRegion)
        {
            GrayscaleRegion = grayscaleRegion ?? throw new ArgumentNullException(nameof(grayscaleRegion));
            if (GrayscaleRegion.Width != 1) throw new ArgumentException();
            if (GrayscaleRegion.Height != 2) throw new ArgumentException();

            WhitePixelRegion = new TextureRegion2D(grayscaleRegion.Texture, 0, 0, 1, 1);

            SyncRoot = new object();
            Elements = new ListArray<UIElement>();
            Elements.Changed += Elements_Changed;

            Input.TextInput += Input_TextInput;
        }

        /// <summary>
        /// Constructs a <see cref="UIManager"/> instance with a static grayscale texture.
        /// </summary>
        /// <param name="device">
        /// The <see cref="GraphicsDevice"/> to use for creating a
        /// grayscale texture if the existing static texture is null.
        /// </param>
        public UIManager(GraphicsDevice device) : this(GetRegion(device))
        {
        }

        private static TextureRegion2D GetRegion(GraphicsDevice device)
        {
            if (_grayscaleRegion == null)
            {
                var tex = new Texture2D(device, 1, 2);
                tex.SetData(new Color[] { Color.White, Color.Gray });
                _grayscaleRegion = new TextureRegion2D(tex, 0, 0, 1, 2);
            }

            return _grayscaleRegion;
        }

        private void Elements_Changed(int oldVersion, int newVersion)
        {
            FlagForSort();
        }

        private void Input_TextInput(TextInputEventArgs e)
        {
            if (Disposed == false)
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
            if (Disposed == false)
                ElementsNeedSorting = true;
        }

        public void SortElements()
        {
            lock (SyncRoot)
            {
                if (ElementsNeedSorting)
                {
                    Elements.Sort(UIDepthComparer.Instance);
                    ElementListSorted?.Invoke();
                    ElementsNeedSorting = false;
                }
            }
        }

        public bool GetElement(float x, float y, out UIElement output)
        {
            lock (SyncRoot)
            {
                for (int i = 0, length = Elements.Count; i < length; i++)
                {
                    UIElement item = Elements[i];
                    if (item.Boundaries.Contains(new Point2(x, y)))
                    {
                        output = item;
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

        private void Component_MarkedDirty(DirtMarkType type)
        {
            if (type.HasFlagF(DirtMarkType.Position))
                FlagForSort();
        }

        public void AddElement(UIElement component)
        {
            lock (SyncRoot)
            {
                Elements.Add(component);
                component.MarkedDirty += Component_MarkedDirty;
                FlagForSort();
            }
        }

        public bool RemoveElement(UIElement element)
        {
            lock (SyncRoot)
            {
                int index = Elements.FindIndex((x) => x == element);
                if (index == -1)
                {
                    return false;
                }
                else
                {
                    Elements[index].MarkedDirty -= Component_MarkedDirty;
                    Elements.RemoveAt(index);
                    
                    return true;
                }
            }
        }

        public ListArray<UIElement> GetSortedElements()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(UIManager));

            lock (SyncRoot)
            {
                SortElements();
                return Elements;
            }
        }

        public IEnumerable<UIElement> GetInputSensitiveElements()
        {
            lock (SyncRoot)
            {
                ListArray<UIElement> elements = GetSortedElements();
                for (int i = 0, length = elements.Count; i < length; i++)
                {
                    UIElement element = elements[i];
                    if (element.BlockCursor || element.TriggerMouseEvents)
                        yield return element;
                }
            }
        }

        public void Update()
        {
            bool anyDown = Input.IsAnyMouseDown(out var down);
            bool anyPressed = Input.IsAnyMousePressed(out var pressed);
            bool anyReleased = Input.IsAnyMouseReleased(out var released);

            MouseState state = Input.NewMouseState;
            Point mousePos = state.Position;

            lock (SyncRoot)
            {
                foreach (var senseElement in GetInputSensitiveElements())
                {
                    lock (senseElement.SyncRoot)
                    {
                        senseElement.IsMouseHovering = senseElement.Boundaries.Contains(mousePos);
                        if (senseElement.IsMouseHovering)
                        {
                            if (senseElement.TriggerMouseEvents)
                            {
                                if (anyDown)
                                    senseElement.TriggerOnMouseDown(state, pressed);

                                if (anyPressed)
                                    senseElement.TriggerOnMousePress(state, pressed);

                                if (anyReleased)
                                    senseElement.TriggerOnMouseRelease(state, released);
                            }

                            if (senseElement.AllowSelection)
                            {
                                if (Input.IsAnyMouseDown(out var temp))
                                    SelectedElement = senseElement;
                            }

                            if (senseElement.BlockCursor == true)
                                break;
                        }
                    }
                }

                var element = SelectedElement;
                if (element != null)
                {
                    lock (element.SyncRoot)
                    {
                        element.IsSelected = true;
                        if (element.TriggerKeyEvents)
                        {
                            var keysDown = Input.KeysDown;
                            var keysPressed = Input.KeysPressed;
                            var keysReleased = Input.KeysReleased;

                            for (int d = 0; d < keysDown.Count; d++)
                                element.TriggerOnKeyDown(keysDown[d]);

                            for (int p = 0; p < keysPressed.Count; p++)
                                element.TriggerOnKeyPress(keysPressed[p]);

                            for (int r = 0; r < keysReleased.Count; r++)
                                element.TriggerOnKeyRelease(keysReleased[r]);
                        }
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if(Disposed == false)
            {
                if (disposing)
                {
                    lock (SyncRoot)
                    {
                        for (int i = Elements.Count; i-- > 0;)
                        {
                            Elements[i]?.Dispose();
                        }
                    }
                }

                Input.TextInput -= Input_TextInput;

                Disposed = true;
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
