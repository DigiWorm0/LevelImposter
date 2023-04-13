using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Core
{
    [Serializable]
    public class MissingShipException : Exception
    {
        public const string EXCEPTION_STRING = "LIShipStatus.Instance or LIShipStatus.ShipStatus is null";

        public MissingShipException() : base(EXCEPTION_STRING)
        { }
    }
}
