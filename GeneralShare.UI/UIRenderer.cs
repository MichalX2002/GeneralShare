using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GeneralShare.UI
{
    public class UIRenderer
    {
        protected Dictionary<SamplingMode, List<UIElement>> _elementQueue;

        public UIManager Manager { get; }

        public UIRenderer(UIManager manager)
        {
            Manager = manager;

            _elementQueue = new Dictionary<SamplingMode, List<UIElement>>();
        }

        public virtual void Draw(GameTime time, SpriteBatch spriteBatch)
        {
            FillQueue();
            DrawQueue(time, spriteBatch);
        }

        protected virtual void FillQueue()
        {
            lock (Manager.SyncRoot)
            {
                var transforms = Manager.GetSortedTransforms();
                for (int i = 0, count = transforms.Count; i < count; i++)
                {
                    UITransform transform = transforms[i];
                    if (transform.IsActive == false || transform is UIContainer)
                        continue;

                    if (transform is UIElement element)
                    {
                        if (_elementQueue.TryGetValue(element.PreferredSamplingMode, out var list) == false)
                        {
                            list = new List<UIElement>();
                            _elementQueue.Add(element.PreferredSamplingMode, list);
                        }
                        list.Add(element);
                    }
                }
            }
        }

        protected virtual void DrawQueue(GameTime time, SpriteBatch spriteBatch)
        {
            foreach (var entry in _elementQueue)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, GetSamplerState(entry.Key), DepthStencilState.Default);
                var list = entry.Value;
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    spriteBatch.Draw(time, list[i]);
                }
                spriteBatch.End();

                list.Clear();
            }
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
