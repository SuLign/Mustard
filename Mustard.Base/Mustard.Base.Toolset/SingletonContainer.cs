using System;

namespace Mustard.Base.Toolset;

public delegate void SingletonAction<TSingleTon>(TSingleTon instance);

/// <summary>
/// 单例容器
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonContainer<T>
{
    private static SingletonAction<T> instanceUnregisterAction;
    private static Lazy<T> instance;

    /// <summary>
    /// 当前类型单例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance.Value;
            }
            return default;
        }
    }

    public static void RegisterSingletonInstance<TImplament>(TImplament instance, SingletonAction<T> unregAction) where TImplament : T
    {
        instanceUnregisterAction?.Invoke(SingletonContainer<T>.instance.Value);
        SingletonContainer<T>.instance = new Lazy<T>(() => instance);
        instanceUnregisterAction = unregAction;
    }

    public T TInstance => instance.Value;
}
