using System;
using System.Collections.Generic;
using System.Text;

namespace LevelImposter.Shop
{
    [Serializable]
    public class LICallback<T>
    {
        public int v { get; set; }
        public string error { get; set; }
        public T data { get; set; }
    }
}
