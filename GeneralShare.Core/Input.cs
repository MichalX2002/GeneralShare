using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using BS = Microsoft.Xna.Framework.Input.ButtonState;
using KS = Microsoft.Xna.Framework.Input.KeyboardState;
using MB = GeneralShare.MouseButton;

namespace GeneralShare
{
    public static class Input
    {
        public delegate void TextInputDelegate(TextInputEventArgs e);
        public static event TextInputDelegate TextInput;

        private static readonly ListArray<HeldKey> _lastKeysHeld;
        private static readonly ListArray<HeldKey> _keysHeld;
        private static readonly ListArray<Keys> _oldKeysDown;
        private static readonly ListArray<Keys> _keysDown;
        private static readonly ListArray<Keys> _keysPressed;
        private static readonly ListArray<Keys> _keysReleased;

        private static MouseState _oldMS;
        private static MouseState _newMS;
        public static MouseState OldMouseState => _oldMS;
        public static MouseState NewMouseState => _newMS;

        public static IReadOnlyList<Keys> KeysDown { get; private set; }
        public static IReadOnlyList<HeldKey> KeysHeld { get; private set; }
        public static IReadOnlyList<Keys> KeysPressed { get; private set; }
        public static IReadOnlyList<Keys> KeysReleased { get; private set; }

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

            _lastKeysHeld = new ListArray<HeldKey>(KS.MaxKeysPerState);
            _keysHeld = new ListArray<HeldKey>(KS.MaxKeysPerState);
            _oldKeysDown = new ListArray<Keys>(KS.MaxKeysPerState);
            _keysDown = new ListArray<Keys>(KS.MaxKeysPerState);
            _keysPressed = new ListArray<Keys>(KS.MaxKeysPerState);
            _keysReleased = new ListArray<Keys>(KS.MaxKeysPerState);

            KeysDown = _keysDown.AsReadOnly();
            KeysHeld = _keysHeld.AsReadOnly();
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

        public static bool IsKeyHeld(Keys key, float timeThreshold)
        {
            for (int i = 0; i < _keysHeld.Count; i++)
            {
                var k = _keysHeld[i];
                if (k.Key == key && k.Time >= timeThreshold)
                    return true;
            }
            return false;
        }

        public static bool IsKeyHeld(Keys key)
        {
            return IsKeyHeld(key, 0.5f);
        }

        public static bool IsKeyDown(Keys key)
        {
            return _keysDown.Contains(key);
        }

        public static bool IsKeyUp(Keys key)
        {
            return !IsKeyDown(key);
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
            return GetMState(pressedState, buttons, BS.Pressed) 
                && GetMState(releasedState, buttons, BS.Released);
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

        public static void Update(GameTime time)
        {
            _oldMS = _newMS;
            _newMS = Mouse.GetState();
            
            _oldKeysDown.Clear();
            _oldKeysDown.AddRange(_keysDown);

            // Getting KeyList updates it's internal keyboard state (and Modifiers) on DirectX, 
            // DesktopGL (the SDL window loop) updates KeyList and Modifiers constantly,
            // therefore call UpdateModifiers *after* getting KeyList.
            var keyList = Keyboard.KeyList;

            _keysDown.Clear();
            _keysDown.AddRange(keyList);
            UpdateModifiers(); 

            GetKeyDifferences(_keysPressed, _keysDown, _oldKeysDown);
            GetKeyDifferences(_keysReleased, _oldKeysDown, _keysDown);
            UpdateHeldKeys(time);
        }

        private static void UpdateModifiers()
        {
            ModifiersDown = Keyboard.Modifiers;
            CtrlDown = ModifiersDown.HasAnyFlag(KeyModifier.Ctrl, KeyModifier.LeftCtrl, KeyModifier.RightCtrl);
            AltDown = ModifiersDown.HasAnyFlag(KeyModifier.Alt, KeyModifier.LeftAlt, KeyModifier.RightAlt);
            ShiftDown = ModifiersDown.HasAnyFlag(KeyModifier.Shift, KeyModifier.LeftShift, KeyModifier.RightShift);
            NumLock = ModifiersDown.HasAnyFlag(KeyModifier.NumLock);
            CapsLock = ModifiersDown.HasAnyFlag(KeyModifier.CapsLock);
        }

        private static void GetKeyDifferences(
            ListArray<Keys> output, ListArray<Keys> keys1, ListArray<Keys> keys2)
        {
            output.Clear();
            for (int i = 0; i < keys1.Count; i++)
            {
                Keys key = keys1[i];
                if (keys2.Contains(key) == false)
                    output.Add(key);
            }
        }

        private static void UpdateHeldKeys(GameTime time)
        {
            _lastKeysHeld.AddRange(_keysHeld);
            _keysHeld.Clear();
            for (int i = 0; i < _keysDown.Count; i++)
            {
                AddHeldKey(time.Delta, _keysDown[i]);
            }
            _lastKeysHeld.Clear();
        }

        private static void AddHeldKey(float delta, Keys key)
        {
            for (int i = 0; i < _lastKeysHeld.Count; i++)
            {
                HeldKey last = _lastKeysHeld[i];
                if (last.Key == key)
                {
                    last._time += delta;
                    _keysHeld.Add(last);
                    return; // return as we only want one (the previous) HeldKey in the list
                }
            }

            // add a new HeldKey if it didn't exist before
            _keysHeld.Add(new HeldKey(key));
        }
        
        public struct HeldKey
        {
            internal float _time;

            public Keys Key { get; }
            public float Time => _time;

            public HeldKey(Keys key)
            {
                Key = key;
                _time = 0;
            }
        }
    }
}
