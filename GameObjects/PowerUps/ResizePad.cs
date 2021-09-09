using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.PowerUps
{
    class ResizePad : PowerUp
    {
        double _factor;
        public ResizePad(List<Drawable> drawables, double factor) : base( drawables, factor > 1 ? PowerType.LongPad : PowerType.ShortPad)
        {
            _factor = factor.ClampMin(0.1);
        }
        public override void Activate(Level level)
        {
            level.Pad.SetScaleX(_factor);
            base.Activate(level);
        }

        public override void Deactivate(Level level)
        {
            level.Pad.SetScaleX(1 / _factor);
            base.Deactivate(level);
        }
    }
}
