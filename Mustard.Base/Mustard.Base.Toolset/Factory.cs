using System;
namespace Mustard.Base.Toolset;

public class Factory<T>
{
    public static Factory<T> Instance { get; set;}

    private Factory() { }   

    public T CreateProduct()
    {
        var instance =  Activator.CreateInstance<T>();
        return instance;
    }
}
