using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace GeneralShare.UI
{
    public abstract class UIElement : UITransform
    {
        public delegate void ContentStateDelegate(bool hadContentBefore);
        public delegate void MouseHoverDelegate(MouseState mouseState);
        public delegate void GenericMouseDelegate(MouseState mouseState, MouseButton buttons);
        public delegate void GenericKeyboardDelegate(Keys key);
        
        public event GenericMouseDelegate OnMouseDown;
        public event GenericMouseDelegate OnMousePress;
        public event GenericMouseDelegate OnMouseRelease;
        public event MouseHoverDelegate OnMouseEnter;
        public event MouseHoverDelegate OnMouseHover;
        public event MouseHoverDelegate OnMouseLeave;
        public event GenericKeyboardDelegate OnKeyDown;
        public event GenericKeyboardDelegate OnKeyHeld;
        public event GenericKeyboardDelegate OnKeyPress;
        public event GenericKeyboardDelegate OnKeyRelease;
        public event Input.TextInputDelegate OnTextInput;

        public abstract RectangleF Boundaries { get; }
        public string Name { get; private set; }
        public bool TriggerMouseEvents { get; set; }
        public bool TriggerKeyEvents { get; set; }
        public bool IsSelected { get; internal set; }

        /// <summary>
        /// Returns <see langword="true"/> if the cursor is within
        /// this <see cref="UIElement"/>'s boundaries, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsHoveredOver { get; internal set; }

        /// <summary>
        /// Dictates if this <see cref="UIElement"/> intercepts mouse events.
        /// </summary>
        public bool IsIntercepting { get; set; }

        /// <summary>
        /// Dictates if this <see cref="UIElement"/> can be selected
        /// as <see cref="UIManager.SelectedElement"/>.
        /// </summary>
        public bool IsSelectable { get; set; }
        
        public UIElement(string name, UIManager manager) : base(manager)
        {
            Name = name ?? string.Empty;
            TriggerMouseEvents = false;
            TriggerKeyEvents = false;
            IsIntercepting = true;
        }

        public UIElement(UIManager manager) : this(null, manager) { }

        private void TriggerMouseHoverEvent(MouseState mouseState, MouseHoverDelegate action)
        {
            action?.Invoke(mouseState);
        }

        private void TriggerGenericMouseEvent(
            MouseState state, MouseButton buttons, GenericMouseDelegate action)
        {
            action?.Invoke(state, buttons);
        }

        private void TriggerGenericKeyboardEvent(Keys key, GenericKeyboardDelegate action)
        {
            action?.Invoke(key);
        }

        internal void TriggerOnTextInput(TextInputEventArgs e)
        {
            OnTextInput?.Invoke(e);
        }

        internal void TriggerOnMouseDown(MouseState state, MouseButton buttons)
        {
            TriggerGenericMouseEvent(state, buttons, OnMouseDown);
        }

        internal void TriggerOnMousePress(MouseState state, MouseButton buttons)
        {
            TriggerGenericMouseEvent(state, buttons, OnMousePress);
        }

        internal void TriggerOnMouseRelease(MouseState state, MouseButton buttons)
        {
            TriggerGenericMouseEvent(state, buttons, OnMouseRelease);
        }

        internal void TriggerOnMouseEnter(MouseState state)
        {
            TriggerMouseHoverEvent(state, OnMouseEnter);
        }

        internal void TriggerOnMouseHover(MouseState state)
        {
            TriggerMouseHoverEvent(state, OnMouseHover);
        }

        internal void TriggerOnMouseLeave(MouseState state)
        {
            TriggerMouseHoverEvent(state, OnMouseLeave);
        }

        internal void TriggerOnKeyDown(Keys key)
        {
            TriggerGenericKeyboardEvent(key, OnKeyDown);
        }

        internal void TriggerOnKeyHeld(Keys key)
        {
            TriggerGenericKeyboardEvent(key, OnKeyHeld);
        }

        internal void TriggerOnKeyPress(Keys key)
        {
            TriggerGenericKeyboardEvent(key, OnKeyPress);
        }

        internal void TriggerOnKeyRelease(Keys key)
        {
            TriggerGenericKeyboardEvent(key, OnKeyRelease);
        }
    }
}