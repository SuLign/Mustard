using System.Windows.Media;

namespace Mustard.UIExtension.PlotControl;

public class MarkTag
{
    internal int TagID { get; set; }
    public string TagName { get; set; }
    public string TagContennt { get; set; }
    public int MarkBindLineIndex { get; set; } = -1;
    public double TagPositionX { get; set; }
    public double TagPositionY { get; set; }
    public Color TagColor { get; set; }
}
