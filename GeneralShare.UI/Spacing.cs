using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace GeneralShare.UI
{
    [Serializable]
    public struct Spacing : IEquatable<Spacing>
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;

        public Spacing(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public bool Equals(Spacing other)
        {
            return other.Left == Left
                && other.Right == Right
                && other.Top == Top
                && other.Bottom == Bottom;
        }

        

        public override string ToString()
        {
            return $"Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom}";
        }
    }
}
