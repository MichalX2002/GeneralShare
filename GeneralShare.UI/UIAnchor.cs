
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeneralShare.UI
{
    public class UIAnchor : UITransform
    {
        private PivotPosition _pivot;
        private Vector3 _pivotOffset;
        private Viewport _viewport;

        public PivotPosition Pivot { get => _pivot; set => SetPivot(value); }
        public Vector3 PivotOffset { get => _pivotOffset; set => SetPivotOffset(value); }

        public UIAnchor(UIManager manager) : base(manager)
        {
            _pivot = PivotPosition.Default;
        }
       
        public UIAnchor() : this(null)
        {
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

        public override void ViewportChanged(in Viewport viewport)
        {
            _viewport = viewport;
            UpdatePivotPosition();
        }

        private void UpdatePivotPosition()
        {
            Rectangle rect = Manager != null ? Manager._lastViewport.Bounds : _viewport.Bounds;
            Vector3 basePos = new Vector3(rect.X + _pivotOffset.X, rect.Y + _pivotOffset.Y, _pivotOffset.Z);
            switch (_pivot)
            {
                // Bottom
                case PivotPosition.BottomLeft:
                    basePos.Y += rect.Height;
                    break;

                case PivotPosition.BottomRight:
                    basePos.X += rect.Width;
                    basePos.Y += rect.Height;
                    break;

                case PivotPosition.Bottom:
                    basePos.X += rect.Width / 2;
                    basePos.Y += rect.Height;
                    break;

                // Center
                case PivotPosition.Left:
                    basePos.Y += rect.Height / 2;
                    break;

                case PivotPosition.Right:
                    basePos.X += rect.Width;
                    basePos.Y += rect.Height / 2;
                    break;

                case PivotPosition.Center:
                    basePos.X += rect.Width / 2;
                    basePos.Y += rect.Height / 2;
                    break;

                // Top
                case PivotPosition.TopLeft:
                    break;

                case PivotPosition.TopRight:
                    basePos.X += rect.Width;
                    break;

                case PivotPosition.Top:
                    basePos.X += rect.Width / 2;
                    break;

                default:
                    basePos = Vector3.Zero;
                    break;
            }
            Position = basePos;
            InvokeMarkedDirty(DirtMarkType.PivotPosition);
        }
    }
}
