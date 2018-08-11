using System;

namespace GeneralShare
{
    public class EventBasedReporter<T> : IProgress<T>
    {
        public delegate void ProgressDelegate(T value);

        public event ProgressDelegate OnProgress;

        public void Report(T value)
        {
            OnProgress?.Invoke(value);
        }
    }
}
