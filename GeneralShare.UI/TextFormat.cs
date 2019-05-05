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
        public static ICharIterator ColorFormat(
            ICharIterator input, BitmapFont font, bool keepSequences, byte baseAlpha, IReferenceList<Color?> output)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (font == null) throw new ArgumentNullException(nameof(font));

            if (input.Length == 0)
                return input;

            var builder = StringBuilderPool.Rent(input.Length);
            ColorFormat(input, builder, font, keepSequences, baseAlpha, output);
            var iterator = CharIteratorPool.Rent(builder, 0, builder.Length);
            StringBuilderPool.Return(builder);
            return iterator;
        }

        public static void ColorFormat(
            ICharIterator input, StringBuilder textOutput,
            BitmapFont font, bool keepSequences, byte baseAlpha, ICollection<Color?> colorOutput)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (textOutput == null) throw new ArgumentNullException(nameof(textOutput));
            if (font == null) throw new ArgumentNullException(nameof(font));

            if (input.Length == 0)
                return;

            Span<byte> colorBuffer = stackalloc byte[4];
            Span<char> charBuffer = stackalloc char[2];
            Span<char> sequenceBuffer = stackalloc char[16];
            Color? currentColor = null;

            bool inSequence = false;
            for (int i = 0; i < input.Length; i++)
            {
                void AddAtLoopIndex(Span<char> buffer, ref int index)
                {
                    if (index >= input.Length)
                        return;

                    int utf32 = input.GetCharacter32(ref index);
                    int count = ConvertFromUtf32(utf32, buffer);
                    bool addColor = colorOutput != null && font.ContainsCharacterRegion(utf32);

                    for (int utfIndex = 0; utfIndex < count; utfIndex++)
                    {
                        if (addColor)
                            colorOutput.Add(currentColor);
                        textOutput.Append(buffer[utfIndex]);
                    }
                }

                if (input.GetCharacter16(i) != '§')
                {
                    AddAtLoopIndex(charBuffer, ref i);
                    continue;
                }

                if (inSequence)
                {
                    if (keepSequences)
                        AddAtLoopIndex(charBuffer, ref i);
                    
                    currentColor = null;
                    inSequence = false;
                }
                else
                {
                    i++;
                    if (i < input.Length)
                    {
                        if (input.GetCharacter16(i) == '[')
                        {
                            var sequence = GetSequence(input, i + 1, sequenceBuffer);
                            if (sequence.Tail > 0)
                            {
                                if (colorOutput != null)
                                {
                                    currentColor = sequenceBuffer[0] == '#' 
                                        ? GetHexColor(colorBuffer, sequenceBuffer, sequence.Length, baseAlpha) 
                                        : GetRgba(colorBuffer, sequenceBuffer, sequence.Length, baseAlpha);
                                }

                                if (!keepSequences)
                                    i = sequence.Tail + 1;
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

        /// <summary>
        /// Non-allocating version of <see cref="char.ConvertFromUtf32(int)"/> as
        /// this one uses a <see cref="Span{T}"/> instead of returning a <see cref="string"/>.
        /// </summary>
        /// <param name="utf32">The Unicode character to decompose into <see cref="char"/>'s.</param>
        /// <param name="buffer">A <see cref="char"/> buffer with at least 2 capacity.</param>
        /// <returns>The amount of resulting <see cref="char"/>'s in the buffer. </returns>
        public static int ConvertFromUtf32(int utf32, Span<char> buffer)
        {
            if (buffer.Length < 2)
                throw new ArgumentException(nameof(buffer), "Insufficient capacity.");

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

        // TODO: Think of adding a font selecting char as first char in a sequence
        // and use a specialized font collection instead of only a BitmapFont property (mostly in UIText).
        // Switching fonts while building will need some work, but applying the color format should be the same.
        private static CharSequence GetSequence(
            ICharIterator input, int start, Span<char> output)
        {
            int tail = start;
            while (tail < input.Length)
            {
                // don't bother going beyond text length or 15 chars 
                // ("255,255,255,255" is 15 chars, hex is smaller than that)
                tail++;
                if (tail == input.Length)
                    return CharSequence.Invalid;

                int length = tail - start;
                char tailChar = input.GetCharacter16(tail);
                if (tailChar == ']' || tailChar == '§')
                {
                    // we reached the end and know the length of the sequence
                    // we can't really use
                    for (int i = 0; i < length; i++)
                        output[i] = input.GetCharacter16(start + i);
                    
                    return new CharSequence(tail, length);
                }

                if (length > 15)
                    return CharSequence.Invalid;
            }
            return CharSequence.Invalid;
        }

        public static Color GetHexColor(Span<byte> buffer, Span<char> sequence, int count, byte baseAlpha)
        {
            buffer.Fill(0);
            buffer[3] = baseAlpha;

            int offset = sequence[0] == '#' ? 1 : 0;
            StringHelper.HexToByteSpan(sequence.Slice(offset, count - offset), buffer);
            return new Color(buffer[0], buffer[1], buffer[2], buffer[3]);
        }

        private static byte FastParse(Span<char> value, int offset, int count)
        {
            int tmp = 0;
            int length = Math.Min(count + offset, value.Length);
            for (int i = offset; i < length; i++)
                tmp = tmp * 10 + (value[i] - '0');
            if (tmp > 255)
                tmp = 255;
            return (byte)tmp;
        }

        public static Color GetRgba(Span<byte> buffer, Span<char> sequence, int count, byte baseAlpha)
        {
            buffer.Fill(0);
            buffer[3] = baseAlpha;

            int delimeterCount = 0;
            int lastOffset = 0;

            for (int i = 0; i < count; i++)
            {
                if(sequence[i] == ',')
                {
                    buffer[delimeterCount] = FastParse(sequence, lastOffset, i - lastOffset);
                    lastOffset = i + 1;

                    delimeterCount++;
                    if (delimeterCount > 3)
                        break;
                }
            }
            buffer[delimeterCount] = FastParse(sequence, lastOffset, count - lastOffset);

            if(delimeterCount == 0)
            {
                buffer[1] = buffer[0];
                buffer[2] = buffer[0];
            }

            return new Color(buffer[0], buffer[1], buffer[2], buffer[3]);
        }

        readonly struct CharSequence
        {
            public static readonly CharSequence Invalid = new CharSequence(-1, 0);

            public int Tail { get; }
            public int Length { get; }

            public CharSequence(int tail, int length)
            {
                Tail = tail;
                Length = length;
            }
        }
    }
}
