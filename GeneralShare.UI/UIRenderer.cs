using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GeneralShare.UI
{
    public class UIRenderer
    {
        public UIManager Manager { get; }

        public UIRenderer(UIManager manager)
        {
            Manager = manager;
        }

        public virtual void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            bool isBatching = false;
            SamplingMode lastSampling = SamplingMode.LinearClamp;

            var transforms = Manager.GetSortedTransformList();
            for (int i = 0; i < transforms.Count; i++)
            {
                var transform = transforms[i];
                if (!transform.IsActive || !transform.IsDrawable)
                    continue;

                if (lastSampling != transform.PreferredSampling)
                {
                    if (isBatching)
                    {
                        spriteBatch.End();
                        isBatching = false;
                    }
                    lastSampling = transform.PreferredSampling;
                }

                if (!isBatching)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, GetSamplerState(lastSampling));
                    isBatching = true;
                }

                transform.AssertPure();
                transform.Draw(time, spriteBatch);
            }
            spriteBatch.End();
        }

        public SamplerState GetSamplerState(SamplingMode samplingMode)
        {
            switch (samplingMode)
            {
                case SamplingMode.AnisotropicClamp: return SamplerState.AnisotropicClamp;
                case SamplingMode.AnisotropicWrap: return SamplerState.AnisotropicWrap;
                case SamplingMode.LinearClamp: return SamplerState.LinearClamp;
                case SamplingMode.LinearWrap: return SamplerState.LinearWrap;
                case SamplingMode.PointClamp: return SamplerState.PointClamp;
                case SamplingMode.PointWrap: return SamplerState.PointWrap;

                default:
                    return SamplerState.LinearWrap;
            }
        }
    }
}
