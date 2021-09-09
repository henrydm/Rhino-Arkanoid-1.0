using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.PowerUps
{
    class StickyBall:PowerUp
    {
        public StickyBall(List<Drawable> drawables) :base(drawables, PowerType.StickyBall)
        {

        }
    }
}
