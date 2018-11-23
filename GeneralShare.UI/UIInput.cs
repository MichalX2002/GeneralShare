using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralShare.UI
{
    public class UIInput : UITextBase
    {
        private int _maxCharCount;
        private int _totalTextLength;
        private bool _obscureValue;
        private char _obscureChar;
        private HashSet<char> _excludedChars;
        private ReadOnlyWrapper<char> _readonlyExcludedChars;
        private float _keyRepeatTime;
        private float _repeatKeyOccuring;

        private RectangleF _caretRect;
        private Color _visibleCaretColor;
        private Vector2 _caretSize;
        private int _caretIndex;
        private float _caretAngle;
        private float _currentCaretAlpha;
        private byte _caretAlphaThreshold;

        private string _prefix;
        private bool _prefixExpressions;
        private bool _keepPrefixExpressions;
        private Color _basePrefixColor;

        public int CaretIndex { get => _caretIndex; set => SetCaretIndex(value); }
        public Vector2 CaretSize { get => _caretSize; set => SetCaretSize(value); }
        public float CaretBlinkSpeed { get; set; }
        public Color CaretColor { get; set; }
        public byte CaretAlphaThreshold
        {
            get => _caretAlphaThreshold;
            set => _caretAlphaThreshold = (byte)Mathf.Clamp(value, 0, byte.MaxValue);
        }

        public string Value { get => _value; set => SetValue(value); }
        public string Prefix { get => _prefix; set => SetPrefix(value); }
        public bool UsePrefixExpressions { get => _prefixExpressions; set => SetPrefixExp(value); }
        public bool KeepPrefixExpressions { get => _keepPrefixExpressions; set => SetKeepPrefixExp(value); }
        public Color BasePrefixColor { get => _basePrefixColor; set => SetPrefixColor(value); }

        public float KeyRepeatDelay { get; set; } 
        public float KeyRepeatRate { get; set; }

        public bool AllowNewLine { get; set; }
        public bool ObscureValue { get => _obscureValue; set => SetObscureValue(value); }
        public char ObscureChar { get => _obscureChar; set => SetObscureChar(value); }
        public int MaxCharCount { get => _maxCharCount; set => SetMaxCharCount(value); }
        public IEnumerable<char> CharBlacklist =>  _excludedChars;

        public UIInput(UIManager manager, BitmapFont font) : base(manager, font)
        {
            _excludedChars = new HashSet<char>();
            _readonlyExcludedChars = new ReadOnlyWrapper<char>(_excludedChars);

            TriggerMouseEvents = true;
            TriggerKeyEvents = true;
            IsIntercepting = true;
            IsSelectable = true;
            UseShadow = true;
            BuildCharQuadTree = true;

            MaxCharCount = 256;
            CaretIndex = 0;
            CaretSize = new Vector2(2, -1);
            CaretBlinkSpeed = 0.666f;
            CaretColor = Color.Red;
            BasePrefixColor = Color.White;
            KeyRepeatDelay = 0.5f;
            KeyRepeatRate = 20;
            CaretAlphaThreshold = 25;

            OnMousePress += UIInput_OnMousePress;
            OnTextInput += UIInput_OnTextInput;
            OnKeyPress += UIInput_OnKeyPress;
            OnKeyRepeat += UIInput_OnKeyRepeat;
        }

        public void SetValueNow(StringBuilder builder)
        {
            SetAndProcessText(builder);
        }

        private void PauseCaretAnimation()
        {
            _repeatKeyOccuring = 0.666f;
        }

        private void SetObscureValue(bool value)
        {
            MarkDirtyE(ref _obscureValue, value, DirtMarkType.ObscureValue);
        }

        private void SetObscureChar(char value)
        {
            MarkDirtyE(ref _obscureChar, value, DirtMarkType.ObscureChar);
        }

        public bool BlacklistChar(char value)
        {
            return _excludedChars.Add(value);
        }

        public bool WhitelistChar(char value)
        {
            return _excludedChars.Remove(value);
        }

        private void SetMaxCharCount(int value)
        {
            _maxCharCount = value;
        }

        private void SetPrefixColor(Color value)
        {
            MarkDirtyE(ref _basePrefixColor, value, DirtMarkType.InputPrefix);
        }

        private void SetPrefixExp(bool value)
        {
            MarkDirtyE(ref _prefixExpressions, value, DirtMarkType.InputPrefix);
        }

        private void SetKeepPrefixExp(bool value)
        {
            MarkDirtyE(ref _keepPrefixExpressions, value, DirtMarkType.InputPrefix);
        }

        private void SetPrefix(string value)
        {
            MarkDirtyE(ref _prefix, value, DirtMarkType.InputPrefix);
        }

        private void SetCaretIndex(int value)
        {
            lock (SyncRoot)
            {
                PrepareCaretIndex(ref value);
                if (MarkDirtyE(ref _caretIndex, value, DirtMarkType.CaretIndex))
                    PauseCaretAnimation();
            }
        }

        private void SetCaretSize(Vector2 value)
        {
            MarkDirtyE(ref _caretSize, value, DirtMarkType.CaretSize);
        }

        private void UIInput_OnKeyRepeat(Keys key, float timeDown)
        {
            if (timeDown > KeyRepeatDelay)
                UIInput_OnKeyPress(key);

            // TODO: this will run at current framerate, better make it delta time based
            // we have an update function, so that's relatively simple
        }

        private void UIInput_OnKeyPress(Keys key)
        {
            lock (SyncRoot)
            {
                switch (key)
                {
                    case Keys.Left:
                    case Keys.Down:
                        CaretIndex--;
                        break;

                    case Keys.Right:
                    case Keys.Up:
                        CaretIndex++;
                        break;
                }
            }
        }

        private void UIInput_OnTextInput(TextInputEventArgs e)
        {
            lock (SyncRoot)
            {
                void Insert(char value)
                {
                    if (_excludedChars.Contains(value))
                        return;

                    int insertIndex = _caretIndex - SpecialProcessedTextLength;
                    if (Value.Length < _maxCharCount)
                    { 
                        _caretIndex++;
                        PauseCaretAnimation();

                        Value = Value.Insert(insertIndex, value.ToString());
                        MarkDirty(DirtMarkType.CaretIndex, true);
                    }
                }

                switch (e.Key)
                {
                    case Keys.Back:
                        if (Value.Length > 0)
                        {
                            int index = _caretIndex - SpecialProcessedTextLength - 1;
                            if (index >= 0)
                            {
                                CaretIndex--;
                                Value = Value.Remove(index, 1);
                            }
                        }
                        break;

                    case Keys.Delete:
                        if (Value.Length > 0)
                        {
                            int index = _caretIndex - SpecialProcessedTextLength;
                            if (index >= 0 && index < Value.Length)
                            {
                                Value = Value.Remove(index, 1);
                                MarkDirty(DirtMarkType.CaretIndex, true);
                                PauseCaretAnimation();
                            }
                        }
                        break;

                    case Keys.Enter:
                        if (AllowNewLine)
                            Insert('\n');
                        break;

                    default:
                        char c = e.Character;
                        if (char.IsLetter(c) ||
                            char.IsNumber(c) ||
                            char.IsSymbol(c) ||
                            char.IsWhiteSpace(c) ||
                            char.IsPunctuation(c))
                        {
                            Insert(c);
                        }
                        break;
                }
            }
        }

        private void UIInput_OnMousePress(MouseState mouseState, MouseButton buttons)
        {
            lock (SyncRoot)
            {
                float sizeX = Scale.X * Font.LineHeight * 0.9f;
                float sizeY = Scale.Y * Font.LineHeight;
                var pos = mouseState.Position;
                var range = new RectangleF(pos.X - sizeX, pos.Y - sizeY, sizeX * 2, sizeY * 2);

                var item = _charQuadTree.CurrentTree.QueryNearest(range, pos);
                if (item != null)
                {
                    int newIndex = (int)Math.Ceiling(item.Value);
                    SetCaretIndex(newIndex);
                }
            }
        }

        private void PrepareCaretIndex(ref int index)
        {
            int valueLength = Value == null ? 0 : Value.Length - Value.LineCount() - 1;
            int prefixLength = _prefix == null ? 0 : _prefix.Length - _prefix.LineCount() - 1;
            _totalTextLength = valueLength + prefixLength;

            if (index > _totalTextLength)
                index = _totalTextLength;
            else if (index < prefixLength)
                index = prefixLength;
            else if (index < 0)
                index = 0;
        }

        private RectangleF GetCaretPosition()
        {
            lock (SyncRoot)
            {
                Vector3 globalPos = GlobalPosition;
                RectangleF output = new RectangleF
                {
                    X = globalPos.X,
                    Y = globalPos.Y,
                    Width = _caretSize.X * Scale.X,
                    Height = (_caretSize.Y < 0 ? Font.LineHeight : _caretSize.Y) * Scale.Y
                };

                PrepareCaretIndex(ref _caretIndex);
                Vector3 lastPos = _textSprites.GetReferenceAt(_caretIndex - 1).Sprite.TR.Position;
                double rawDistance = (lastPos.Y - globalPos.Y) / Font.LineHeight;
                double line = Math.Floor(rawDistance / Scale.Y);
                double yOffset = Math.Round(globalPos.Y + line * Font.LineHeight * Scale.Y - 0.5f);

                output.X = lastPos.X;
                output.Y = (float)yOffset;

                return output;
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            lock (SyncRoot)
            {
                base.Draw(time, batch);

                if (BuildCharQuadTree == false)
                    return;
                
                if (IsSelected && _caretRect.Width > 0 && _caretRect.Height > 0)
                    batch.DrawFilledRectangle(_caretRect, _visibleCaretColor);

                /*
                void DrawTree(Collections.QuadTree<float> ree)
                {
                    foreach (var rect in ree.Items)
                    {
                        if (rect.Bounds.Width != 0 && rect.Bounds.Height != 0)
                            batch.DrawRectangle(rect.Bounds, Color.Green);
                        else
                            batch.DrawPoint(rect.Bounds.Position.ToVector2(), Color.Green, 2);
                    }
                    batch.DrawRectangle(ree.Boundary, Color.Blue);
                    if (ree.Divided)
                    {
                        DrawTree(ree.TopLeft);
                        DrawTree(ree.TopRight);
                        DrawTree(ree.BottomLeft);
                        DrawTree(ree.BottomRight);
                    }
                }
                DrawTree(_charQuadTree.CurrentTree);
                */
            }
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            if (IsSelected)
                UpdateCaretColor(time.Delta);
            else if (IsActive)
                _caretAngle = (float)(Math.PI / 2f);
        }

        private void UpdateCaretColor(float delta)
        {
            float alpha = 255f;
            if (_repeatKeyOccuring > 0f)
            {
                _repeatKeyOccuring -= delta;
            }
            else if (CaretBlinkSpeed > 0)
            {
                _caretAngle += delta;
                double sin = (Math.Sin(_caretAngle * CaretBlinkSpeed * Math.PI * 2) + 1) / 2;
                alpha = Mathf.Map((float)(CaretColor.A * sin), 0, 255, _caretAlphaThreshold, 255);
            }
            _currentCaretAlpha = Mathf.Lerp(_currentCaretAlpha, alpha, delta * 9);
            _visibleCaretColor = new Color(CaretColor, (byte)_currentCaretAlpha);
        }

        protected override void SpecialBeforeTextProcessing()
        {
            SpecialTextFormat.Format(
                new SpecialTextFormat.Input(_prefix), _processedText, _basePrefixColor, Font,
                _keepPrefixExpressions, _prefixExpressions ? _expressionColors : null);
        }

        protected override void SpecialPostTextProcessing()
        {
            if (ObscureValue)
            {
                int length = _processedText.Length - _prefix.Length;
                for (int i = _prefix.Length; i < length; i++)
                    _processedText[i] = _obscureChar;
            }
        }

        protected override void SpecialBoundaryUpdate(RectangleF input, out RectangleF output)
        {
            if (HasAnyDirtMark(DirtMarkType.Position | DirtMarkType.CaretIndex | DirtMarkType.CaretSize))
                _caretRect = GetCaretPosition();

            RectangleF.Union(input, _caretRect, out output);
            MarkDirty(DirtMarkType.ShadowMath);
        }

        protected override void Dispose(bool disposing)
        {
            OnMousePress -= UIInput_OnMousePress;
            OnTextInput -= UIInput_OnTextInput;
            OnKeyPress -= UIInput_OnKeyPress;

            base.Dispose(disposing);
        }
    }
}