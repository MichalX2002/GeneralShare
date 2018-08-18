using System;

namespace GeneralShare.UI
{
    public static class DirtMarkTypeExtensions
    {
        public static bool HasFlagF(this DirtMarkType mark, DirtMarkType comparison)
        {
            return (mark & comparison) == comparison;
        }
    }

    [Flags]
    public enum DirtMarkType
    {
        // Transform
        Position = 1 << 0,
        Scale = 1 << 1,
        Rotation = 1 << 2,
        Origin = 1 << 3,

        // TextBox
        Font = 1 << 4,
        UseTextExpressions = 1 << 5,
        KeepTextExpressions = 1 << 6,
        ShadowOffset = 1 << 7,
        ShadowColor = 1 << 8,
        UseShadow = 1 << 9,
        ShadowMath = 1 << 10,
        BuildTextTree = 1 << 11,
        TextAlignment = 1 << 27,

        // ProgressBar
        BackgroundColor = 1 << 12,
        Bounds = 1 << 13,
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
        Transform = 1 << 22,
        Value = 1 << 23,
        ValueProcessed = 1 << 24,
        ObscureValue = 1 << 25,
        ObscureChar = 1 << 26,
        PivotPosition = 1 << 28
    }
}