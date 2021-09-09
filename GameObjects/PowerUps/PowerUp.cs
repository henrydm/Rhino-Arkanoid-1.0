using Rhino.Display;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.AnimationObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.PowerUps
{
    enum PowerType { None, MultiBall, SlowBall, FastBall, FireBall, StickyBall, LongPad, ShortPad, ExtraLife, Laser, FailSafe }

    class PowerUp : Drawable
    {
        public PowerType Type;
        const double _speed = 2.5;
        private DateTime _activationTime;
        public double LiveTime = 15000;
        public bool IsActive => RemainingTime < LiveTime;

        public List<Drawable> Ornaments;
        public List<IAnimator> _animators;
        public double RemainingTime => (DateTime.Now - _activationTime).TotalMilliseconds;


        /// TODO Check if pullbiggest is removing the bigest one
        public PowerUp(List<Drawable> drawables, PowerType type) : base(drawables?.PullBiggest())
        {
            _activationTime = DateTime.MinValue;
            Type = type;
            Ornaments = drawables;
            _animators = new List<IAnimator> { new Translator(_speed, -Vector3d.ZAxis) };

        }

        public void AppendTx(Transform tx)
        {
            Transform *= tx;
            Ornaments?.ForEach(_ => _.Transform *= tx);
        }

        public virtual void Activate(Level level)
        {
            _activationTime = DateTime.Now;
        }

        public virtual void Deactivate(Level level)
        {

        }

        public override void Draw(DisplayPipeline dp, double ellapsedMs)
        {

            Ornaments?.ForEach(_ => _.Draw(dp, ellapsedMs));
            base.Draw(dp, ellapsedMs);
        }

        public void ProcessFrame(double ellapsedMs)
        {
            _animators?.ForEach(_ => _.ProcessFrame(new List<Drawable>(Ornaments) { this }, ellapsedMs));
        }

        public static PowerUp Create(List<Drawable> drawables, PowerType type)
        {
            switch (type)
            {
                case PowerType.MultiBall:
                    return new MultiBall(drawables, 3);
                case PowerType.SlowBall:
                    return new BallSpeed(drawables, 0.6);
                case PowerType.FastBall:
                    return new BallSpeed(drawables, 1.2);
                case PowerType.FireBall:
                    return null;
                case PowerType.StickyBall:
                    return new StickyBall(drawables);
                case PowerType.LongPad:
                    return new ResizePad(drawables, 1.5);
                case PowerType.ShortPad:
                    return new ResizePad(drawables, 0.6);
                case PowerType.ExtraLife:
                    return new ExtraLife(drawables, 1);
                case PowerType.Laser:
                    return null;
            }

            return null;
        }
    }
}
