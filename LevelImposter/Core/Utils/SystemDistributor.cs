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
            // Breaks task-fuel
            SystemTypes.LowerEngine,
            SystemTypes.UpperEngine
        };

        // Decontamination
        private static int _minDeconID => Enum.GetValues(typeof(SystemTypes)).Length;
        private static int _deconID = _minDeconID;

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
            if (_deconID > byte.MaxValue)
            {
                LILogger.Error($"Map is out of decontamination IDs! (Max {byte.MaxValue - _minDeconID})");
                return (SystemTypes)_minDeconID;
            }

            // Assign System
            _deconID++;
            LILogger.Warn($"Assigned {_deconID} ({(SystemTypes)_deconID})");
            return (SystemTypes)_deconID;
        }

        /// <summary>
        /// Resets the SystemTypes generator
        /// </summary>
        public static void Reset()
        {
            _systemID = 1;
            _deconID = _minDeconID;
        }
    }
}
