namespace Mustard.UIExtension.PlotControl;

internal struct PanelDrawingElement
{
    public float X;
    public float Y;
    public float Z;
    public float ScR;
    public float ScG;
    public float ScB;
    public float ScA;

    public PanelDrawingElement()
    {
        ScA = 1;
        ScR = 0;
        ScG = 0;
        ScB = 0;
        X = 0;
        Y = 0;
        Z = 0;
    }
}
