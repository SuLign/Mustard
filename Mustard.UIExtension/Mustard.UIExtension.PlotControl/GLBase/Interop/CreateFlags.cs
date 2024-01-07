using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.UIExtension.PlotControl.GLBase.Interop
{
    [Flags]
    internal enum CreateFlags
    {
        Multithreaded = 0x4,
        PureDevice = 0x10,
        HardwareVertexProcessing = 0x40
    }
}
