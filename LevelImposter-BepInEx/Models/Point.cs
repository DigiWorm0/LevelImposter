using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Models
{
    [Serializable]
    class Point
    {
        public Point()
        {
            this.x = 0;
            this.y = 0;
        }

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;
    }
}
