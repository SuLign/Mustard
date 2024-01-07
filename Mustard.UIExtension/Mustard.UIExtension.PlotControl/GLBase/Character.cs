using OpenTK;

namespace Mustard.UIExtension.PlotControl.GLBase;

public struct Character
{
    public int TextureID { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Bearing { get; set; }
    public int Advance { get; set; }
    public Vector2 Rect { get; set; }
}
