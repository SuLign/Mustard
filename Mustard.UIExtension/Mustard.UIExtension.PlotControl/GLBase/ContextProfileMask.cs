using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mustard.UIExtension.PlotControl.GLBase;

//
// 摘要:
//     Not used directly.
[Flags]
public enum ContextProfileMask
{
    //
    // 摘要:
    //     Original was GL_CONTEXT_CORE_PROFILE_BIT = 0x00000001
    ContextCoreProfileBit = 0x1,
    //
    // 摘要:
    //     Original was GL_CONTEXT_COMPATIBILITY_PROFILE_BIT = 0x00000002
    ContextCompatibilityProfileBit = 0x2
}
