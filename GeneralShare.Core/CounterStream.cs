using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace GeneralShare.Core
{
    public class CounterStream : Stream
    {
        public const int MAX_AVERAGE_CAPACITY = 1000;
        public const int DEFAULT_AVERAGE_CAPACITY = 8;
        private const TaskContinuationOptions ASYNC_OPTIONS =
            TaskContinuationOptions.OnlyOnRanToCompletion |
            TaskContinuationOptions.ExecuteSynchronously;

        private const long PRECISION_MUL = 4;
        private const long TICK_THRESHOLD = 1000 / PRECISION_MUL;

        private Stream _stream;
        private bool _leaveOpen;

        private long __bytesRead;
        private long __bytesWritten;

        private int _avgBufferCapacity;
        private bool _calculateAverages;
        private int[] _reads;
        private int[] _writes;

        private long _lastRead = Environment.TickCount;
        private long _readTicks = 0;
        private int _readIndex = 0;

        private long _lastWrite = Environment.TickCount;
        private long _writeTicks = 0;
        private int _writeIndex = 0;

        public override bool CanSeek => _stream.CanSeek;
        public override bool CanRead => _stream.CanRead;
        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override bool CanTimeout => _stream.CanTimeout;
        public override int ReadTimeout { get => _stream.ReadTimeout; set => _stream.ReadTimeout = value; }
        public override int WriteTimeout { get => _stream.WriteTimeout; set => _stream.WriteTimeout = value; }
        
        public long BytesRead => __bytesRead;
        public long BytesWritten => __bytesWritten;

        public double AverageRead => _calculateAverages ? GetReadAvg() : -1;
        public double AverageWrite => _calculateAverages ? GetWriteAvg() : -1;

        public int AverageBufferCapacity
        {
            get => _avgBufferCapacity;
            set
            {
                if (value <= 0)
                    value = DEFAULT_AVERAGE_CAPACITY;
                if (value >= MAX_AVERAGE_CAPACITY)
                    value = MAX_AVERAGE_CAPACITY;

                if (_avgBufferCapacity != value)
                {
                    _avgBufferCapacity = value;
                    SetAverageBuffers();
                }
            }
        }

        public bool CalculateAverages
        {
            get => _calculateAverages;
            set
            {
                if (value && _reads == null)
                    SetAverageBuffers();
                _calculateAverages = value;
            }
        }

        public CounterStream(
            Stream stream, bool leaveOpen,
            bool calculateAverages, int averageBufferCapacity)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
            AverageBufferCapacity = averageBufferCapacity;
            CalculateAverages = calculateAverages;
        }

        public CounterStream(Stream stream, bool leaveOpen, int averageBufferCapacity) :
           this(stream, leaveOpen, averageBufferCapacity > 0, averageBufferCapacity)
        {
        }

        public CounterStream(Stream stream, int averageBufferCapacity) :
            this(stream, false, averageBufferCapacity)
        {
        }

        public CounterStream(Stream stream, bool leaveOpen) :
            this(stream, leaveOpen, false, DEFAULT_AVERAGE_CAPACITY)
        {
        }

        public CounterStream(Stream stream) :
            this(stream, false, DEFAULT_AVERAGE_CAPACITY)
        {
        }

        private void SetAverageBuffers()
        {
            void UpdateArray(ref int[] array)
            {
                var old = array;
                array = new int[_avgBufferCapacity];
                if (old != null)
                    Array.Copy(old, array, array.Length);
            }
            
            UpdateArray(ref _reads);
            UpdateArray(ref _writes);
        }

        private double GetReadAvg()
        {
            UpdateReadIndex();

            long sum = 0;
            double div = 0;
            for (int i = 0; i < _reads.Length; i++)
            {
                long r = _reads[i];
                if (r > 0)
                {
                    sum += r;
                    div++;
                }
            }
            return (sum / div) * PRECISION_MUL;
        }

        private double GetWriteAvg()
        {
            UpdateWriteIndex();

            long sum = 0;
            double div = 0;
            for (int i = 0; i < _writes.Length; i++)
            {
                long r = _writes[i];
                if (r > 0)
                {
                    sum += r;
                    div++;
                }
            }
            return (sum / div) * PRECISION_MUL;
        }

        private void UpdateReadIndex()
        {
            long now = Environment.TickCount;
            _readTicks += now - _lastRead;
            _lastRead = now;

            while (_readTicks > TICK_THRESHOLD)
            {
                _readTicks -= TICK_THRESHOLD;
                _readIndex++;

                if (_readIndex >= _reads.Length)
                    _readIndex = 0;

                _reads[_readIndex] = 0;
            }
        }

        private void UpdateWriteIndex()
        {
            long now = Environment.TickCount;
            _writeTicks += now - _lastWrite;
            _lastWrite = now;

            while (_writeTicks > TICK_THRESHOLD)
            {
                _writeTicks -= TICK_THRESHOLD;
                _writeIndex++;

                if (_writeIndex >= _reads.Length)
                    _writeIndex = 0;

                _writes[_writeIndex] = 0;
            }
        }

        private void AddBytesRead(int value)
        {
            __bytesRead += value;
            if (_calculateAverages)
            {
                UpdateReadIndex();
                _reads[_readIndex] += value;
            }
        }

        private void AddBytesWritten(int value)
        {
            __bytesWritten += value;
            if (_calculateAverages)
            {
                UpdateWriteIndex();
                _writes[_readIndex] += value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _stream.Read(buffer, offset, count);
            AddBytesRead(read);
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
            AddBytesWritten(count);
        }

        public override int ReadByte()
        {
            int value = _stream.ReadByte();
            if (value != -1)
                AddBytesRead(1);
            return value;
        }

        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
            AddBytesWritten(1);
        }

        public override Task<int> ReadAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.ReadAsync(buffer, offset, count, cancellationToken).ContinueWith((t) =>
            {
                AddBytesRead(t.Result);
                return t.Result;
            }, ASYNC_OPTIONS);
        }

        public override Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.WriteAsync(buffer, offset, count, cancellationToken).ContinueWith((t) =>
            {
                AddBytesWritten(count);
            }, ASYNC_OPTIONS);
        }

        public override IAsyncResult BeginRead(
            byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var result = _stream.BeginRead(buffer, offset, count, callback, state);
            return new AsyncResultWrapper(result, count);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            var result = asyncResult as AsyncResultWrapper;
            int read = _stream.EndRead(result.InnerResult);
            AddBytesRead(read);
            return read;
        }

        public override IAsyncResult BeginWrite(
            byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var result = _stream.BeginWrite(buffer, offset, count, callback, state);
            return new AsyncResultWrapper(result, count);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            var result = asyncResult as AsyncResultWrapper;
            _stream.EndWrite(result.InnerResult);
            AddBytesWritten(result.ByteCount);
        }

        public override Task CopyToAsync(
            Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _stream.FlushAsync(cancellationToken);
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override ObjRef CreateObjRef(Type requestedType)
        {
            return _stream.CreateObjRef(requestedType);
        }

        public override object InitializeLifetimeService()
        {
            return _stream.InitializeLifetimeService();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _stream.GetHashCode();
        }

        public override string ToString()
        {
            return _stream.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                {
                    _stream.Dispose();
                }
                _stream = null;
            }
        }

        class AsyncResultWrapper : IAsyncResult
        {
            public readonly IAsyncResult InnerResult;
            public readonly int ByteCount;

            public bool CompletedSynchronously => InnerResult.CompletedSynchronously;
            public WaitHandle AsyncWaitHandle => InnerResult.AsyncWaitHandle;
            public object AsyncState => InnerResult.AsyncState;
            public bool IsCompleted => InnerResult.IsCompleted;

            public AsyncResultWrapper(IAsyncResult result, int count)
            {
                InnerResult = result;
                ByteCount = count;
            }
        }
    }
}
