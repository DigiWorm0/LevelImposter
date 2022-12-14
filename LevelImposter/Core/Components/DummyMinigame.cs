using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelImposter.Core
{
    public class DummyMinigame : Minigame
    {
        public DummyMinigame(IntPtr intPtr) : base(intPtr)
        {
        }

        public override void Begin(PlayerTask task) { }
    }
}
