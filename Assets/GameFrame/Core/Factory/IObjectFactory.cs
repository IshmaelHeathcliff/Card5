using Cysharp.Threading.Tasks;

namespace Core.Factory
{
    public interface IObjectFactory<T>
    {
        T Create();
    }

    public interface IAsyncObjectFactory<T>
    {
        UniTask<T> Create();
    }
}
