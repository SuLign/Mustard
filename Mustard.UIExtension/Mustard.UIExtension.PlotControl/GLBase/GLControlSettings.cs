using OpenTK.Graphics;

namespace Mustard.UIExtension.PlotControl.GLBase;

public class GLControlSettings
{
    public bool RenderContinuously { get; set; } = true;


    public bool UseDeviceDpi { get; set; } = true;


    public IGraphicsContext ContextToUse { get; set; }

    public GraphicsContextFlags GraphicsContextFlags { get; set; }

    public ContextProfileMask GraphicsProfile { get; set; }

    public int MajorVersion { get; set; } = 3;


    public int MinorVersion { get; set; } = 3;


    public bool IsUsingExternalContext => ContextToUse != null;

    internal GLControlSettings Copy()
    {
        return new GLControlSettings
        {
            ContextToUse = ContextToUse,
            GraphicsContextFlags = GraphicsContextFlags,
            GraphicsProfile = GraphicsProfile,
            MajorVersion = MajorVersion,
            MinorVersion = MinorVersion,
            RenderContinuously = RenderContinuously,
            UseDeviceDpi = UseDeviceDpi
        };
    }

    internal static bool WouldResultInSameContext(GLControlSettings a, GLControlSettings b)
    {
        if (a.MajorVersion != b.MajorVersion)
        {
            return false;
        }

        if (a.MinorVersion != b.MinorVersion)
        {
            return false;
        }

        if (a.GraphicsProfile != b.GraphicsProfile)
        {
            return false;
        }

        if (a.GraphicsContextFlags != b.GraphicsContextFlags)
        {
            return false;
        }

        return true;
    }
}
