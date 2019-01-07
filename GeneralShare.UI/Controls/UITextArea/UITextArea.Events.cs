using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;

namespace GeneralShare.UI
{
    public partial class UITextArea : UITextElement
    {
        #region Event Handlers
        private void UITextArea_OnKeyRepeat(Keys key, float timeDown)
        {
            if (timeDown > 0.5f)
                TryMoveCaret(key);

            // TODO: this will run at current framerate, it's better to make it delta time based,
            // we have an update function so that's relatively simple
        }

        private void UITextArea_OnKeyPress(Keys key)
        {
            TryMoveCaret(key);

            switch (key)
            {
                case Keys.V:
                    if (Input.CtrlDown && Input.HasClipboardText)
                    {
                        using (var iter = Input.ClipboardText.ToIterator())
                        {
                            for (int i = 0; i < iter.Length; i++)
                                if (InsertChar(Value.Length, iter.GetCharacter16(i)))
                                    break; // InsertChar() hit the char limit
                        }
                    }
                    break;
            }
        }

        private void UITextArea_OnTextInput(TextInputEventArgs e)
        {
            void AppendCharAfterCaret(char value)
            {
                InsertChar(Value.Length, value);
            }

            switch (e.Key)
            {
                case Keys.Tab:
                    for (int i = 0; i < 4; i++)
                        AppendCharAfterCaret(' ');
                    break;

                case Keys.Back:
                    if (Value.Length > 0)
                        Remove(Value.Length - 1, 1);
                    break;

                case Keys.Enter:
                    if (_isMultiLined)
                        AppendCharAfterCaret('\n');
                    break;

                default:
                    char c = e.Character;
                    if (char.IsLetter(c) ||
                        char.IsNumber(c) ||
                        char.IsSymbol(c) ||
                        char.IsWhiteSpace(c) ||
                        char.IsPunctuation(c))
                    {
                        AppendCharAfterCaret(c);
                    }
                    break;
            }
        }
        #endregion

        #region Helper Functions
        private void PauseCaretAnimation()
        {
            _repeatKeyOccuring = 0.666f;
        }

        /// <summary>
        /// Returns <see langword="true"/> if this operation would exceed the 
        /// current character limit, otherwise <see langword="false"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns>
        /// <see langword="true"/> if this operation would exceed the current limit,
        /// otherwise <see langword="false"/>.
        /// </returns>
        private bool InsertChar(int index, char value)
        {
            if (Value.Length >= CharacterLimit)
                return true;

            if (CharBlacklist.Contains(value))
                return false;

            PauseCaretAnimation();

            using (var iter = value.ToIterator())
                Insert(index, iter);

            return false;
        }

        private void TryMoveCaret(Keys key)
        {
            switch (key)
            {
                case Keys.Left:
                case Keys.Down:
                    //CaretIndex--;
                    break;

                case Keys.Right:
                case Keys.Up:
                    //CaretIndex++;
                    break;
            }
        }
        #endregion
    }
}