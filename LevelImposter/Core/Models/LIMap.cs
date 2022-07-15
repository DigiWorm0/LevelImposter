using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LIMap : LIMetadata
    {
        public LIElement[] elements { get; set; }
    }
}
