using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using RhinoArkanoid.GameObjects.PowerUps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RhinoArkanoid.GameObjects
{
    class Pad
    {
        double _padSpeed;
        byte[] _hitSound;
        List<Drawable> _drawables;
        public double MaxZ { get; }

        public BoundingBox BoundingBox => _drawables.GetBoundingBoxTransformed();

        public Pad(List<Drawable> drawables, double padSpeed, byte[] hitSound)
        {
            _hitSound = hitSound;
            _padSpeed = padSpeed;
            _drawables = drawables;
            MaxZ = _drawables.GetBoundingBoxTransformed().Max.Z;
        }

        public double _factor = 1;

        public void SetScaleX(double factor)
        {
            _factor = factor;
            var fullBox = _drawables.GetBoundingBoxTransformed();
            var width = fullBox.Max.X - fullBox.Min.X;
            _drawables.ForEach(_ => _.Transform *= Transform.Scale(new Plane(fullBox.Center, Vector3d.XAxis, Vector3d.YAxis), _factor, 1, 1));

        }

        /// <summary>
        /// Moves the pad if arrow keys are pressed.
        /// </summary>
        /// <param name="elapsedMilliseconds">Time betwen frames.</param>
        /// <param name="wall">Limit objects to collide.</param>
        public bool ProcessFrame(double elapsedMilliseconds, Wall wall, List<PowerUp> powerUps, out List<PowerUp> collisionPowerUps, out List<PowerUp> lostPowerUps)
        {
            var padBbox = _drawables.GetBoundingBoxTransformed();

            //Move the pad
            if (KeyBoard.PressedKey.Contains(Keys.Left) || KeyBoard.PressedKey.Contains(Keys.Right))
            {
                var targetVector = Vector3d.XAxis;
                if (KeyBoard.PressedKey.Contains(Keys.Left)) targetVector *= -1;

                var tx = Transform.Translation(targetVector * elapsedMilliseconds / 1000 * _padSpeed);
                padBbox.Transform(tx);

                //Clamp translations to wall limits
                if (padBbox.Min.X < wall.PadMinX) tx *= Transform.Translation(wall.PadMinX - padBbox.Min.X, 0, 0);
                if (padBbox.Max.X > wall.PadMaxX) tx *= Transform.Translation(wall.PadMaxX - padBbox.Max.X, 0, 0);

                _drawables.ForEach(_ => _.Transform *= tx);
            }

            //Process Power ups
            collisionPowerUps = new List<PowerUp>();
            lostPowerUps = new List<PowerUp>();
            foreach (var powerUp in powerUps)
            {
                var powerBox = powerUp.BoundingBoxTransformed;
                var collisionBox = BoundingBox.Intersection(powerBox, padBbox);
                if (collisionBox.IsValid) collisionPowerUps.SafeAdd(powerUp);
                else if (powerBox.Min.Z < 0) lostPowerUps.Add(powerUp);
            }

            return collisionPowerUps.Any();
        }

        public bool Collide(Line motionLine, double ballRadius, out Point3d intersectionPt, out Vector3d normal)
        {

            intersectionPt = Point3d.Unset;
            normal = Vector3d.Unset;
            if (!motionLine.IsValid || motionLine.To.Z > motionLine.From.Z || motionLine.To.Z- ballRadius > MaxZ) return false;

            //Intersection.LineLine(motionLine, new Line(new Point3d(0, 0, MaxZ+ballRadius), Vector3d.XAxis), out var a, out var b);
            //intersectionPt = motionLine.PointAt(a);

            var padBox = BoundingBox;
            var boxInterval = new Interval(padBox.Min.X, padBox.Max.X);
            return boxInterval.IncludesParameter(motionLine.To.X + ballRadius) || boxInterval.IncludesParameter(motionLine.To.X - ballRadius);

            //intersectionPt = Point3d.Unset;
            //normal = Vector3d.Unset;
            //var min = double.MaxValue;
            //foreach (var drawable in _drawables)
            //{
            //    if (!drawable.Collide(motionLine, ballRadius, out var pt, out var currentNormal)) continue;

            //    var currentDistance = motionLine.From.DistanceToSquared(pt);
            //    if (currentDistance > min) continue;
            //    min = currentDistance;
            //    intersectionPt = pt;
            //    normal = currentNormal;
            //    return true;
            //}


            //return false;


            //intersectionPt = Point3d.Unset;
            //normal = Vector3d.Unset;

            //var direction = motionLine.To - motionLine.From;
            //if (direction.Z >= 0) return false;


            //var meshBox = Mesh.CreateFromBox(_drawables.GetBoundingBoxTransformed(), 1, 1, 1);
            //var intersectionPts = Intersection.MeshLine(meshBox, motionLine);
            //var intersect = intersectionPts != null && intersectionPts.Length > 0;
            //if (intersect)
            //{
            //    var min = double.MaxValue;
            //    foreach (var pt in intersectionPts)
            //    {
            //        var currentDistance = motionLine.From.DistanceToSquared(pt);
            //        if (currentDistance > min) continue;
            //        min = currentDistance;
            //        intersectionPt = pt;
            //    }

            //}

            //if (meshBox.IsPointInside(motionLine.From, 0.001, false) || meshBox.IsPointInside(motionLine.To, 0.001, false))
            //{
            //    var upperPt = motionLine.From.Z > motionLine.To.Z ? motionLine.From : motionLine.To;
            //    var mpFrom = meshBox.ClosestMeshPoint(upperPt, 0);
            //    intersectionPt = mpFrom.Point;
            //    normal = meshBox.NormalAt(mpFrom);
            //    //Rhino.RhinoApp.WriteLine("point inside PAD");
            //    return true;
            //}

            //return intersect;
        }

        public bool ShouldBounceBall(Ball ball, out Point3d padPt, out Vector3d reboundDirection)
        {
            reboundDirection = Vector3d.Unset;
            if (Collide(ball.MotionLine, ball.BallRadius, out padPt, out var padNormal))
            {
                var padBox = _drawables.GetBoundingBoxTransformed();
                var factor = ((ball.BoundingBoxTransformed.Center.X - padBox.Min.X) / (padBox.Max.X - padBox.Min.X)).Clamp(0.1,0.9);

                reboundDirection = -Vector3d.XAxis;
                reboundDirection.Transform(Transform.Rotation(Math.PI * factor, Vector3d.YAxis, Point3d.Origin));
                return true;
            }

            return false;
        }

        public void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            _drawables?.ForEach(_ => _?.Draw(dp, ellapsedMs));
        }

        public void Reset()
        {
            _drawables?.ForEach(_ => _.Transform = Transform.Identity);
            SetScaleX(_factor);
        }

        public void PlayHitSound()
        {
            Sound.Play(_hitSound);
        }


    }
}
