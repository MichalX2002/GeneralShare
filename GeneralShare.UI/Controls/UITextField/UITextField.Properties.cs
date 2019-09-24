using GeneralShare.Collections;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public partial class UITextField : UITextElement
    {
        #region Properties
        protected override bool AllowTextColorFormatting => false;

        public CaretData Caret { get; }
        public ValidateKeyDelegate ValidateInput
        {
            get => _validateInput;
            set
            {
                if (value == null)
                    _validateInput = DefaultValidateInput;
                else
                    _validateInput = value;
            }
        }

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

        #region Property Getters
        protected override SizeF GetMeasure()
        {
            SizeF measure = base.GetMeasure();
            if (Length == 0)
                measure += _placeholderSegment.Measure;
            return measure;
        }
        #endregion

        #region Property Setters
        private void SetPlaceholder(ICharIterator value)
        {
            using (value)
                _placeholderSegment.SetText(Font, value, UsePlaceholderColorFormatting);

            MarkDirty(DirtMarkType.Placeholder);
        }

        private void SetPlaceholderColor(Color value)
        {
            MarkDirty(ref _placeholderColor, value, DirtMarkType.PlaceholderColor);
        }

        private void SetCharLimit(int value)
        {
            if (_charLimit != value)
            {
                _charLimit = value;
                if (Value.Length > _charLimit)
                    Remove(_charLimit, Value.Length - _charLimit);
            }
        }

        private void SetIsMultiLined(bool value)
        {
            if (_isMultiLined != value)
            {
                _isMultiLined = value;
                UpdateGlyphs();
            }
        }

        private void SetIsObscured(bool value)
        {
            if (_isObscured != value)
            {
                _isObscured = value;
                UpdateGlyphs();
            }
        }

        private void SetObscureChar(char value)
        {
            if (_obscureChar != value)
            {
                _obscureChar = value;
                if (_isObscured)
                    UpdateGlyphs();
            }
        }
        #endregion
    }
}
