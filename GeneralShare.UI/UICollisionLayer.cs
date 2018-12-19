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
            IsDrawable = false;
        }

        private void SetDestination(RectangleF value)
        {
            MarkDirty(ref _destination, value, DirtMarkType.Destination);
        }

        protected override void OnMarkedDirty(DirtMarkType type)
        {
            if (type.HasAnyFlag(DirtMarkType.Transform, DirtMarkType.Destination))
            {
                var globalPos = GlobalPosition;
                float x = globalPos.X + _destination.X - Origin.X * Scale.X;
                float y = globalPos.Y + _destination.Y - Origin.Y * Scale.Y;
                float w = Destination.Width * Scale.X;
                float h = Destination.Height * Scale.Y;
                _boundaries = new RectangleF(x, y, w, h);

                MarkClean(DirtMarkType.Transform | DirtMarkType.Destination);
                InvokeMarkedDirty(DirtMarkType.Boundaries);
            }
        }
    }
}
