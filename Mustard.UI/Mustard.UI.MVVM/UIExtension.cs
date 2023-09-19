using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Mustard.UI.MVVM;

public static class UIExtention
{
    public static void RegistDependencyProperty<T>() where T : DependencyObject => RegistDependencyProperty(typeof(T));

    public static void RegistDependencyProperty(Type type)
    {
        var staticFields = type.GetRuntimeFields();
        var properties = type.GetRuntimeProperties();
        var methods = type.GetRuntimeMethods();
        foreach (var property in properties)
        {
            var met = methods.FirstOrDefault(e => e.IsStatic && e.Name == $"{property.Name}Changed");
            PropertyChangedCallback call = (met == null ? null : (PropertyChangedCallback)met.CreateDelegate(typeof(PropertyChangedCallback)));
            RegistProperty(property, null, call);
        }

        void RegistProperty(PropertyInfo proInfo, object defaultVal = null, PropertyChangedCallback callback = null)
        {
            var depField = staticFields.FirstOrDefault(e => e.IsStatic && (e.Name == $"{proInfo.Name}Property"));
            if (depField == null) return;
            var frameworkProMeta = new FrameworkPropertyMetadata();
            if (defaultVal != null)
            {
                frameworkProMeta.DefaultValue = defaultVal;
            }
            if (callback != null)
            {
                frameworkProMeta.PropertyChangedCallback = callback;
            }
            var dep = DependencyProperty.Register(
                proInfo.Name,
                proInfo.PropertyType,
                type, frameworkProMeta
                );
            depField.SetValue(null, dep);
        }
    }
}
