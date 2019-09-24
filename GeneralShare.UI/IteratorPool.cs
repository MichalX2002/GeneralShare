using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collections;
using System;

namespace GeneralShare.UI
{
    public static class IteratorPool
    {
        private static Bag<TransformCharIterator> _transformIterators;

        public static int PoolCapacity => 512;

        static IteratorPool()
        {
            _transformIterators = new Bag<TransformCharIterator>();
        }

        public static ICharIterator Rent(
            ICharIterator source, bool leaveSourceOpen,
            TransformCharIterator.Transform transform, bool leaveTransformOpen)
        {
            lock (_transformIterators)
            {
                if (_transformIterators.TryTake(out var iterator))
                {
                    iterator.Set(source, leaveSourceOpen, transform, leaveTransformOpen);
                    return iterator;
                }
            }
            return new TransformCharIterator(source, leaveSourceOpen, transform, leaveTransformOpen);
        }

        public static void Return(ICharIterator iterator)
        {
            if (iterator == null)
                return;

            if (iterator is TransformCharIterator transformIterator)
            {
                lock (_transformIterators)
                {
                    if (transformIterator._isInUse && _transformIterators.Count < PoolCapacity)
                    {
                        _transformIterators.Add(transformIterator);
                        transformIterator._isInUse = false;
                    }
                }
            }
            else
                throw new ArgumentException("The iterator was not rented from this pool.", nameof(iterator));
        }
    }

}
