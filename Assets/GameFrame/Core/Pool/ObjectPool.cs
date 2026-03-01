using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Pool
{
    public abstract class ObjectPool<T> : IObjectPool<T> where T : class, new()
    {
        public int MaxSize { private get; set; } = 100;
        protected Stack<T> Pool = new();
        public virtual T Allocate()
        {
            T obj = Pool.Count > 0 ? Pool.Pop() : CreateObject();

            return obj;
        }

        public virtual void Recycle(T obj)
        {
            if (Pool.Count > MaxSize)
            {
                return;
            }

            Pool.Push(obj);
        }

        public virtual void InitPool(int initialSize, int maxSize)
        {
            MaxSize = maxSize;

            for (int i = 0; i < initialSize; i++)
            {
                Pool.Push(CreateObject());
            }

        }

        protected virtual T CreateObject()
        {
            return new T();
        }
    }

    public abstract class AsyncObjectPool<T> : IAsyncObjectPool<T> where T : class
    {
        public int MaxSize { private get; set; } = 100;
        protected Stack<T> Pool = new();
        public virtual async UniTask<T> Allocate()
        {
            T obj = Pool.Count > 0 ? Pool.Pop() : await CreateObjectAsync();

            return obj;
        }

        public virtual void Recycle(T obj)
        {
            if (Pool.Count > MaxSize)
            {
                return;
            }

            Pool.Push(obj);
        }

        public virtual async UniTask InitPool(int initialSize, int maxSize)
        {
            MaxSize = maxSize;

            for (int i = 0; i < initialSize; i++)
            {
                Pool.Push(await CreateObjectAsync());
            }

        }

        protected abstract UniTask<T> CreateObjectAsync();
    }
}
