using MonoGame.Extended;

namespace GeneralShare.UI
{
    public class UICollisionLayer : UIElement
    {
        private RectangleF _boundaries;
        private RectangleF _destination;

        public override RectangleF Boundaries => _boundaries;
        public RectangleF Destination { get => _destination; set => SetDestination(value); }

        public UICollisionLayer(UIManager manager) : base(manager)
        {
            MarkedDirty += UILayer_MarkedDirty;
        }

        private void SetDestination(RectangleF value)
        {
            MarkDirtyE(ref _destination, value, DirtMarkType.Destination);
        }

        private void UILayer_MarkedDirty(DirtMarkType type)
        {
            if (type.HasAnyFlag(DirtMarkType.Transform, DirtMarkType.Destination))
            {
                float x = Position.X + _destination.X - Origin.X * Scale.X;
                float y = Position.Y + _destination.Y - Origin.Y * Scale.Y;
                float w = Destination.Width * Scale.X;
                float h = Destination.Height * Scale.Y;
                _boundaries = new RectangleF(x, y, w, h);

                InvokeMarkedDirty(DirtMarkType.Boundaries);
            }
        }
    }
}
