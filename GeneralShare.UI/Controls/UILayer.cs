using Microsoft.Xna.Framework;

namespace GeneralShare.UI
{
    public class UILayer : UIElement
    {
        private RectangleF _boundaries;
        private RectangleF _destination;

        public override RectangleF Boundaries => _boundaries;
        public RectangleF Destination { get => _destination; set => SetDestination(value); }

        public UILayer(UIManager manager) : base(manager)
        {
            IsDrawable = false;
        }

        private void SetDestination(RectangleF value)
        {
            MarkDirty(ref _destination, value, DirtMarkType.Destination);
        }

        protected override void Cleanup()
        {
            if (HasDirtMarks(DirtMarkType.Transform | DirtMarkType.Destination))
            {
                Vector3 pos = GlobalPosition;
                Vector2 scale = GlobalScale;

                _boundaries.X = pos.X + _destination.X - Origin.X * scale.X;
                _boundaries.Y = pos.Y + _destination.Y - Origin.Y * scale.Y;
                _boundaries.Width = Destination.Width * scale.X;
                _boundaries.Height = Destination.Height * scale.Y;

                MarkClean(DirtMarkType.Transform | DirtMarkType.Destination);
                InvokeMarkedDirty(DirtMarkType.Boundaries);
            }
        }
    }
}
