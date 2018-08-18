using System;

namespace GeneralShare.UI
{
    [Flags]
    public enum PivotPosition
    {
        TopLeft = 1 << 0,
        Top = 1 << 1,
        TopRight = 1 << 2,

        Left = 1 << 3,
        Center = 1 << 4,
        Right = 1 << 5,

        BottomLeft = 1 << 6,
        Bottom = 1 << 7,
        BottomRight = 1 << 8,

        None = 1 << 9,
        Default = TopLeft
    }
}
