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
    public class UIContainer : IDisposable
    {
        public delegate void ElementListSortedDelegate();
        private static TextureRegion2D _grayscaleRegion;

        private bool _disposed;
        private ListArray<UIElement> _uiElements;
        private UIElement __selectedElement;

        public TextureRegion2D WhitePixelRegion { get; set; }
        public TextureRegion2D GrayscaleRegion { get; set; }

        public object SyncRoot { get; private set; }
        public event ElementListSortedDelegate ElementListSorted;
        public bool ElementsNeedSorting { get; private set; }
        public ListArray<UIElement> Elements => GetElements();
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

        public UIContainer(GraphicsDevice device) : this(GetRegion(device))
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

        public UIContainer(TextureRegion2D grayscaleRegion)
        {
            SyncRoot = new object();
            GrayscaleRegion = grayscaleRegion ?? throw new ArgumentNullException(nameof(grayscaleRegion));
            WhitePixelRegion = new TextureRegion2D(grayscaleRegion.Texture, 0, 0, 1, 1);

            _uiElements = new ListArray<UIElement>();
            _uiElements.Changed += Elements_Changed;

            Input.TextInput += Input_TextInput;
        }

        private void Elements_Changed(int oldVersion, int newVersion)
        {
            FlagForSort();
        }

        private void Input_TextInput(TextInputEventArgs e)
        {
            lock (SyncRoot)
            {
                if (SelectedElement != null)
                    SelectedElement.TriggerOnTextInput(e);
            }
        }

        private void FlagForSort()
        {
            ElementsNeedSorting = true;
        }

        public void SortElements()
        {
            lock (SyncRoot)
            {
                if (ElementsNeedSorting)
                {
                    _uiElements.Sort(UIDepthComparer.Instance);
                    ElementListSorted?.Invoke();
                    ElementsNeedSorting = false;
                }
            }
        }

        public bool GetElement(float x, float y, out UIElement output)
        {
            lock (SyncRoot)
            {
                for (int i = 0, length = _uiElements.Count; i < length; i++)
                {
                    UIElement item = _uiElements[i];
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
                _uiElements.Add(component);
                component.MarkedDirty += Component_MarkedDirty;
                FlagForSort();
            }
        }

        public bool RemoveElement(UIElement element)
        {
            lock (SyncRoot)
            {
                int index = _uiElements.FindIndex((x) => x == element);
                if (index == -1)
                {
                    return false;
                }
                else
                {
                    _uiElements[index].MarkedDirty -= Component_MarkedDirty;
                    _uiElements.RemoveAt(index);
                    
                    return true;
                }
            }
        }

        public ListArray<UIElement> GetElements()
        {
            lock (SyncRoot)
            {
                SortElements();
                return _uiElements;
            }
        }

        public IEnumerable<UIElement> GetInputSensitiveElements()
        {
            lock (SyncRoot)
            {
                ListArray<UIElement> elements = GetElements();
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
            if(_disposed == false)
            {
                if (disposing)
                {
                    lock (SyncRoot)
                    {
                        for (int i = _uiElements.Count; i-- > 0;)
                        {
                            _uiElements[i]?.Dispose();
                        }
                    }
                }

                Input.TextInput -= Input_TextInput;

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UIContainer()
        {
            Dispose(false);
        }
    }
}
