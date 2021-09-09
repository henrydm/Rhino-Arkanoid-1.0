using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.Blocks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace RhinoArkanoid.GameObjects.Levels
{
    class Atari : Level
    {
        private const double _padWidth = 40;
        private const double _padHeight = 5;
        private const double _padDepth = 5;

        private const double _blockWidth = 10;
        private const double _blockHeight = 5;
        private const double _blockDepth = 5;

        private const double _spaceBtwBlocksX = 0;
        private const double _spaceBtwBlocksZ = 0;
        private const double _wallPadding = 0;

        private const double _ballSpeed = 160;
        private const double _padSpeed = 250;

        private const int columns = 20;
        private const int rows = 6;
        private const int blocksMinZ = 100;
        private const double xBound = (_blockWidth + _spaceBtwBlocksX) * columns - _spaceBtwBlocksX;

        private static double _ballRadius = _padHeight * 0.6;
        private static Dictionary<Interval, DisplayMaterial> _colorRanges;

        public Atari() : base("Atari Times",  null)
        {

        }

        public override List<Ball> CreateBalls()
        {
            return new List<Ball> { new Ball(Mesh.CreateFromBox(new BoundingBox(new Point3d(-_ballRadius, -_ballRadius, 0), new Point3d(_ballRadius, _ballRadius, _ballRadius*2)), 1, 1, 1), new DisplayMaterial(Color.FromArgb(197, 77, 65)), _ballSpeed) };
        }

        protected override List<Block> CreateBlocks()
        {
            var blocks = new List<Block>();
            var centerTx = Transform.Translation(new Vector3d(xBound * -0.5, _blockDepth * -0.5, blocksMinZ));
            var lineColors = new Color[] { Color.FromArgb(211, 85, 70), Color.FromArgb(208, 113, 56), Color.FromArgb(186, 122, 44), Color.FromArgb(164, 154, 36), Color.FromArgb(66, 164, 68), Color.FromArgb(63, 79, 205) };
            var sounds = new List<byte[]> { Resources.Breakout1Blue, Resources.Breakout2Green, Resources.Breakout3Yellow, Resources.Breakout4Brown, Resources.Breakout5Orange, Resources.Breakout6Red };
            _colorRanges = new Dictionary<Interval, DisplayMaterial>();

            for (int z = 0; z < lineColors.Length; z++)
            {
                var deltaZ = (_blockHeight + _spaceBtwBlocksZ) * z;
                _colorRanges.Add(new Interval(blocksMinZ + deltaZ - _blockHeight * 0.5, blocksMinZ + deltaZ + _blockHeight * 0.5), new DisplayMaterial(lineColors[z]));
                for (int x = 0; x < 20; x++)
                {
                    var mesh = Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(_blockWidth, _blockHeight, _blockDepth)), 1, 1, 1);
                    blocks.Add(new Brick(lineColors[z], mesh, sounds[z]) { Transform = Transform.Translation((_blockWidth + _spaceBtwBlocksX) * x, 0, deltaZ) * centerTx });
                }
            }

            _colorRanges.Add(new Interval(0, blocksMinZ), new DisplayMaterial(Color.FromArgb(197, 77, 65)));

            return blocks;
        }

        protected override Pad CreatePad()
        {
            var padMesh = Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(_padWidth, _padHeight, _padDepth)), 1, 1, 1);
            padMesh.Transform(Transform.Translation(_padWidth * -0.5, _padHeight * -0.5, 0));
            return new Pad(new List<Drawable> { new Drawable(padMesh, new DisplayMaterial(Color.FromArgb(197, 77, 65))) }, _padSpeed, Resources.BreakoutPad);
        }

        protected override Wall CreateWall()
        {
            var zBound = (_blockHeight + _spaceBtwBlocksZ) * rows - _spaceBtwBlocksZ + _blockHeight * 4;
            var wallWidth = xBound * 0.08;

            var wallBigBox = new BoundingBox(Point3d.Origin, new Point3d(xBound + _wallPadding * 2 + wallWidth * 2, _blockDepth, zBound + _wallPadding + wallWidth + blocksMinZ));
            var wallSmallBox = new BoundingBox(Point3d.Origin, new Point3d(xBound + _wallPadding * 2, _blockDepth, zBound + _wallPadding + blocksMinZ));
            wallSmallBox.Transform(Transform.Translation(wallBigBox.Center.X - wallSmallBox.Center.X, 0, 0));

            var wallBrep = Brep.CreateBooleanDifference(wallBigBox.ToBrep(), wallSmallBox.ToBrep(), 0.001);
            if (wallBrep == null || wallBrep.Length == 0) throw new Exception($"Level {Title} wall limits cannot be created");

            var meshes = Mesh.CreateFromBrep(wallBrep[0], MeshingParameters.Default);

            var fullMesh = new Mesh();
            foreach (var mesh in meshes)
            {
                fullMesh.Append(mesh);
            }

            var wallLimits = new Drawable(fullMesh, new DisplayMaterial(Color.FromArgb(126, 126, 126))) { Transform = Transform.Translation(wallBigBox.Max.X * -0.5, wallBigBox.Max.Y * -0.5, 0) };

            //Background
            var meshBackground = new Mesh();
            meshBackground.Vertices.Add(new Point3d(wallBigBox.Min.X, wallBigBox.Max.Y, wallBigBox.Min.Z));
            meshBackground.Vertices.Add(new Point3d(wallBigBox.Min.X, wallBigBox.Max.Y, wallBigBox.Max.Z));
            meshBackground.Vertices.Add(new Point3d(wallBigBox.Max.X, wallBigBox.Max.Y, wallBigBox.Max.Z));
            meshBackground.Vertices.Add(new Point3d(wallBigBox.Max.X, wallBigBox.Max.Y, wallBigBox.Min.Z));
            meshBackground.Faces.AddFace(0, 1, 2, 3);
            meshBackground.Transform(Transform.Translation((wallBigBox.Max.X - wallBigBox.Min.X) * -0.5, 0, 0));

            var ornaments = new Drawable(meshBackground, new DisplayMaterial(Color.Black));

            return new AtariWall(new List<Drawable> { wallLimits }, new List<Drawable> { ornaments }, Resources.BreakoutWall, Resources.BreakoutPad, wallSmallBox.Max.Z - 2);
        }

        public override void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            foreach (var ball in Balls)
            {
                foreach (var kvp in _colorRanges)
                {
                    if (!kvp.Key.IncludesParameter(ball.MotionLine.To.Z)) continue;
                    ball._material = kvp.Value;
                    break;
                }
            }

            base.Draw(dp, ellapsedMs);

            //Score
            //dp.Draw3dText("1234",Color.Gray, Plane.WorldXY, 20, "Miriam Mono CLM");

        }

    }
}
