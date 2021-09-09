using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.PowerUps
{
    class MultiBall : PowerUp
    {
        private readonly int _ballAmount;

        public MultiBall(List<Drawable> drawables, int ballAmount) : base(drawables, PowerType.MultiBall)
        {
            _ballAmount = ballAmount;
            LiveTime = 0;
        }
        public override void Activate(Level level)
        {
            base.Activate(level);

            var stickedBalls = (from ball in level.Balls where ball.StickOnPad select ball).Count();
            for (int i = stickedBalls; i < _ballAmount; i++)
            {
                level.Balls.AddRange(level.CreateBalls());
            }
        }

        public override void Deactivate(Level level)
        {
            base.Deactivate(level);
        }
    }
}
