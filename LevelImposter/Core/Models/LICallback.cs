using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Core
{
    [Serializable]
    public class LICallback<T>
    {
        public int v { get; set; }
        public string error { get; set; }
        public T data { get; set; }
    }
}
