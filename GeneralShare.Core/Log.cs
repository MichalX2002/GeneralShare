using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace GeneralShare
{
    public static class Log
    {
        private static object _syncRoot = new object();
        private static StreamWriter _logWriter;

        public static bool IsOpened { get; private set; }

        public static void Open(Stream stream)
        {
            lock (_syncRoot)
            {
                if (stream == null)
                    throw new ArgumentNullException(nameof(stream));

                if(IsOpened)
                    throw new InvalidOperationException("The log is already open.");
                
                _logWriter = new StreamWriter(stream);
                _logWriter.AutoFlush = true;
                IsOpened = true;
            }
        }

        public static void Open(string fileName, FileMode fileMode = FileMode.CreateNew)
        {
            Open(new FileStream(fileName, fileMode));
        }

        public static void Close()
        {
            lock (_syncRoot)
            {
                try
                {
                    _logWriter?.Dispose();
                }
                finally
                {
                    _logWriter = null;
                    IsOpened = false;
                }
            }
        }

        public static void LineBreak()
        {
            WriteLine(Environment.NewLine);
        }

        public static void Error(object obj, bool showPrefix = true)
        {
            Error(obj.ToString(), showPrefix);
        }

        public static void Error(Exception exception, bool showPrefix = true)
        {
            Error(exception.ToString(), showPrefix);
        }

        public static void Error(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, "ERROR");
        }

        public static void Warning(object obj, bool showPrefix = true)
        {
            Warning(obj.ToString(), showPrefix);
        }
        
        public static void Warning(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, "WARN");
        }

        public static void Info(object obj, bool showPrefix = true)
        {
            Info(obj == null ? "null" : obj.ToString(), showPrefix);
        }

        public static void Info(string message, bool showPrefix = true)
        {
            BaseLog(message, showPrefix, true, Thread.CurrentThread, null);
        }

        public static void Debug(object obj, bool showPrefix = true)
        {
            if (DebugUtils.IsTracing || DebugUtils.IsDebuggerAttached) // check here to prevent a string allocation
                Debug(obj.ToString(), showPrefix);
        }

        public static void Debug(string message, bool showPrefix = true)
        {
            if (DebugUtils.IsTracing || DebugUtils.IsDebuggerAttached)
                BaseLog(message, showPrefix, true, Thread.CurrentThread, "DEBUG");
        }

        public static void Trace(object obj, bool showPrefix = true)
        {
            if(DebugUtils.IsTracing) // check here to prevent a string allocation
                Trace(obj.ToString(), showPrefix);
        }

        public static void Trace(string message, bool showPrefix = true)
        {
            if (DebugUtils.IsTracing)
                BaseLog(message, showPrefix, true, Thread.CurrentThread, "TRACE");
        }

        private static void BaseLog(string message, bool showPrefix, bool showTime, Thread thread, string type)
        {
            string prefix = null;
            if (IsOpened && showPrefix && (showTime || thread != null || !string.IsNullOrWhiteSpace(type)))
            {
                string time = showTime ? GetTimeSinceStart() : null;
                string typePart = string.IsNullOrWhiteSpace(type) ? null : type;
                string threadPart = thread != null && !string.IsNullOrWhiteSpace(thread.Name) ?
                    '\'' + thread.Name + '\'' : null;

                prefix = "[" + Join(" ", time, threadPart, typePart) + "] ";
            }
            WriteLine(prefix + message);
        }

        private static string Join(string separator, params string[] values)
        {
            int length = 0;
            foreach(var str in values)
            {
                if (str == null)
                    continue;

                if (length > 0)
                    length += separator.Length;
                length += str.Length;
            }

            var builder = new StringBuilder(length);
            for (int i = 0; i < values.Length; i++)
            {
                string str = values[i];
                if (str == null || str.Length == 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(separator);
                builder.Append(str);
            }

            return builder.ToString();
        }

        public static string GetTimeSinceStart()
        {
            return DebugUtils.TimeSinceStart.ToHMS();
        }

        [DebuggerHidden]
        private static void AssertOpen()
        {
            if (!IsOpened)
                throw new InvalidOperationException("The log is not open.");
        }

        private static void WriteLine(string value)
        {
            lock (_syncRoot)
            {
                AssertOpen();

                _logWriter.WriteLine(value);
                Console.WriteLine(value);
            }
        }
    }
}
