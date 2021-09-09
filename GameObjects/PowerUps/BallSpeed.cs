using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.PowerUps
{
    class BallSpeed : PowerUp
    {
        double _speedFactor;

        public BallSpeed(List<Drawable> drawables, double factor) : base(drawables, factor > 1 ? PowerType.FastBall : PowerType.SlowBall)
        {
            _speedFactor = factor;
        }

        public override void Activate(Level level)
        {
            base.Activate(level);
            // foreach(var ball in level.b)
        }
    }
}
