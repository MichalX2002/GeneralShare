using System;

namespace GeneralShare.UI
{
    [Flags]
    public enum DirtMarkType
    {
        None = 0,

        // Transform
        Transform = 1 << 22,
        Position = 1 << 0 | Transform,
        Origin = 1 << 1 | Transform,
        Scale = 1 << 2 | Transform,
        Rotation = 1 << 3 | Transform,

        // TextBox
        Font = 1 << 4,
        UseTextExpressions = 1 << 5,
        KeepTextExpressions = 1 << 6,
        ShadowOffset = 1 << 7,
        ShadowColor = 1 << 8,
        UseShadow = 1 << 9,
        ShadowMath = 1 << 10,
        BuildQuadTree = 1 << 11,
        TextAlignment = 1 << 27,

        // ProgressBar
        BackgroundColor = 1 << 12,
        Range = 1 << 14,
        BarDirection = 1 << 15,
        BarThickness = 1 << 16,

        // InputBox
        CaretIndex = 1 << 17,
        CaretSize = 1 << 18,
        InputPrefix = 1 << 19,

        // General
        ClipRectangle = 1 << 20,
        Color = 1 << 21,
        Value = 1 << 23,
        ValueProcessed = 1 << 24,
        ObscureValue = 1 << 25,
        ObscureChar = 1 << 26,
        Boundaries = 1 << 29,
        Destination = 1 << 13,
        Enabled = 1 << 30,
        DrawOrder = 1 << 28
    }
}