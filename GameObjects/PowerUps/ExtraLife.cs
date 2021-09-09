using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.PowerUps
{
    class ExtraLife : PowerUp
    {
        private int _lifeAmount;
        public ExtraLife(List<Drawable> drawables,int lives) : base(drawables,PowerType.ExtraLife)
        {
            _lifeAmount = lives;
            LiveTime = 0;
        }

        public override void Activate(Level level)
        {
            level.SetLives(level.Lives + _lifeAmount);
        }
    }
}
