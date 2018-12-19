using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UIDrawOrderComparer : IComparer<UITransform>
    {
        public static readonly UIDrawOrderComparer Instance;

        static UIDrawOrderComparer()
        {
            Instance = new UIDrawOrderComparer();
        }

        private UIDrawOrderComparer()
        {
        }

        public int Compare(UITransform x, UITransform y)
        {
            return x.DrawOrder.CompareTo(y.DrawOrder);
        }
    }
}
