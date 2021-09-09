using Rhino.Geometry;
using System.Collections.Generic;

namespace RhinoArkanoid.GameObjects.AnimationObjects
{
    class Translator : IAnimator
    {
        public double Speed;
        public Vector3d Direction;

        public Translator(double speed, Vector3d direction)
        {
            Speed = speed;
            Direction = direction;
        }
   
        public virtual void ProcessFrame(List<Drawable> drawables, double ellapsedMs)
        {
            var delta = Speed * (ellapsedMs / 1000);
            var tx = Transform.Translation(Direction * delta * Speed);
           drawables?.ForEach(_ => _.Transform = tx*_.Transform);

        }
    }
}
