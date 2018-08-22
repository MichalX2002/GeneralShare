using MonoGame.Extended;

namespace GeneralShare.UI
{
    public class UICollisionLayer : UIElement
    {
        private RectangleF _boundaries;
        private RectangleF _bounds;

        public override RectangleF Boundaries => _boundaries;
        public RectangleF Bounds { get => _bounds; set => SetBounds(value); }

        public UICollisionLayer(UIManager manager) : base(manager)
        {
            MarkedDirty += UILayer_MarkedDirty;
        }

        private void SetBounds(RectangleF value)
        {
            MarkDirtyE(ref _bounds, value, DirtMarkType.Bounds);
        }

        private void UILayer_MarkedDirty(DirtMarkType type)
        {
            if (type.HasFlags(DirtMarkType.Transform, DirtMarkType.Bounds))
            {
                float x = Position.X + _bounds.X - Origin.X * Scale.X;
                float y = Position.Y + _bounds.Y - Origin.Y * Scale.Y;
                float w = Bounds.Width * Scale.X;
                float h = Bounds.Height * Scale.Y;
                _boundaries = new RectangleF(x, y, w, h);

                InvokeMarkedDirty(DirtMarkType.Boundaries);
            }
        }
    }
}
