using System;
using System.IO;
using MonoGame.Utilities.IO;

namespace GeneralShare
{
    public static class BinaryReaderHelper
    {
        public static int SkipString(this BinaryReader reader)
        {
            int length = reader.Read7BitEncodedInt();
            return Skip(reader, length);
        }

        public static int Skip(this BinaryReader reader, int count)
        {
            var baseStream = reader.BaseStream;
            int skipped = 0;

            if (baseStream.CanSeek)
            {
                long originalPosition = baseStream.Position;
                long diff = baseStream.Seek(count, SeekOrigin.Current) - originalPosition;
                skipped = (int)diff;
            }
            else
            {
                byte[] block = RecyclableMemoryManager.Instance.GetBlock();
                try
                {
                    int read;
                    while (
                        count > 0 &&
                        (read = baseStream.Read(block, 0, Math.Min(block.Length, count))) > 0)
                    {
                        count -= read;
                        skipped += read;
                    }
                }
                finally
                {
                    RecyclableMemoryManager.Instance.ReturnBlock(block, null);
                }
            }
            return skipped;
        }

        private static int Read7BitEncodedInt(this BinaryReader reader)
        {
            // Read out an Int32 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                // Check for a corrupted stream. Read a max of 5 bytes.
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    throw new InvalidDataException("Invalid 7-bit encoded 32-bit integer.");

                // ReadByte handles end of stream cases for us.
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }
    }
}