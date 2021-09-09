using Rhino.Display;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.Blocks;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace RhinoArkanoid.GameObjects
{
    class CustomImage : Level
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

        private const int columns = 13;
        private const int rows = 15;
        private const int blocksMinY = 100;
        private const double xBound = ((_blockWidth + _spaceBtwBlocksX) * columns - _spaceBtwBlocksX);

        private static double _ballRadius = 2;

        public CustomImage() : base("Atari Times", null)
        {

        }

        public override List<Ball> CreateBalls()
        {
            return new List<Ball> { new Ball(Mesh.CreateFromSphere(new Sphere(new Point3d(0, 0, _padHeight + _ballRadius), _ballRadius), 10, 10), new DisplayMaterial(Color.White), _ballSpeed) };
        }

        protected override List<Block> CreateBlocks()
        {
            var blocks = new List<Block>();
            var centerTx = Transform.Translation(new Vector3d(xBound * -0.5, _blockDepth*-0.5, blocksMinY));
            var scaledBm = new Bitmap(Resources.PantherLogo, columns, rows);

            for (int x = 0; x < scaledBm.Width; x++)
                for (int y = 0; y < scaledBm.Height; y++)
                    blocks.Add(item: new Brick(scaledBm.GetPixel(x, scaledBm.Height - 1 - y), Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(_blockWidth, _blockHeight, _blockDepth)), 1, 1, 1)) { Transform = Transform.Translation((_blockWidth + _spaceBtwBlocksX) * x, 0, (_blockHeight + _spaceBtwBlocksZ) * y) * centerTx });

            return blocks;
        }

        protected override Pad CreatePad()
        {
            var padMesh = Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(_padWidth, _padHeight, _padDepth)), 1, 1, 1);
            padMesh.Transform(Transform.Translation(_padWidth*-0.5, _padHeight*-0.5, 0));
            return new Pad(new List<Drawable> { new Drawable(padMesh, new DisplayMaterial(Color.Blue)) }, _padSpeed,null);
        }

        protected override Wall CreateWall()
        {
            var zBound = ((_blockHeight + _spaceBtwBlocksZ) * rows - _spaceBtwBlocksZ);
            var wallWidth = xBound * 0.1;
         
            var wallBigBox = new BoundingBox(Point3d.Origin, new Point3d(xBound + _wallPadding * 2 + wallWidth * 2, _blockDepth, zBound + _wallPadding + wallWidth + blocksMinY));
            var wallSmallBox = new BoundingBox(Point3d.Origin, new Point3d(xBound + _wallPadding * 2, _blockDepth, zBound + _wallPadding + blocksMinY));
            wallSmallBox.Transform(Transform.Translation(wallBigBox.Center.X - wallSmallBox.Center.X, 0, 0));

            var wallBrep = Brep.CreateBooleanDifference(wallBigBox.ToBrep(), wallSmallBox.ToBrep(), 0.001);
            if (wallBrep == null || wallBrep.Length == 0) throw new Exception($"Level {Title} wall limits cannot be created");

            var meshes = Mesh.CreateFromBrep(wallBrep[0], MeshingParameters.Default);

            var fullMesh = new Mesh();
            foreach (var mesh in meshes)
            {
                fullMesh.Append(mesh);
            }

            var wallLimits = new Drawable(fullMesh, new DisplayMaterial(Color.Blue)) { Transform = Transform.Translation(wallBigBox.Max.X * -0.5, wallBigBox.Max.Y * -0.5, 0) };

            return new Wall(new List<Drawable> { wallLimits },null,null);
        }

    }
}
