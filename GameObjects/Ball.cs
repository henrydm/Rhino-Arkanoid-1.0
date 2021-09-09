using Rhino.Display;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.Blocks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RhinoArkanoid.GameObjects
{
    class Ball : Drawable
    {
        private double _ballSpeedOriginal;
        private double _ballSpeed;
        public double BallRadius { get; }
        protected Vector3d _ballDirection;
        private Point3d _lastNonCollisionPoint;
        public Line MotionLine;
        public bool StickOnPad { get; set; }
        public List<Drawable> Ornaments;

        public Ball(Mesh mesh, DisplayMaterial material, double ballSpeed) : base(mesh, material)
        {
            _ballSpeed = ballSpeed;
            _ballSpeedOriginal = ballSpeed;
            _ballDirection = new Vector3d(0.5, 0, 0.7);

            var box = mesh.GetBoundingBox(true);
            var boxX = box.Max.X - box.Min.X;
            var boxY = box.Max.Y - box.Min.Y;
            BallRadius = Math.Max(boxX, boxY) * 0.5;
            StickOnPad = true;
        }

        public void Reset()
        {
            Transform = Transform.Identity;
            _ballDirection = _ballDirection = new Vector3d(0.5, 0, 0.5);
        }

        static bool _ballShot;

        Block _lastCollisionBlock;
        public CollisionResult ProcessFrame(double ellapsedMs, Wall wall, Pad pad, List<Block> blocks,bool stickyBall)
        {
            //Ball Sticked
            if (StickOnPad)
            {
                var padBox = pad.BoundingBox;
                Transform = Transform.Translation(new Point3d(padBox.Center.X, 0, padBox.Max.Z) - BoundingBoxOriginal.Center);

                //Shoot ball
                if (KeyBoard.PressedKey.Contains(Keys.Space))
                {
                    if (!_ballShot) StickOnPad = false;
                    _ballShot = true;
                }
                else
                {
                    _ballShot = false;
                }

                return new CollisionResult(CollisionResult.ResultType.None, null);

            }

            //Create ball movement
            var oldPos = BoundingBoxTransformed.Center;
            var tx = Transform.Translation(_ballDirection * ellapsedMs / 1000 * _ballSpeed);
            var nextPt = new Point3d(BoundingBoxTransformed.Center);
            nextPt.Transform(tx);
            var motionLine = new Line(BoundingBoxTransformed.Center, nextPt);

            //Wall Collision
            if (wall.Collide(motionLine, BallRadius, out var wallPt, out var wallNormal, out var collisionObj))
            {
                wallPt.Transform(Transform.Translation(_ballDirection * -0.01));
                _ballDirection = _ballDirection.GetReflected(wallPt, wallNormal);
                Transform = Transform.Translation(_lastNonCollisionPoint - BoundingBoxOriginal.Center);
                MotionLine = new Line(oldPos, BoundingBoxTransformed.Center);
                return new CollisionResult(CollisionResult.ResultType.Wall, collisionObj);
            }

            //Pad Collision
            if (pad.ShouldBounceBall(this, out var padPt, out var reboundDirection))
            {
                //padPt.Z = pad.MaxZ+BallRadius;
                //Transform = Transform.Translation(padPt - BoundingBoxOriginal.Center);
                _ballDirection = reboundDirection;
                MotionLine = new Line(oldPos, BoundingBoxTransformed.Center);
                if (stickyBall) StickOnPad = true;
                return new CollisionResult(CollisionResult.ResultType.Pad, null);
            }

            //Blocks Collision
            var collisionBlockMinDist = double.MaxValue;
            Block collisionBlock = null;
            var collisionBlockPt = Point3d.Unset;
            var collisionBlockNormal = Vector3d.Unset;
            foreach (var currentBlock in blocks)
            {
                if (!currentBlock.Collide(motionLine, BallRadius, out var currentBlockPt, out var currentBlockNormal)) continue;
                var currentDistance = motionLine.From.DistanceToSquared(currentBlockPt);
                if (currentDistance > collisionBlockMinDist) continue;
                collisionBlockMinDist = currentDistance;
                collisionBlock = currentBlock;
                collisionBlockPt = currentBlockPt;
                collisionBlockNormal = currentBlockNormal;
            }
            if (collisionBlock != null)// && collisionBlock != _lastCollisionBlock)
            {

                Transform = Transform.Translation(_lastNonCollisionPoint - BoundingBoxOriginal.Center);
                MotionLine = new Line(oldPos, BoundingBoxTransformed.Center);
                _ballDirection = _ballDirection.GetReflected(collisionBlockPt, collisionBlockNormal);

                // _ballDirection = _ballDirection.GetReflected(collisionBlockPt, collisionBlockNormal);
                // Transform = Transform.Translation(collisionBlockPt - BoundingBoxOriginal.Center);
                return new CollisionResult(CollisionResult.ResultType.Block, collisionBlock);
            }
            else
            {

            }
            //_lastCollisionBlock = collisionBlock;

            //Loose Ball
            if (motionLine.To.Z < 0)
            {
                return new CollisionResult(CollisionResult.ResultType.Loose, null);
            }


            _lastNonCollisionPoint = BoundingBoxTransformed.Center;

            //Move
            Transform *= tx;
            MotionLine = new Line(oldPos, BoundingBoxTransformed.Center);
            return new CollisionResult(CollisionResult.ResultType.None, null);



            //Add Doc Objects
            // Rhino.RhinoDoc.ActiveDoc.Objects.AddPoint(MotionLine.From);
            // Rhino.RhinoDoc.ActiveDoc.Objects.AddLine(MotionLine);
        }

        public override void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            base.Draw(dp, ellapsedMs);
            Ornaments?.ForEach(_ => _.Draw(dp, ellapsedMs));
        }

    }
}
