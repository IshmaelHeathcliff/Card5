using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Core.Pool
{
    public abstract class PoolSystem<T> : AbstractSystem, IObjectPool<T> where T : class, new()
    {
        public int MaxSize { private get; set; } = 100;
        protected Stack<T> Pool;
        public virtual T Allocate()
        {
            T obj = Pool.Count > 0 ? Pool.Pop() : CreateObject();
            return obj;
        }

        public virtual void Recycle(T obj)
        {
            if (Pool.Count > MaxSize)
            {
                DestroyObject(obj);
                return;
            }

            Pool.Push(obj);
        }

        public virtual void InitPool(int size)
        {
            Pool = new Stack<T>();

            for (int i = 0; i < size; i++)
            {
                Pool.Push(CreateObject());
            }

        }

        protected abstract T CreateObject();
        protected abstract void DestroyObject(T obj);

        protected override void OnInit()
        {
            Pool = new Stack<T>();
        }
    }

    public abstract class AsyncPoolSystem<T> : PoolSystem<T>, IAsyncObjectPool<T> where T : class, new()
    {
        public new virtual async UniTask<T> Allocate()
        {
            T obj;
            if (Pool.Count > 0)
            {
                obj = Pool.Pop();
            }
            else
            {
                obj = await CreateObjectAsync();
            }

            return obj;
        }

        public new virtual async UniTask InitPool(int size)
        {
            Pool = new Stack<T>();

            for (int i = 0; i < size; i++)
            {
                Pool.Push(await CreateObjectAsync());
            }
        }

        protected sealed override T CreateObject()
        {
            return null;
        }

        protected abstract UniTask<T> CreateObjectAsync();

    }
}
