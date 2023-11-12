using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mustard.UI.Sunflower;

public static class SunFlowerCore
{
    public static void Init()
    {
        LoadConverters();
    }

    public static void LoadConverters(string converterLibrary = null)
    {
        if (converterLibrary == null)
        {
            converterLibrary = "Mustard.UI.Converter.dll";
        }
        if (!File.Exists(converterLibrary)) throw new FileNotFoundException($"\"{Path.GetFileName(converterLibrary)}\"不存在，请检查。");
        var assembly = Assembly.LoadFrom(Path.GetFullPath(converterLibrary));
        var converterTypes = assembly.GetTypes().Where(e => e.GetInterface("IValueConverter") != null)?.ToList();
        if (converterTypes == null || converterTypes.Count == 0) throw new FileLoadException($"未能从\"{Path.GetFileName(converterLibrary)}\"中找到继承于\"IValueConverter\"接口的转换类。");
        foreach (var converterType in converterTypes)
        {
            Application.Current.Resources.Add(converterType.Name, Activator.CreateInstance(converterType));
        }
    }
}
