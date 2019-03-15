using System;

namespace GeneralShare.UI
{
    [Flags]
    public enum DirtMarkType
    {
        // TODO: re-order values to cleanup this list
        // 1<< 5 is free
        // 1<< 6 is free
        // 1<< 8 is free
        // 1<< 18 is free
        // 1<< 19 is free
        // 1<< 22 is free
        // 1<< 24 is free

        None = 0,

        // Transform
        Position = 1 << 0,
        Rotation = 1 << 1,
        Origin = 1 << 2,
        Scale = 1 << 3,
        Transform = Position | Rotation | Origin | Scale,

        // Text
        Font = 1 << 4,
        ShadowSpacing = 1 << 7,
        Shadowed = 1 << 9,
        Placeholder = 1 << 10,
        PlaceholderColor = 1 << 17,
        BuildQuadTree = 1 << 11,
        TextAlignment = 1 << 27,

        // ProgressBar
        BackgroundColor = 1 << 12,
        Range = 1 << 14,
        BarDirection = 1 << 15,
        BarThickness = 1 << 16,

        // General
        Destination = 1 << 13,
        ClipRectangle = 1 << 20,
        Color = 1 << 21,
        Value = 1 << 23,
        ClipRect = 1 << 26,
        DrawOrder = 1 << 28,
        Boundaries = 1 << 29,
        Enabled = 1 << 30,
        Texture = 1 << 25
    }
}