using System.Collections.Generic;

namespace GeneralShare.UI
{
    public struct UIDrawOrderComparer : IComparer<UITransform>
    {
        public int Compare(UITransform x, UITransform y)
        {
            return x.DrawOrder.CompareTo(y.DrawOrder);
        }
    }
}
