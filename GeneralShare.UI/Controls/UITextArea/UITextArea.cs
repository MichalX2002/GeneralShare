using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public partial class UITextArea : UITextElement
    {
        public const char DefaultObscureChar = '*';

        private char _obscureChar;
        private bool _isObscured;
        private bool _isMultiLined;
        private int _charLimit;

        private TextSegment _placeholderSegment;
        private Color _placeholderColor;
        private float _placeholderColorLerp;

        // TODO: implement key repeating
        private float _keyRepeatTime;
        private float _repeatKeyOccuring;

        #region Properties
        protected override bool AllowTextColorFormatting => false;
        protected bool IsPlaceholderVisible => base.GetSpriteCount() == 0;

        public CaretData Caret { get; }

        public ObservableHashSet<char> CharBlacklist { get; }
        public bool IsObscured { get => _isObscured; set => SetIsObscured(value); }
        public char ObscureChar { get => _obscureChar; set => SetObscureChar(value); }

        public ICharIterator Value { get => TextValue; set => TextValue = value; }
        public bool IsMultiLined { get => _isMultiLined; set => SetIsMultiLined(value); }
        public int CharacterLimit { get => _charLimit; set => SetCharLimit(value); }
        public Color SelectionColor { get; set; }

        public ICharIterator Placeholder { get => _placeholderSegment.CurrentText; set => SetPlaceholder(value); }
        public bool UsePlaceholderColorFormatting { get; set; }
        public Color PlaceholderColor { get => _placeholderColor; set => SetPlaceholderColor(value); }
        public Color PlaceholderSelectColor { get; set; }
        public float PlaceholderColorTransitionSpeed { get; set; }
        public float SelectionOutlineThickness { get; set; }
        #endregion

        public UITextArea(UIManager manager, BitmapFont font) : base(manager, font)
        {
            Caret = new CaretData();

            _placeholderSegment = new TextSegment(font);
            UsePlaceholderColorFormatting = true;
            PlaceholderColor = Color.Gray * 0.825f;
            PlaceholderSelectColor = Color.LightGoldenrodYellow * 0.9f;
            PlaceholderColorTransitionSpeed = 0.15f;

            _obscureChar = DefaultObscureChar;
            _charLimit = 4096;
            
            CharBlacklist = new ObservableHashSet<char>();
            CharBlacklist.OnAdd += (s, value) => Remove(value);
            IsKeyboardEventTrigger = true;
            IsMouseEventTrigger = true;
            IsSelectable = true;
            SelectionColor = Color.OrangeRed;
            SelectionOutlineThickness = 2f;

            OnKeyRepeat += UITextArea_OnKeyRepeat;
            OnKeyPress += UITextArea_OnKeyPress;
            OnTextInput += UITextArea_OnTextInput;
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            if (IsHovered || IsSelected)
            {
                LerpPlaceholderColor(time, 1);
            }
            else if (_placeholderSegment.Color != _placeholderColor)
            {
                LerpPlaceholderColor(time, 0);
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch)
        {
            base.Draw(time, batch);

            if (IsPlaceholderVisible)
                batch.DrawString(_placeholderSegment, StartPosition);

            if (IsSelected && GetSpriteCount() > 0)
            {
                var outlineOffset = new RectangleF(
                    -SelectionOutlineThickness,
                    -SelectionOutlineThickness,
                    SelectionOutlineThickness * 2,
                    SelectionOutlineThickness * 2);

                batch.DrawRectangle(Boundaries + outlineOffset, SelectionColor, SelectionOutlineThickness);
            }
        }

        private void LerpPlaceholderColor(GameTime time, float dst)
        {
            float transitionSpeed = 1f;
            if(PlaceholderColorTransitionSpeed > 0)
                transitionSpeed = time.Delta * 1f / PlaceholderColorTransitionSpeed * 2;

            _placeholderColorLerp = Mathf.Lerp(_placeholderColorLerp, dst, transitionSpeed);
            _placeholderSegment.Color = Color.Lerp(_placeholderColor, PlaceholderSelectColor, _placeholderColorLerp);
            _placeholderSegment.ApplyColors();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_placeholderSegment != null)
                {
                    _placeholderSegment.Dispose();
                    _placeholderSegment = null;
                }
            }

            base.Dispose(disposing);
        }

        public class CaretData
        {
            public int Offset;
            public int StartIndex;
            public int SelectionCount;
        }
    }
}