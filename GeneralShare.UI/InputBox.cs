using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class InputBox : TextBox
    {
        private RectangleF _caretRect;
        private Color _visibleCaretColor;
        private Vector2 _caretSize;
        private int _caretIndex;
        private int _maxCharCount;
        private int _totalTextLength;
        private HashSet<char> _excludedChars;
        private ReadOnlyCollectionWrapper<char> _readonlyExcludedChars;

        private string _prefixText;
        private bool _prefixExpressions;
        private bool _keepPrefixExpressions;
        private Color _basePrefixColor;

        public int CaretIndex { get => _caretIndex; set => SetCaretIndex(value); }
        public Vector2 CaretSize { get => _caretSize; set => SetCaretSize(value); }
        public float CaretBlinkSpeed { get; set; }
        public Color CaretColor { get; set; }

        public string Prefix { get => _prefixText; set => SetPrefix(value); }
        public bool UsePrefixExpressions { get => _prefixExpressions; set => SetPrefixExp(value); }
        public bool KeepPrefixExpressions { get => _keepPrefixExpressions; set => SetKeepPrefixExp(value); }
        public Color BasePrefixColor { get => _basePrefixColor; set => SetPrefixColor(value); }

        public bool AllowNewLine { get; set; }
        public int MaxCharCount { get => _maxCharCount; set => SetCharCount(value); }
        public IEnumerable<char> CharBlacklist =>  _excludedChars;

        public InputBox(UIContainer container, BitmapFont font) : base(container, font)
        {
            TriggerMouseEvents = true;
            TriggerKeyEvents = true;
            BlockCursor = true;
            AllowSelection = true;
            
            UseShadow = true;
            UseTextExpressions = false;
            BuildTextQuadTree = true;

            _excludedChars = new HashSet<char>();
            _readonlyExcludedChars = new ReadOnlyCollectionWrapper<char>(_excludedChars);
            SetCharCount(200);
            CaretIndex = 0;
            CaretSize = new Vector2(2, -1);
            CaretBlinkSpeed = 1;
            CaretColor = Color.Red;
            BasePrefixColor = Color.White;

            OnMousePress += InputBox_OnMousePress;
            OnTextInput += InputBox_OnTextInput;
            OnKeyPress += InputBox_OnKeyPress;
        }

        public bool BlacklistChar(char value)
        {
            return _excludedChars.Add(value);
        }

        public bool WhitelistChar(char value)
        {
            return _excludedChars.Remove(value);
        }

        private void SetCharCount(int value)
        {
            _maxCharCount = value;
        }

        private void SetPrefixColor(Color value)
        {
            MarkDirtyEquatable(ref _basePrefixColor, value, DirtMarkType.InputPrefix);
        }

        private void SetPrefixExp(bool value)
        {
            MarkDirtyEquatable(ref _prefixExpressions, value, DirtMarkType.InputPrefix);
        }

        private void SetKeepPrefixExp(bool value)
        {
            MarkDirtyEquatable(ref _keepPrefixExpressions, value, DirtMarkType.InputPrefix);
        }

        private void SetPrefix(string value)
        {
            MarkDirtyEquatable(ref _prefixText, value, DirtMarkType.InputPrefix);
        }

        private void SetCaretIndex(int value)
        {
            SetCaretIndexInternal(value);
        }

        private void SetCaretIndexInternal(int value)
        {
            lock (_syncRoot)
            {
                PrepareCaretIndex(ref value);
                MarkDirtyEquatable(ref _caretIndex, value, DirtMarkType.CaretIndex);
            }
        }

        private void SetCaretSize(Vector2 value)
        {
            MarkDirtyEquatable(ref _caretSize, value, DirtMarkType.CaretSize);
        }

        private void InputBox_OnKeyPress(Keys key)
        {
            lock (_syncRoot)
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

        private void InputBox_OnTextInput(TextInputEventArgs e)
        {
            lock (_syncRoot)
            {
                void Insert(char value)
                {
                    if (_excludedChars.Contains(value))
                        return;

                    int insertIndex = _caretIndex - SpecialProcessedTextLength;
                    if (Value.Length < _maxCharCount)
                    {
                        _caretIndex++;
                        Value = Value.Insert(insertIndex, value.ToString());
                        MarkDirty(DirtMarkType.CaretIndex, true);
                    }
                }

                switch (e.Key)
                {
                    case Keys.Back:
                    case Keys.Delete:
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

                    case Keys.Enter:
                        if (AllowNewLine)
                            Insert('\n');
                        break;

                    default:
                        char c = e.Character;
                        if (char.IsLetterOrDigit(c) ||
                            char.IsPunctuation(c) ||
                            char.IsWhiteSpace(c) ||
                            char.IsSymbol(c))
                        {
                            Insert(c);
                        }
                        break;
                }
            }
        }

        private void InputBox_OnMousePress(in MouseState mouseState, MouseButton buttons)
        {
            lock (_syncRoot)
            {
                float sizeX = _scale.X * _font.LineHeight * 0.9f;
                float sizeY = _scale.Y * _font.LineHeight;
                var pos = mouseState.Position;
                var range = new RectangleF(pos.X - sizeX, pos.Y - sizeY, sizeX * 2, sizeY * 2);

                var item = _textQuadTree.CurrentTree.QueryNearest(range, pos);
                if (item != null)
                {
                    int newIndex = (int)Math.Ceiling(item.Value);
                    SetCaretIndexInternal(newIndex);
                }
            }
        }

        private void PrepareCaretIndex(ref int index)
        {
            string value = Value;
            int valueLength = value == null ? 0 : value.Length;
            int prefixLength = _prefixText == null ? 0 : _prefixText.Length;
            _totalTextLength = valueLength + prefixLength;

            if (index > _totalTextLength)
            {
                index = _totalTextLength;
            }
            else if (index < prefixLength)
            {
                index = prefixLength;
            }
            else if (index < 0)
            {
                index = 0;
            }
        }

        private RectangleF GetCaretPosition()
        {
            lock (_syncRoot)
            {
                RectangleF output = new RectangleF
                {
                    X = _position.X,
                    Y = _position.Y,
                    Width = _caretSize.X * _scale.X,
                    Height = (_caretSize.Y < 0 ? Font.LineHeight : _caretSize.Y) * _scale.Y
                };

                PrepareCaretIndex(ref _caretIndex);
                if (_caretIndex > 0 && _caretIndex <= _totalTextLength)
                {
                    Vector3 lastPos = _textCache.GetReferenceAt(_caretIndex - 1).Sprite.TR.Position;
                    double rawDistance = (lastPos.Y - _position.Y) / Font.LineHeight;
                    double line = Math.Floor(rawDistance / _scale.Y);
                    double yOffset = Math.Round(_position.Y + line * Font.LineHeight * _scale.Y - 0.5f);

                    output.X = lastPos.X;
                    output.Y = (float)yOffset;
                }
                
                return output;
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            lock (_syncRoot)
            {
                base.Draw(time, batch);

                if (BuildTextQuadTree == false)
                    return;

                if (IsSelected)
                {
                    if (CaretBlinkSpeed <= 0)
                    {
                        _visibleCaretColor = CaretColor;
                    }
                    else
                    {
                        double totalSec = time.TotalGameTime.TotalSeconds;
                        double sin = Math.Sin(totalSec * CaretBlinkSpeed * Math.PI * 2) + 1;
                        _visibleCaretColor = new Color(CaretColor, (int)(CaretColor.A * sin * 0.5f));
                    }

                    if (_caretRect.Width > 0 && _caretRect.Height > 0)
                        batch.DrawFilledRectangle(_caretRect, _visibleCaretColor);
                }

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

                DrawTree(_textQuadTree.CurrentTree);
                */
            }
        }

        protected override void SpecialTextProcessing()
        {
            UseTextExpressions = false;
            if (UsePrefixExpressions)
            {
                TextFormatter.ColorFormat(_prefixText, _processedText,
                    _basePrefixColor, _font, _keepPrefixExpressions, _expressionColors);
            }
            else
            {
                _processedText.Append(_prefixText);
            }
        }

        protected override void SpecialBoundaryUpdate(in RectangleF input, out RectangleF output)
        {
            if (HasDirtMarks(DirtMarkType.Position | DirtMarkType.CaretIndex | DirtMarkType.CaretSize))
                _caretRect = GetCaretPosition();

            RectangleF.Union(input, _caretRect, out output);
            MarkDirty(DirtMarkType.ShadowMath);
        }
    }
}