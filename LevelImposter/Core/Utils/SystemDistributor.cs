using System;
using System.Linq;

namespace LevelImposter.Core
{
    public static class SystemDistributor
    {
        // Systems
        private static int _systemID = 1;
        public static readonly SystemTypes DEFAULT_SYSTEM = 0; 
        private static readonly SystemTypes[] SYSTEM_BLACKLIST = new SystemTypes[]
        {
            SystemTypes.LowerEngine,
            SystemTypes.UpperEngine
        };

        // Decontamination
        private static int _deconID = 0;
        private static readonly SystemTypes[] DECON_SYSTEMS = new SystemTypes[]
        {
            SystemTypes.Decontamination,
            SystemTypes.Decontamination2,
            SystemTypes.Decontamination3
        };

        /// <summary>
        /// Generates a dedicated SystemTypes for a system. 
        /// </summary>
        /// <returns>
        /// A random, generic <c>SystemTypes</c> element. The enum value of this is irrelevant, only used as a byte.
        /// </returns>
        public static SystemTypes GetNewSystemType()
        {
            SystemTypes systemType;
            do
            {
                // Check Out of Bounds
                if (_systemID > byte.MaxValue)
                {
                    LILogger.Error($"Map is out of room IDs! (Max {byte.MaxValue - SYSTEM_BLACKLIST.Length})");
                    return DEFAULT_SYSTEM;
                }

                // Assign System
                systemType = (SystemTypes)_systemID;
                _systemID++;
            }
            while (SYSTEM_BLACKLIST.Any(x => x == systemType));
            return systemType;
        }

        /// <summary>
        /// Generates a dedicated SystemTypes for a decontamination system.
        /// </summary>
        /// <returns>
        /// A random, generic <c>SystemTypes</c> element. The enum value of this is irrelevant, only used as a byte.
        /// </returns>
        public static SystemTypes GetNewDeconSystemType()
        {
            // Check Out of Bounds
            if (_deconID >= DECON_SYSTEMS.Length)
            {
                LILogger.Error($"Map is out of decontamination IDs! (Max {DECON_SYSTEMS.Length})");
                return DECON_SYSTEMS[0];
            }

            // Assign System
            return DECON_SYSTEMS[_deconID++];
        }

        /// <summary>
        /// Resets the SystemTypes generator
        /// </summary>
        public static void Reset()
        {
            _systemID = 1;
        }
    }
}
