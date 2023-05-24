using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Core
{
    /// <summary>
    /// A list of constants used within LevelImposter
    /// </summary>
    public static class LIConstants
    {
        public const string MAP_NAME = "Random Custom Map";     // Name to pipulate Constants.MapNames
        public const float PLAYER_POS = -5.0f;                  // Z value of the player
        public const int CONNECTION_TIMEOUT = 16;               // Maximum time for host to wait (AmongUsClient.MAX_CLIENT_WAIT_TIME)
        public const int MAX_LOAD_TIME = 8;                    // Maximum time to build map before aborting
    }
}
