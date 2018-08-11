using System.Collections.Generic;

namespace GeneralShare.UI
{
    public struct UIDepthComparer : IComparer<UIElement>
    {
        public static readonly UIDepthComparer Instance;

        public int Compare(UIElement x, UIElement y)
        {
            if (x.Z == y.Z)
                return 0;

            if (x.Z < y.Z)
                return 1;

            return -1;
        }
    }
}
