using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UIDrawOrderComparer : IComparer<UITransform>
    {
        public static readonly UIDrawOrderComparer Instance = new UIDrawOrderComparer();
        
        private UIDrawOrderComparer()
        {
        }

        public int Compare(UITransform x, UITransform y)
        {
            int dx = x.DrawOrder;
            if (x.Container != null)
                dx += x.Container.DrawOrder;

            int dy = y.DrawOrder;
            if (y.Container != null)
                dy += y.Container.DrawOrder;

            return dx.CompareTo(dy);
        }
    }
}
