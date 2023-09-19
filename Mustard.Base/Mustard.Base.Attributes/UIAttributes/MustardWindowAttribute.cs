namespace Mustard.Base.Attributes.UIAttributes;

public class MustardWindowAttribute : Attribute
{
    public string Title { get; set; }
    public bool IsEntryPoint { get; set; }

    public MustardWindowAttribute(string title, bool isEntryPoint)
    {
        Title = title;
        IsEntryPoint = isEntryPoint;
    }

    public MustardWindowAttribute(string title) : this(title, false) { }

    public MustardWindowAttribute(bool isEntryPoint) : this(null, isEntryPoint) { }
}

