using System.Collections.Generic;

namespace GeneralShare.UI
{
    public struct UIDepthComparer : IComparer<UITransform>
    {
        public int Compare(UITransform x, UITransform y)
        {
            return x.Z.CompareTo(y.Z);
        }
    }
}
