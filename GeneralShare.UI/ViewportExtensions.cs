
using Microsoft.Xna.Framework.Graphics;

namespace GeneralShare.UI
{
    public static class ViewportExtensions
    {
        public static bool EqualsTo(this Viewport src, Viewport viewport)
        {
            return src.Bounds == viewport.Bounds 
                && src.MaxDepth == viewport.MaxDepth 
                && src.MinDepth == viewport.MinDepth 
                && src.TitleSafeArea == viewport.TitleSafeArea;
        }
    }
}
