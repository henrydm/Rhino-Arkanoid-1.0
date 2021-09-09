using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace RhinoArkanoid.GameObjects.AnimationObjects
{
    class Rotator : IAnimator
    {
        double _speed;
        Vector3d _axis;
        Point3d _normalizedCoords;
        public Point3d boxPt;
        public Rotator(double speed, Vector3d axis, double normalizedX, double normalizedY, double normalizedZ)
        {
            _speed = speed;
            _axis = axis;
            _normalizedCoords = new Point3d(normalizedX, normalizedY, normalizedZ);

        }
        public void ProcessFrame(List<Drawable> drawables, double ellapsedMs)
        {
            if (boxPt.IsValid)
            {
                boxPt = drawables.GetBoundingBoxTransformed().GetNormalizedPt(_normalizedCoords.X, _normalizedCoords.Y, _normalizedCoords.Z);
            }

            // Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(boxPt);
            var tx = Transform.Rotation(_speed * (ellapsedMs / 1000), _axis, boxPt);
            drawables?.ForEach(_ => _.Transform *= tx);

        }
    }
}
