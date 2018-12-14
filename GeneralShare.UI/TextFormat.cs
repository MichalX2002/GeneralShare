using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;

namespace GeneralShare.UI
{
    public static partial class TextFormat
    {
        [ThreadStatic]
        private static StringBuilder __sequenceBuilder;
        
        private static StringBuilder SequenceBuilder
        {
            get
            {
                if (__sequenceBuilder == null)
                    __sequenceBuilder = new StringBuilder();
                return __sequenceBuilder;
            }
        }

        public static ICharIterator ColorFormat(
            ICharIterator input, Color baseColor, BitmapFont font, bool keepSequences, IReferenceList<Color> output)
        {
            var builder = CharIteratorPool.RentBuilder(input.Count);
            ColorFormat(input, builder, baseColor, font, keepSequences, output);
            var iterator = CharIteratorPool.Rent(builder, 0, builder.Length);
            CharIteratorPool.ReturnBuilder(builder);
            return iterator;
        }

        public static int ConvertFromUtf32(int utf32, Span<char> buffer)
        {
            if (utf32 < 0 || utf32 > 0x10ffff || (utf32 >= 0x00d800 && utf32 <= 0x00dfff))
                return 0;

            if (utf32 < 0x10000)
            {
                buffer[0] = (char)utf32;
                return 1;
            }

            utf32 -= 0x10000;
            buffer[0] = (char)((utf32 / 0x400) + '\ud800');
            buffer[1] = (char)((utf32 % 0x400) + '\udc00');
            return 2;
        }

        public static void ColorFormat(
            ICharIterator input, StringBuilder textOutput,
            Color baseColor, BitmapFont font,
            bool keepSequences, ICollection<Color> colorOutput)
        {
            Span<byte> colorBuffer = stackalloc byte[4];
            Span<char> charBuffer = stackalloc char[2];
            StringBuilder seqBuilder = SequenceBuilder;
            Color currentColor = baseColor;

            bool inSequence = false;
            int inputLength = input.Count;
            for (int i = 0; i < inputLength; i++)
            {
                void AddAtLoopIndex(Span<char> buffer, ref int index)
                {
                    int utf32 = input.GetCharacter(ref index);
                    int count = ConvertFromUtf32(utf32, buffer);
                    bool addColor = colorOutput != null && font.ContainsCharacterRegion(utf32);

                    for (int charIndex = 0; charIndex < count; charIndex++)
                    {
                        if (addColor)
                            colorOutput.Add(currentColor);
                        textOutput.Append(buffer[charIndex]);
                    }
                }

                if (input.GetCharacter(ref i) != '§')
                {
                    AddAtLoopIndex(charBuffer, ref i);
                    continue;
                }

                if (inSequence)
                {
                    if (keepSequences)
                        AddAtLoopIndex(charBuffer, ref i);
                    
                    currentColor = baseColor;
                    inSequence = false;
                }
                else
                {
                    i++;
                    if (i < input.Count)
                    {
                        if (input.GetCharacter(ref i) == '[')
                        {
                            seqBuilder.Clear();
                            int tailOffset = GetSequence(input, i + 1, seqBuilder, charBuffer, out var seq);
                            if (tailOffset > 0)
                            {
                                if (colorOutput != null)
                                {
                                    currentColor = seq[0] == '#' ?
                                        GetHexColor(colorBuffer, seq) : GetRgba(colorBuffer, seq);
                                }

                                if (keepSequences == false)
                                    i = tailOffset + 1;
                                else
                                {
                                    colorOutput.Add(currentColor);
                                    textOutput.Append('§');
                                }

                                inSequence = true;
                            }
                        }
                    }
                    AddAtLoopIndex(charBuffer, ref i);
                }
            }
        }

        // Think of adding a font selecting char as first char in a sequence
        // and use a font collection instead of BitmapFont (in TextBox).
        // Switching fonts while building will need some work,
        // but applying the color format should be the same.
        private static int GetSequence(
            ICharIterator input, int start,
            StringBuilder seqBuilder, Span<char> buffer, out StringBuilder sequence)
        {
            int tail = start;
            while (tail < input.Count)
            {
                tail++;

                // Don't bother going beyond text length or 15 chars 
                // ("255,255,255,255" is 15 chars)
                if (tail == input.Count)
                {
                    sequence = null;
                    return 0;
                }

                if (input.GetCharacter(ref tail) == ']')
                {
                    for (int i = start; i < tail; i++)
                    {
                        int utf32 = input.GetCharacter(ref i);
                        int count = ConvertFromUtf32(utf32, buffer);
                        for (int charIndex = 0; charIndex < count; charIndex++)
                            seqBuilder.Append(buffer[charIndex]);
                    }
                    sequence = seqBuilder;
                    return tail;
                }

                if (tail - start > 15)
                {
                    sequence = null;
                    return 0;
                }
            }

            sequence = null;
            return -1;
        }

        public static Color GetHexColor(Span<byte> buffer, StringBuilder seq)
        {
            buffer[3] = 255;
            int offset = seq[0] == '#' ? 1 : 0;
            StringHelper.HexToByteArray(seq, offset, buffer);
            return new Color(buffer[0], buffer[1], buffer[2], buffer[3]);
        }

        private static byte FastParse(StringBuilder value, int offset, int count)
        {
            int tmp = 0;
            int length = count + offset;
            for (int i = offset; i < length; i++)
                tmp = tmp * 10 + (value[i] - '0');
            if (tmp > 255)
                tmp = 255;
            return (byte)tmp;
        }

        public static Color GetRgba(Span<byte> buffer, StringBuilder seq)
        {
            buffer[3] = 255;
            int itemCount = 0;
            int lastOffset = 0;

            for (int i = 0; i < seq.Length; i++)
            {
                if(seq[i] == ',')
                {
                    buffer[itemCount] = FastParse(seq, lastOffset, i - lastOffset);
                    lastOffset = i + 1;

                    itemCount++;
                    if (itemCount == 4)
                        break;
                }
            }
            buffer[itemCount] = FastParse(seq, lastOffset, seq.Length - lastOffset);

            return new Color(buffer[0], buffer[1], buffer[2], buffer[3]);
        }
    }
}
