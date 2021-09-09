using Rhino.Display;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.PowerUps;
using System.Collections.Generic;

namespace RhinoArkanoid.GameObjects.Blocks
{
    class Block : Drawable
    {  
        public List<Drawable> Ornaments;
        public PowerUp PowerUp;
        private byte[] _hitSound;
        private byte[] _lastHitSound;

        protected int InitialRemainingHits { get; }
        public int RemainingHits { get; set; }


        public Block(Mesh mesh, DisplayMaterial material, int hits, byte[] hitSound, byte[] lastHitSound) : base(mesh, material)
        {
            InitialRemainingHits = hits;
            RemainingHits = hits;
            _hitSound = hitSound;
            _lastHitSound = lastHitSound;
        }

        public void PlayHitSound()
        {
            Sound.Play(RemainingHits == 0 ? _lastHitSound : _hitSound);
        }

        public override void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            base.Draw(dp, ellapsedMs);
            Ornaments?.ForEach(_ => _.Draw(dp, ellapsedMs));
        }
    }
}
