using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mustard.Base.Toolset;

public class Classes
{
    public static bool LoadTypesFromCurrentAssembly<TInstance, TAttr>(out List<Tuple<TAttr, Type>> outTypes) where TAttr : Attribute
    {
        outTypes = null;
        var assembly = Assembly.GetEntryAssembly();
        var types = assembly.GetTypes();
        foreach (var item in types)
        {
            if (item.IsClass)
            {
                var attr = item.GetCustomAttribute<TAttr>();
                if (attr != null)
                {
                    try
                    {
                        if (outTypes == null) outTypes = new List<Tuple<TAttr, Type>>();
                        outTypes.Add(new Tuple<TAttr, Type>(attr, item));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy - MM - dd HH - mm - ss - fff")}: 加载库内容失败【{ex.Message}】");
                        continue;
                    }
                }
            }
        }
        return (outTypes != null);
    }

    public static bool LoadMultiTypeFromDll<TAttr>(string dllPath, out List<Tuple<TAttr, Type>> outTypes, out Assembly library) where TAttr : Attribute
    {
        outTypes = null;
        library = null;
        if (!File.Exists(dllPath)) return false;
        library = Assembly.LoadFile(dllPath);
        var types = library.GetTypes();
        foreach (var item in types)
        {
            if (item.IsClass)
            {
                var attr = item.GetCustomAttribute<TAttr>();
                if (attr != null)
                {
                    try
                    {
                        if (outTypes == null) outTypes = new List<Tuple<TAttr, Type>>();
                        outTypes.Add(new Tuple<TAttr, Type>(attr, item));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("yyyy - MM - dd HH - mm - ss - fff")}: 加载库内容失败【{ex.Message}】");
                        continue;
                    }
                }
            }
        }
        return (outTypes != null);
    }
}
