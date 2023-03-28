using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImTool.Scene3D
{
    [Flags]
    public enum ActorFlags : int
    {
        CanNeverUpdate = 1,
        DontUpdate     = 2,
        DontRender     = 4,
    }
}
