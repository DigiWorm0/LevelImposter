using System;

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
