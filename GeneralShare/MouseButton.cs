using System;

namespace GeneralShare
{
    [Flags]
    public enum MouseButton
    {
        Left = 1,
        Right = 2,
        Middle = 4,
        XButton1 = 8,
        XButton2 = 16,
        All = 31,
        None = 32,
    }
}
