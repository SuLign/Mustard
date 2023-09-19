using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Mustard.Base.Toolset;

public class IoCContainer
{
    private readonly ConcurrentDictionary<Type, Type> types;

    public static IoCContainer Instance { get; }

    static IoCContainer()
    {
        Instance = new IoCContainer();
    }

    private IoCContainer()
    {
        types = new ConcurrentDictionary<Type, Type>();
    }

    public void Register<TInterface, TImplementation>() where TImplementation : TInterface
    {
        if (types.ContainsKey(typeof(TInterface))) return;
        types.TryAdd(typeof(TInterface), typeof(TImplementation));
    }

    public TInterface Resolve<TInterface>()
    {
        if (types.ContainsKey(typeof(TInterface)))
        {
            var typeofImp = types[typeof(TInterface)];
            return (TInterface)Create(typeofImp);
        }
        return default;
    }

    private object Create(Type type)
    {
        var defaultConstructor = type.GetConstructors()[0];
        var defaultParams = defaultConstructor.GetParameters();
        var paramaters = defaultParams.Select(param => Create(param.ParameterType));
        return defaultConstructor?.Invoke(defaultParams);
    }
}
