using System;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace Core.Factory
{
    public abstract class AsyncObjectFactory<T> : IAsyncObjectFactory<T> where T : new()
    {
        public abstract UniTask<T> Create();
    }

    /// <summary>
    /// 默认对象工厂：相关对象是通过New 出来的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultObjectFactory<T> : IObjectFactory<T> where T : new()
    {
        public virtual T Create()
        {
            return new T();
        }
    }

    public static class ObjectFactory
    {
        /// <summary>
        /// 动态创建类的实例：创建有参的构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static object Create(Type type, params object[] constructorArgs)
        {
            return Activator.CreateInstance(type, constructorArgs);
        }

        /// <summary>
        /// 动态创建类的实例：泛型扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static T Create<T>(params object[] constructorArgs)
        {
            return (T)Create(typeof(T), constructorArgs);
        }

        /// <summary>
        /// 动态创建类的实例：创建无参/私有的构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object CreateNonPublicConstructorObject(Type type)
        {
            // 获取私有构造函数
            ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            // 获取无参构造函数
            ConstructorInfo ctor = Array.Find(constructorInfos, c => c.GetParameters().Length == 0) ?? throw new Exception("Non-Public Constructor() not found! in " + type);

            return ctor.Invoke(null);
        }

        /// <summary>
        /// 动态创建类的实例：创建无参/私有的构造函数  泛型扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CreateNonPublicConstructorObject<T>()
        {
            return (T)CreateNonPublicConstructorObject(typeof(T));
        }

        /// <summary>
        /// 创建带有初始化回调的 对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="onObjectCreate"></param>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static object CreateWithInitialAction(Type type, Action<object> onObjectCreate,
            params object[] constructorArgs)
        {
            object obj = Create(type, constructorArgs);
            onObjectCreate(obj);
            return obj;
        }

        /// <summary>
        /// 创建带有初始化回调的 对象：泛型扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="onObjectCreate"></param>
        /// <param name="constructorArgs"></param>
        /// <returns></returns>
        public static T CreateWithInitialAction<T>(Action<T> onObjectCreate, params object[] constructorArgs)
        {
            T obj = Create<T>(constructorArgs);
            onObjectCreate(obj);
            return obj;
        }
    }
}
