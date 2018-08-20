using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.ObjectModel;
using BS = Microsoft.Xna.Framework.Input.ButtonState;
using KS = Microsoft.Xna.Framework.Input.KeyboardState;
using MB = GeneralShare.MouseButton;

namespace GeneralShare
{
    public static class Input
    {
        public delegate void TextInputDelegate(TextInputEventArgs e);

        public static event TextInputDelegate TextInput;

        private static readonly ListArray<Keys> _oldKeysDown;
        private static readonly ListArray<Keys> _keysDown;
        private static readonly ListArray<Keys> _keysPressed;
        private static readonly ListArray<Keys> _keysReleased;

        private static MouseState _oldMS;
        private static MouseState _newMS;
        public static MouseState OldMouseState => _oldMS;
        public static MouseState NewMouseState => _newMS;

        public static ReadOnlyCollection<Keys> KeysDown { get; private set; }
        public static ReadOnlyCollection<Keys> KeysPressed { get; private set; }
        public static ReadOnlyCollection<Keys> KeysReleased { get; private set; }

        public static Point MousePosition => _newMS.Position;
        public static Point MouseVelocity => new Point(_newMS.X - _oldMS.X, _newMS.Y - _oldMS.Y);
        public static float MouseScroll => _newMS.ScrollWheelValue - _oldMS.ScrollWheelValue;
        
        public static KeyModifier ModifiersDown { get; private set; }
        public static bool AltDown { get; private set; }
        public static bool CtrlDown { get; private set; }
        public static bool ShiftDown { get; private set; }
        public static bool NumLock { get; private set; }
        public static bool CapsLock { get; private set; }

        static Input()
        {
            _oldMS = Mouse.GetState();
            _newMS = Mouse.GetState();

            _oldKeysDown = new ListArray<Keys>(KS.MaxKeysPerState);
            _keysDown = new ListArray<Keys>(KS.MaxKeysPerState);
            _keysPressed = new ListArray<Keys>(KS.MaxKeysPerState);
            _keysReleased = new ListArray<Keys>(KS.MaxKeysPerState);

            KeysDown = _keysDown.AsReadOnly();
            KeysPressed = _keysPressed.AsReadOnly();
            KeysReleased = _keysReleased.AsReadOnly();
        }

        public static void AddWindow(GameWindow window)
        {
            window.TextInput += Window_TextInput;
        }

        public static void RemoveWindow(GameWindow window)
        {
            window.TextInput -= Window_TextInput;
        }

        private static void Window_TextInput(object s, TextInputEventArgs e)
        {
            TextInput?.Invoke(e);
        }

        public static bool IsKeyUp(Keys key)
        {
            return !_keysDown.Contains(key);
        }

        public static bool IsKeyDown(Keys key)
        {
            return !IsKeyUp(key);
        }

        public static bool IsKeyPressed(Keys key)
        {
            return _keysPressed.Contains(key);     
        }

        public static bool IsKeyReleased(Keys key)
        {
            return _keysReleased.Contains(key);    
        }

        public static bool IsMouseDown(MB buttons)
        {
            return GetMState(_newMS, buttons, BS.Pressed);
        }

        public static bool IsMouseUp(MB buttons)
        {
            return GetMState(_newMS, buttons, BS.Released);
        }

        public static bool IsMousePressed(MB buttons)
        {
            return IsMPressedInternal(buttons, in _newMS, in _oldMS);
        }

        public static bool IsMouseReleased(MB buttons)
        {
            return IsMPressedInternal(buttons, in _oldMS, in _newMS);
        }

        public static bool IsAnyMouseDown(out MB pressedButtons)
        {
            return GetMState(_newMS, MB.All, BS.Pressed, out pressedButtons);
        }

        public static bool IsAnyMouseUp(out MB pressedButtons)
        {
            return GetMState(_newMS, MB.All, BS.Released, out pressedButtons);
        }

        public static bool IsAnyMousePressed(out MB pressedButtons)
        {
            return IsAnyMPressedInternal(_newMS, _oldMS, out pressedButtons);
        }

        public static bool IsAnyMouseReleased(out MB releasedButtons)
        {
            return IsAnyMPressedInternal(_oldMS, _newMS, out releasedButtons);
        }

        private static bool IsMPressedInternal(MB buttons,
            in MouseState pressedState, in MouseState releasedState)
        {
            return
                GetMState(pressedState, buttons, BS.Pressed) &&
                GetMState(releasedState, buttons, BS.Released);
        }

        private static bool IsAnyMPressedInternal(
            in MouseState pressed, in MouseState released, out MB buttons)
        {
            bool anyDown = GetMState(pressed, MB.All, BS.Pressed, out MB down);
            bool anyUp = GetMState(released, MB.All, BS.Released, out MB up);

            if (anyDown == true || anyUp == true)
            {
                buttons = down & up;
                return buttons != 0;
            }
            else
            {
                buttons = MB.None;
                return false;
            }
        }

        private static bool GetMState(in MouseState state, MB buttons, BS press)
        {
            return GetMState(state, buttons, press, out var outButtons);
        }

        private static bool GetMState(in MouseState state,
            MB buttons, BS pressState, out MB pressed)
        {
            bool anyPressed = false;
            MB output = MB.None;

            void Check(BS bState, MB check)
            {
                if ((buttons & check) == check)
                {
                    if (bState == pressState)
                    {
                        output |= check;
                        anyPressed = true;
                    }
                }
            }

            Check(state.LeftButton, MB.Left);
            Check(state.MiddleButton, MB.Middle);
            Check(state.RightButton, MB.Right);
            Check(state.XButton1, MB.XButton1);
            Check(state.XButton2, MB.XButton2);

            pressed = anyPressed ? output & ~MB.None : MB.None;
            return anyPressed;
        }

        public static void Update()
        {
            _oldMS = _newMS;
            _newMS = Mouse.GetState();
            
            _oldKeysDown.Clear(false);
            _oldKeysDown.AddRange(_keysDown);

            _keysDown.Clear(false);
            _keysDown.AddRange(Keyboard.KeyList);
            // getting the KeyList updates it's internal keyboard state (and updates the Modifiers property)
            // (getting the KeyList to update the internal state is only required on DirectX,
            //  on DesktopGL, the KeyList and Modifiers properties are updated constantly by the SDL loop)
            
            // therefore update Modifiers after getting Keyboard.KeyList
            UpdateModifiers(); 

            GetKeyDifferences(_keysPressed, _keysDown, _oldKeysDown);
            GetKeyDifferences(_keysReleased, _oldKeysDown, _keysDown);
        }

        private static void UpdateModifiers()
        {
            ModifiersDown = Keyboard.Modifiers;
            CtrlDown = ModifiersDown.HasFlags(KeyModifier.Ctrl, KeyModifier.LeftCtrl, KeyModifier.RightCtrl);
            AltDown = ModifiersDown.HasFlags(KeyModifier.Alt, KeyModifier.LeftAlt, KeyModifier.RightAlt);
            ShiftDown = ModifiersDown.HasFlags(KeyModifier.Shift, KeyModifier.LeftShift, KeyModifier.RightShift);
            NumLock = ModifiersDown.HasFlags(KeyModifier.NumLock);
            CapsLock = ModifiersDown.HasFlags(KeyModifier.CapsLock);
        }

        private static void GetKeyDifferences(
            ListArray<Keys> output, ListArray<Keys> keys1, ListArray<Keys> keys2)
        {
            output.Clear(false);
            for (int i = 0; i < keys1.Count; i++)
            {
                Keys key = keys1[i];
                if (keys2.Contains(key) == false)
                    output.Add(key);
            }
        }
    }
}
