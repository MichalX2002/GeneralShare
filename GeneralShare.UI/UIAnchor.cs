
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
        }
       
        public UIAnchor() : base()
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

            Vector3 baseVec = new Vector3(rect.X + _pivotOffset.X, rect.Y + _pivotOffset.Y, _pivotOffset.Z);
            switch (_pivot)
            {
                // Bottom
                case PivotPosition.BottomLeft:
                    baseVec.Y += rect.Height;
                    break;

                case PivotPosition.BottomRight:
                    baseVec.X += rect.Width;
                    baseVec.Y += rect.Height;
                    break;

                case PivotPosition.Bottom:
                    baseVec.X += rect.Width / 2;
                    baseVec.Y += rect.Height;
                    break;

                // Center
                case PivotPosition.Left:
                    baseVec.Y += rect.Height / 2;
                    break;

                case PivotPosition.Right:
                    baseVec.X += rect.Width;
                    baseVec.Y += rect.Height / 2;
                    break;

                case PivotPosition.Center:
                    baseVec.X += rect.Width / 2;
                    baseVec.Y += rect.Height / 2;
                    break;

                // Top
                case PivotPosition.TopLeft:
                    break;

                case PivotPosition.TopRight:
                    baseVec.X += rect.Width;
                    break;

                case PivotPosition.Top:
                    baseVec.X += rect.Width / 2;
                    break;
            }
            Position = baseVec;
        }
    }
}
