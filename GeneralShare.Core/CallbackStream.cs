using System;
using System.IO;

namespace GeneralShare
{
    public sealed class CallbackStream : Stream
    {
        public delegate void OnReadDelegate(byte[] buffer, int offset, int count);
        public delegate void OnWriteDelegate(byte[] bufffer, int offset, int count);

        private readonly bool _leaveOpen;

        private readonly OnReadDelegate OnRead;
        private readonly OnWriteDelegate OnWrite;

        public Stream InnerStream { get; private set; }
        public override bool CanRead => InnerStream.CanRead;
        public override bool CanSeek => InnerStream.CanSeek;
        public override bool CanWrite => InnerStream.CanWrite;
        public override bool CanTimeout => InnerStream.CanTimeout;
        public override int ReadTimeout { get => InnerStream.ReadTimeout; set => InnerStream.ReadTimeout = value; }
        public override int WriteTimeout { get => InnerStream.WriteTimeout; set => InnerStream.WriteTimeout = value; }
        public override long Length => InnerStream.Length;
        public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

        public CallbackStream(
            Stream innerStream, OnReadDelegate onRead, OnWriteDelegate onWrite, bool leaveOpen)
        {
            InnerStream = innerStream;
            _leaveOpen = leaveOpen;

            OnRead = onRead;
            OnWrite = onWrite;
        }

        public override void Flush()
        {
            InnerStream.Flush();
        }

        public override void Close()
        {
            InnerStream.Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int length = InnerStream.Read(buffer, offset, count);
            OnRead?.Invoke(buffer, offset, length);
            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            InnerStream.Write(buffer, offset, count);
            OnWrite?.Invoke(buffer, offset, count);
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            InnerStream.SetLength(value);
        }

        public override object InitializeLifetimeService()
        {
            return InnerStream.InitializeLifetimeService();
        }

        public override bool Equals(object obj)
        {
            return InnerStream.Equals(obj);
        }

        public override string ToString()
        {
            return InnerStream.ToString();
        }

        public override int GetHashCode()
        {
            return InnerStream.GetHashCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (_leaveOpen == false)
                    InnerStream.Dispose();
        }
    }
}
