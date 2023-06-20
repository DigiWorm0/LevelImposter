using System;

namespace LevelImposter.Shop
{
    [Serializable]
    public class GHAsset
    {
        public string name { get; set; }
        public int size { get; set; }
        public string browser_download_url { get; set; }
    }
}
