using System;
using System.Diagnostics;
using System.Runtime;

namespace GeneralShare
{
    public static class DebugUtils
    {
        private static long _lastMemoryUsage;
        private static TimeSpan _lastProcessRefresh;

        public static readonly Process CurrentProcess;
        public static readonly DateTime StartTime;

        public static TimeSpan TimeSinceStart => DateTime.Now - StartTime;
        public static bool DebuggerAttached => Debugger.IsAttached;
        public static long TotalMemoryUsage => GetTotalMemoryUsage();
        public static long ManagedMemoryUsage => GC.GetTotalMemory(false);

        static DebugUtils()
        {
            CurrentProcess = Process.GetCurrentProcess();
            _lastProcessRefresh = DateTime.Now.TimeOfDay;
            StartTime = CurrentProcess.StartTime;
        }

        private static long GetTotalMemoryUsage()
        {
            RefreshCurrentProcess();
            return _lastMemoryUsage;
        }
        
        private static void RefreshCurrentProcess()
        {
            var now = DateTime.Now.TimeOfDay;
            if (now.TotalSeconds - _lastProcessRefresh.TotalSeconds > 1)
            {
                CurrentProcess.Refresh();
                _lastMemoryUsage = CurrentProcess.PrivateMemorySize64;
                _lastProcessRefresh = now;
            }
        }

        public static void CollectGarbage()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        }

        public static void CollectLargeObjectHeap()
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            CollectGarbage();
        }
    }
}
