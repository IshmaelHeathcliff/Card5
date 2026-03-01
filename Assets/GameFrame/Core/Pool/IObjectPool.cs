using Cysharp.Threading.Tasks;

namespace Core.Pool
{
    public interface IObjectPool<T> where T : class
    {
        T Allocate();
        void Recycle(T obj);
    }

    public interface IAsyncObjectPool<T> where T : class
    {
        UniTask<T> Allocate();
        void Recycle(T obj);
    }

    public interface IMultiObjectPool<T> where T : class
    {
        T Allocate(string id);
        void Recycle(T obj, string id);
    }

    public interface IMultiAsyncObjectPool<T> where T : class
    {
        UniTask<T> Allocate(string id);
        void Recycle(string id, T obj);
    }
}
