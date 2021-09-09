using Rhino.Display;
using Rhino.Geometry;
using System;

namespace RhinoArkanoid.GameObjects.Blocks
{
    class Gem : Block
    {
        private double _speed = 0.07;
        Vector3d _translationVector;

        public Gem(Mesh mesh, DisplayMaterial material, int hits, byte[] hitSound, byte[] lastHitSound) : base(mesh, material, hits, hitSound, lastHitSound)
        {
            var texture = _material.GetEnvironmentTexture(true);
            if (texture == null) return;
            texture.ApplyUvwTransform = true;

            var rnd = new Random(GetHashCode());
            _translationVector = new Vector3d(rnd.NextDouble().ClampMin(0.01), rnd.NextDouble().ClampMin(0.01), 0);
            _translationVector.Unitize();
        }

        public override void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            if (ellapsedMs > 10000) return;
            var texture = _material.GetEnvironmentTexture(true);
            if (texture == null) return;

            texture.UvwTransform *= Transform.Translation(_translationVector * _speed * ellapsedMs / 1000.0);
            texture.UvwTransform *= Transform.Rotation(_speed * ellapsedMs / 1000.0, Point3d.Origin);

            _material.Transparency = 1 - (RemainingHits / (double)InitialRemainingHits);

            _material.SetEnvironmentTexture(texture, true);

            base.Draw(dp, ellapsedMs);
        }

    }
}
