using MonoGame.Extended.BitmapFonts;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public abstract partial class UITextElement
    {
        public void Insert(int index, ICharIterator chars)
        {
            _segment.Insert(index, chars, AllowTextColorFormatting);
            MarkDirty(DirtMarkType.Value);
        }

        public void Append(ICharIterator chars)
        {
            Insert(_segment.CurrentText.Length, chars);
        }

        public void Remove(int index, int count)
        {
            _segment.Remove(index, count);
            MarkDirty(DirtMarkType.Value);
        }

        public void Remove(IEnumerable<char> chars)
        {
            _segment.Remove(chars);
            MarkDirty(DirtMarkType.Value);
        }

        public void Remove(char value)
        {
            _segment.Remove(value);
            MarkDirty(DirtMarkType.Value);
        }
    }
}
