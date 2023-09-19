using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mustard.Base.Toolset;

public class Classes
{

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private IList<Assembly> _assemblies;

    private Classes()
    {
        _assemblies = new List<Assembly>();
    }

    public static Classes FromDynamicLinkLibrary(string dllPath)
    {
        if (!File.Exists(dllPath)) return null;
        var assembly = Assembly.LoadFrom(dllPath);
        return FromAssembly(assembly);
    }

    public static Classes FromAssembly(Assembly assembly)
    {
        Classes classes = new Classes();
        classes._assemblies.Add(assembly);
        return classes;
    }

    public static Classes FromThisAssembly()
    {
        var assembly = Assembly.GetCallingAssembly();
        return FromAssembly(assembly);
    }

    public static Classes FromEntryPointAssembly()
    {
        var assembly = Assembly.GetEntryAssembly();
        return FromAssembly(assembly);
    }

    public bool AddAssemblyFromFile(string path)
        => AddAssembly(Assembly.LoadFrom(path));

    public bool AddAssemblyFromEntryPoint()
        => AddAssembly(Assembly.GetEntryAssembly());

    public bool AddAssembly(Assembly assembly)
    {
        if (assembly == null) return false;
        if (_assemblies.Contains(assembly)) return true;
        _assemblies.Add(assembly);
        return true;
    }

    public Type[] BasedOn<TParent, TAttribute>
        () where TAttribute : Attribute
        => BasedOn<TAttribute>(typeof(TParent));

    public Type[] BasedOn<TParent, TAttribute>
        (Func<TAttribute, bool> attributeFilter) where TAttribute : Attribute
        => BasedOn(typeof(TParent), attributeFilter);

    public Type[] BasedOn<TAttribute>(Type type) where TAttribute : Attribute
        => BasedOn<TAttribute>(type, null);

    public Type[] BasedOn<TAttribute>
        (Type type, Func<TAttribute, bool> attributeFilter) where TAttribute : Attribute
    {
        var ret = new List<Type>();
        _assemblies.ToList().ForEach(e =>
        {
            var types = e.GetTypes();
            foreach (var typeItem in types)
            {
                if (typeItem.IsClass)
                {
                    var attr = typeItem.GetCustomAttribute<TAttribute>();
                    if (attr != null)
                    {
                        if (attributeFilter != null) if (!attributeFilter(attr)) continue;
                        ret.Add(typeItem);
                    }
                }
            }
        });
        return ret.ToArray();
    }

    private IEnumerable<Type> GetTopLevelInterfaces(Type type)
    {
        var interfaces = type.GetInterfaces();
        var topLevel = new List<Type>(interfaces);
        foreach (var @interface in interfaces)
            foreach (var parent in @interface.GetInterfaces()) topLevel.Remove(parent);
        return topLevel;
    }
}
