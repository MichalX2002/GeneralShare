
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeneralShare.UI
{
    public class UIAnchor : UITransform
    {
        private PivotPosition _pivot;
        private Vector3 _pivotOffset;

        public PivotPosition Pivot { get => _pivot; set => SetPivot(value); }
        public Vector3 Offset { get => _pivotOffset; set => SetPivotOffset(value); }

        public UIAnchor(UIManager manager) : base(manager)
        {
            _pivot = PivotPosition.Default;
        }

        private void SetPivot(PivotPosition value)
        {
            if (_pivot != value)
            {
                _pivot = value;
                UpdatePivotPosition();
            }
        }

        private void SetPivotOffset(Vector3 value)
        {
            if (_pivotOffset != value)
            {
                _pivotOffset = value;
                UpdatePivotPosition();
            }
        }

        internal override void OnViewportChanged(Viewport viewport)
        {
            ViewportChanged(viewport);
        }

        public override void ViewportChanged(Viewport viewport)
        {
            if (Pivot == PivotPosition.None)
                Position = Vector3.Zero;
            else
                UpdatePivotPosition();
        }

        private void UpdatePivotPosition()
        {
            Viewport view = Manager.Viewport;
            var pos = new Vector3(view.X + _pivotOffset.X, view.Y + _pivotOffset.Y, _pivotOffset.Z);

            switch (_pivot)
            {
                // Bottom
                case PivotPosition.BottomLeft:
                    pos.Y += view.Height;
                    break;

                case PivotPosition.BottomRight:
                    pos.X += view.Width;
                    pos.Y += view.Height;
                    break;

                case PivotPosition.Bottom:
                    pos.X += view.Width / 2;
                    pos.Y += view.Height;
                    break;

                // Center
                case PivotPosition.Left:
                    pos.Y += view.Height / 2;
                    break;

                case PivotPosition.Right:
                    pos.X += view.Width;
                    pos.Y += view.Height / 2;
                    break;

                case PivotPosition.Center:
                    pos.X += view.Width / 2;
                    pos.Y += view.Height / 2;
                    break;

                // Top
                case PivotPosition.TopLeft:
                    break;

                case PivotPosition.TopRight:
                    pos.X += view.Width;
                    break;

                case PivotPosition.Top:
                    pos.X += view.Width / 2;
                    break;

                default:
                    pos = Vector3.Zero;
                    break;
            }
            Position = pos;
        }
    }
}
