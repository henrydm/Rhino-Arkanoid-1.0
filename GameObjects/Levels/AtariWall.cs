using Rhino.Geometry;
using System.Collections.Generic;

namespace RhinoArkanoid.GameObjects.Levels
{
    class AtariWall : Wall
    {
        private readonly double _wallTopZ;
        private readonly byte[] _topHitSound;
        private double _lastBallZ;

        public AtariWall(List<Drawable> limits, List<Drawable> forniture, byte[] hitSound, byte[] topHitSound,double topZ) : base(limits, forniture, hitSound)
        {
            _wallTopZ = topZ;
            _topHitSound = topHitSound;
        }

        public override bool Collide(Line motionLine, double ballRadius, out Point3d intersectionPt, out Vector3d normal, out Drawable collisionObj)
        {
            _lastBallZ = motionLine.To.Z;
            return base.Collide(motionLine, ballRadius, out intersectionPt, out normal, out collisionObj);

        }

        public override void PlayHitSound()
        {
            Sound.Play(_lastBallZ < _wallTopZ ? _hitSound : _topHitSound);        
        }
    }
}
