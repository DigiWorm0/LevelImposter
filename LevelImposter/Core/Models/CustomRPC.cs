using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Core
{
    // Among Us     0 - 32
    // TOR          60 - 73, 100 - 149
    // Las Monjas   60 - 69, 75 - 194
    // ToU          100 - 210, 220 - 251
    // Submerged    210 - 214

    // LI           50 - 59 (Guess I'll exist here...)

    public enum RpcIds
    {
        Trigger = 50,
        Teleport,
        SendMapId,
    }
}
