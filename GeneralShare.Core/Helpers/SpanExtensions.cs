﻿using System;
using System.Runtime.InteropServices;

namespace GeneralShare
{
    public static class SpanExtensions
    {
        public static Span<byte> AsBytes<T>(this Span<T> span) where T : struct
        {
            return MemoryMarshal.AsBytes(span);
        }

        public static ReadOnlySpan<byte> AsBytes<T>(this ReadOnlySpan<T> span) where T : struct
        {
            return MemoryMarshal.AsBytes(span);
        }
    }
}
