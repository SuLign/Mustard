using Mustard.UI.Sunflower.ExControls;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mustard.UI.Sunflower;

public static class SunFlowerCore
{
    private static Uri currentThemeUri = null;
    private static string currentTheme = "LightTheme";

    public static void InitTheme()
    {
        if (!File.Exists("colorProfile.conf"))
        {
            SetTheme("DarkTheme");
        }
        else
        {
            try
            {
                using var streamReader = new StreamReader("colorProfile.conf");
                var content = streamReader.ReadToEnd();
                DecodeConfFile(content);
            }
            catch
            {
                SetTheme("LightTheme");
            }
        }
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

    public static void ExportColorPro(string path = "colorProfile.conf")
    {
        if (currentThemeUri != null)
        {
            var appResources = Application.Current.Resources;
            var r = appResources.MergedDictionaries.ToList().Find(e => (e.Source == currentThemeUri));
            if (r != null)
            {
                try
                {
                    using var fwriter = new StreamWriter(path);
                    foreach (var item in r.Keys)
                    {
                        if (r[item] is Color)
                        {
                            fwriter.WriteLine($"{item} = {r[item]}");
                        }
                    }
                    JWMessageBox.Show($"导出配色方案成功！");
                }
                catch (Exception ex)
                {
                    JWMessageBox.Show($"导出配色方案失败：{ex.Message}");
                }
            }
        }
    }

    public static void ImportColorPro(string fielPath)
    {
        try
        {
            using var streamReader = new StreamReader(fielPath);
            var content = streamReader.ReadToEnd();
            DecodeConfFile(content);
        }
        catch { }
    }

    private static void DecodeConfFile(string content)
    {
        var contentLines = content.Split('\n').ToList();
        var configParaPairs = new List<(string paraName, string paraValue)>();

        foreach (var line in contentLines)
        {
            var splits = line.Split('=').Select(e => e.Trim()).ToList();
            if (splits.Count != 2 || splits[0].StartsWith("#")) continue;
            configParaPairs.Add(new(splits[0], splits[1]));
        }
        if (configParaPairs.Exists(e => e.paraName.ToLower() == "basetheme"))
        {
            var baseTheme = configParaPairs.Find(e => e.paraName == "basetheme");
            configParaPairs.Remove(baseTheme);
            SetTheme(baseTheme.paraValue);
        }
        foreach (var pair in configParaPairs)
        {
            var sBrush = new SolidColorBrush();
            sBrush.SetValue(SolidColorBrush.ColorProperty, ColorConverter.ConvertFromString(pair.paraValue));
            Application.Current.Resources[pair.paraName] = sBrush;
        }
    }

    public static void SetTheme(string theme)
    {
        FindAndRemove(currentThemeUri);
        currentThemeUri = FindAndAdd($"pack://application:,,,/Mustard.UI.Sunflower;component/ColorThemes/{theme}.xaml");
    }

    public static void FindAndRemove(Uri uri)
    {
        if (uri == null) return;
        var appResources = Application.Current.Resources;
        var findRes = appResources.MergedDictionaries.ToList().Find(e => (e.Source == uri));
        if (findRes != null)
        {
            appResources.MergedDictionaries.Remove(findRes);
        }
    }

    public static Uri FindAndAdd(string sourcePath, Uri targetUri = null)
    {
        var appResources = Application.Current.Resources;
        Uri uri;
        if (targetUri == null)
        {
            uri = new Uri(sourcePath);
        }
        else uri = targetUri;
        if (appResources.MergedDictionaries.ToList().Find(e => (e.Source == uri)) == null)
        {
            appResources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        }
        return uri;
    }
}
