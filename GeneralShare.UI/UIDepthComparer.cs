using System.Collections.Generic;

namespace GeneralShare.UI
{
    public struct UIDepthComparer : IComparer<UITransform>
    {
        public int Compare(UITransform x, UITransform y)
        {
            if (x.Z == y.Z)
                return 0;

            if (x.Z < y.Z)
                return 1;

            return -1;
        }
    }
}
