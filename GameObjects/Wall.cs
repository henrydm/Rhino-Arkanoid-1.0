using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects
{
    class Wall
    {
        List<Drawable> _limits;
        List<Drawable> _ornaments;

        public double PadMinX { get; }
        public double PadMaxX { get; }
        private Mesh LimitsMesh { get; }
        public BoundingBox LimitsBox { get; }
        protected byte[] _hitSound;

        //Maybe pass the pad to compute the end limits
        public Wall(List<Drawable> limits, List<Drawable> forniture, byte[] hitSound)
        {
            _limits = limits;
            _ornaments = forniture;
            _hitSound = hitSound;

             LimitsMesh = new Mesh();
            _limits?.ForEach(_ => LimitsMesh.Append(_.MeshTransformed));

            LimitsBox = LimitsMesh.GetBoundingBox(true);


            var cutLine = new Line(new Point3d(LimitsBox.Min.X - 1, 0, 1), new Point3d(LimitsBox.Max.X + 1, 0, 1));

            var pts = Intersection.MeshLine(LimitsMesh, cutLine);

            PadMinX = double.MinValue;
            PadMaxX = double.MaxValue;
            var minPtLeft = Point3d.Unset;
            var minPtRight = Point3d.Unset;

            foreach (var pt in pts)
            {
                if (pt.X < 0 && pt.X > PadMinX) PadMinX = pt.X;
                if (pt.X > 0 && pt.X < PadMaxX) PadMaxX = pt.X;
            }
        }

        public virtual bool Collide(Line motionLine, double ballRadius, out Point3d intersectionPt, out Vector3d normal, out Drawable collisionObj)
        {
            intersectionPt = Point3d.Unset;
            normal = Vector3d.Unset;
            collisionObj = null;

            var min = double.MaxValue;
            foreach (var drawable in _limits)
            {
                if (!drawable.Collide(motionLine, ballRadius, out var pt, out var currentNormal)) continue;

                var currentDistance = motionLine.From.DistanceToSquared(pt);
                if (currentDistance > min) continue;
                min = currentDistance;
                intersectionPt = pt;
                normal = currentNormal;
                collisionObj = drawable;
            }


            return min != double.MaxValue;
        }

        public virtual void PlayHitSound()
        {
            Sound.Play(_hitSound);
        }

        public void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            _limits?.ForEach(_ => _?.Draw(dp,  ellapsedMs));
            _ornaments?.ForEach(_ => _?.Draw(dp,  ellapsedMs));
        }


    }
}
