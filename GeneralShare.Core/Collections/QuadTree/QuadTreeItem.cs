using Microsoft.Xna.Framework;

namespace GeneralShare.Collections
{
    public readonly struct QuadTreeItem<T>
    {
        public readonly RectangleF Bounds;
        public readonly T Value;

        public QuadTreeItem(RectangleF bounds, T value)
        {
            Bounds = bounds;
            Value = value;
        }
    }
}