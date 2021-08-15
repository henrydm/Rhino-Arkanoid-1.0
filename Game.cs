using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RhinoArkanoid
{
    public static class Game
    {
        public static bool Playing { get; private set; }
        public static EventHandler OnStopGame;
        private static double _currentFps;
        private static double _ballRadius = 2;
        private static Vector3d _ballDirection;
        private static double _padWidth = 40;
        private static double _padHeight = 5;
        private static double _padDepth = 5;

        private static double _blockWidth = 10;
        private static double _blockHeight = 5;
        private static double _blockDepth = 5;

        private static double _spaceBtwBlocksX = 0.2;
        private static double _spaceBtwBlocksZ = 0.2;

        private static double _ballSpeed = 160;
        private static double _padSpeed = 250;

        private static int columns = 13;
        private static int rows = 15;

        private static int MaxFPS = 30;
        private static double FrameRenderMillisecondsMax = 1000.0 / Convert.ToDouble(MaxFPS);

        private static BackgroundWorker _bw;
        private static Drawable _wall;
        private static Drawable _ball = new Drawable(Color.White, Mesh.CreateFromSphere(new Sphere(new Point3d(0, _blockDepth * 0.5, _padHeight + _ballRadius), _ballRadius), 10, 10));
        private static Drawable _pad = new Drawable(Color.Blue, Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(_padWidth, _padHeight, _padDepth)), 1, 1, 1));
        private static List<Drawable> _blocks;


        public static void Run()
        {
            if (Playing) return;
            Playing = true;

            KeyBoard.OnKeyDown += KeyBoard_OnKeyDown;
            KeyBoard.OnKeyUp += KeyBoard_OnKeyDown;
            KeyBoard.Start();
            Rhino.Display.DisplayPipeline.PostDrawObjects += DisplayPipeline_PostDrawObjects;

            _bw = new BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
            _bw.RunWorkerAsync();
        }

        private static void KeyBoard_OnKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = e.KeyCode == Keys.Left || e.KeyCode == Keys.Right;
        }

        public static void Stop()
        {
            if (!Playing) return;
            Playing = false;
            KeyBoard.OnKeyDown -= KeyBoard_OnKeyDown;
            KeyBoard.OnKeyUp -= KeyBoard_OnKeyDown;
            KeyBoard.Stop();
            Rhino.Display.DisplayPipeline.PostDrawObjects -= DisplayPipeline_PostDrawObjects;

        }


        private static bool _ballDirMod;
        private static void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();
            var elapsedMilliseconds = 0L;

            CreateBlocks(Resources.PantherLogo);

            Reset();
            MaxFPS = 60;
            FrameRenderMillisecondsMax = 1000.0 / Convert.ToDouble(MaxFPS);

            var lastPtBallOutside = _ball.AABB.Center;
            var lastPtBallPosition = lastPtBallOutside;
            var ptList = new List<Guid>();
            while (Playing)
            {
                sw.Restart();

               

                var ptBall = _ball.AABB.Center;
                var wallMesh = _wall.GetMesh();
                var padMesh = _pad.GetMesh();
                var ballMotionLine = new Line(ptBall, lastPtBallOutside);
                var wallBallCollision = _wall.Collide(ballMotionLine);

                //var wallIntersections = Rhino.Geometry.Intersect.Intersection.MeshLine(wallMesh, ballMotionLine);
                //Avoid out of limits postions
    
                //Store ball position if it doesn't get in wall mesh
                if (!wallMesh.IsPointInside(ptBall, 0.0001, false) && _wall.AABB.Contains(ptBall) && !wallBallCollision)
                {
                    lastPtBallOutside = ptBall;
                }





                ptList.Add(RhinoDoc.ActiveDoc.Objects.AddPoint(ptBall));
                if (ptList.Count > 100)
                {
                    RhinoDoc.ActiveDoc.Objects.Delete(ptList[0], true);
                    ptList.RemoveAt(0);
                }


                //Check Pad movement
                if (KeyBoard.PressedKey == Keys.Left)
                {
                    var min = ((_blockWidth + _spaceBtwBlocksX) * columns - _spaceBtwBlocksX) * 0.5;
                    if (_pad.AABB.Min.X > -min) _pad.Transform *= Transform.Translation(-Vector3d.XAxis * elapsedMilliseconds / 1000 * _padSpeed);
                    _ballDirMod = false;
                }
                else if (KeyBoard.PressedKey == Keys.Right)
                {
                    var min = ((_blockWidth + _spaceBtwBlocksX) * columns - _spaceBtwBlocksX) * 0.5;
                    if (_pad.AABB.Max.X < min) _pad.Transform *= Transform.Translation(Vector3d.XAxis * elapsedMilliseconds / 1000 * _padSpeed);
                    _ballDirMod = false;
                }

                //Check ball-wall collisions
                //if (wallMesh.ClosestPoint(ptBall, out var pOnWall, out var wallNormal, _ballRadius + _ballRadius * 0.2) != -1)
                if (wallBallCollision)
                {

                     //RhinoDoc.ActiveDoc.Objects.AddLine(new Line(pOnWall, _ballDirection * 10), new Rhino.DocObjects.ObjectAttributes { ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = Color.DarkMagenta });
                   // if (!_ballDirMod)
                    {
                        wallMesh.ClosestPoint(lastPtBallOutside, out var pOnWall, out var wallNormal,0.0);
                        _ball.Transform *= Transform.Translation(lastPtBallOutside - ptBall);
                        _ballDirection = _ballDirection.GetReflected(pOnWall, wallNormal);

                    }

                    _ballDirMod = true;
                    //var ballDirection =  ptBall - pOnWall;
                    //var length = ballDirection.Length + _ballRadius;
                    //ballDirection.Unitize();
                    //_ball.Transform *= Transform.Translation(ballDirection * length);

                    //RhinoDoc.ActiveDoc.Objects.Add(_wall.GetMesh());
                    //  RhinoDoc.ActiveDoc.Objects.AddPoint(pOnWall, new Rhino.DocObjects.ObjectAttributes { ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = Color.Pink });
                    // RhinoDoc.ActiveDoc.Objects.AddPoint(pOnWall);
                    //  RhinoDoc.ActiveDoc.Objects.AddLine(new Line(pOnWall, _ballDirection * 10));
                    //  RhinoDoc.ActiveDoc.Objects.AddLine(new Line(pOnWall, wallNormal * 10), new Rhino.DocObjects.ObjectAttributes { ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject, ObjectColor = Color.Pink });
                }
                //Check ball-pad collisions
                else if (padMesh.ClosestPoint(ptBall, out var pOnPad, out var padNormal, _ballRadius) != -1)
                {
                    var ballAbsX = ptBall.X - _pad.AABB.Min.X;

                    var factor = ballAbsX / _padWidth;
                    if (factor < 0.1) factor = 0.1;
                    if (factor > 0.9) factor = 0.9;

                    _ballDirection = -Vector3d.XAxis;
                    _ballDirection.Transform(Transform.Rotation(Math.PI * factor, Vector3d.YAxis, Point3d.Origin));
                    _ballDirMod = false;
                }
                //Check Ball lost
                else if (_ball.AABB.Center.Z < 0)
                {
                    Reset();
                }
                //Check bricks-ball collision
                else
                {
                    _ballDirMod = false;
                    foreach (var block in _blocks)
                    {
                        if (block.GetMesh().ClosestPoint(ptBall, out var pOnBlock, out var blockNormal, _ballRadius) != -1)
                        {
                            _ball.Transform *= Transform.Translation(lastPtBallOutside - ptBall);
                            _ballDirection = _ballDirection.GetReflected(pOnBlock, blockNormal);
                            _blocks.Remove(block);

                            //Level Finish
                            if (_blocks.Count == 0)
                            {
                                CreateBlocks(Resources.PantherLogo);
                                Reset();
                            }

                            break;
                        }
                    }
                }

                //Translate ball
                _ball.Transform *= Transform.Translation(_ballDirection * elapsedMilliseconds / 1000 * _ballSpeed);
                lastPtBallPosition = _ball.AABB.Center;

                //if (!_isDrawing)
                RhinoDoc.ActiveDoc.Views.Redraw();

                if (sw.ElapsedMilliseconds < FrameRenderMillisecondsMax)
                {
                    Thread.Sleep(Convert.ToInt32(FrameRenderMillisecondsMax - sw.ElapsedMilliseconds));
                }


                elapsedMilliseconds = sw.ElapsedMilliseconds;
                _currentFps = 1000.0 / elapsedMilliseconds;
     


            }
        }

        public static void Reset()
        {
            _ballDirection = new Vector3d(0.5, 0, 0.5);
            _ball.Transform = Transform.Identity;
            _pad.Transform = Transform.Identity;
        }

        private static void CreateBlocks(Bitmap bm)
        {
            _blocks = new List<Drawable>();
            _wall = null;
            if (bm == null) return;

            var xBound = ((_blockWidth + _spaceBtwBlocksX) * columns - _spaceBtwBlocksX);
            var zBound = ((_blockHeight + _spaceBtwBlocksZ) * rows - _spaceBtwBlocksZ);
            var blocksMinY = 100;

            //Create bricks
            var centerTx = Transform.Translation(new Vector3d(xBound * -0.5, 0, blocksMinY));
            var scaledBm = new Bitmap(bm, columns, rows);
            for (int x = 0; x < scaledBm.Width; x++)
                for (int y = 0; y < scaledBm.Height; y++)
                    _blocks.Add(new Drawable(scaledBm.GetPixel(x, scaledBm.Height - 1 - y), Mesh.CreateFromBox(new BoundingBox(Point3d.Origin, new Point3d(_blockWidth, _blockHeight, _blockDepth)), 1, 1, 1)) { Transform = Transform.Translation((_blockWidth + _spaceBtwBlocksX) * x, 0, (_blockHeight + _spaceBtwBlocksZ) * y) * centerTx });

            //Create walls
            var wallWidth = xBound * 0.1;
            var wallPadding = 0.1;
            var wallBigBox = new BoundingBox(Point3d.Origin, new Point3d(xBound + wallPadding * 2 + wallWidth * 2, _blockDepth, zBound + wallPadding + wallWidth + blocksMinY));
            var wallSmallBox = new BoundingBox(Point3d.Origin, new Point3d(xBound + wallPadding * 2, _blockDepth, zBound + wallPadding + blocksMinY));
            wallSmallBox.Transform(Transform.Translation(wallBigBox.Center.X - wallSmallBox.Center.X, 0, 0));

            var wallBrep = Brep.CreateBooleanDifference(wallBigBox.ToBrep(), wallSmallBox.ToBrep(), 0.001);
            if (wallBrep != null && wallBrep.Length > 0) _wall = new Drawable(Color.Blue, Mesh.CreateFromBrep(wallBrep[0], MeshingParameters.Default)) { Transform = Transform.Translation(wallBigBox.Max.X * -0.5, 0, 0) };
        }

        private static bool _isDrawing;
        private static void DisplayPipeline_PostDrawObjects(object sender, Rhino.Display.DrawEventArgs e)
        {
            if (_blocks == null) return;
            _isDrawing = true;
            try
            {
                foreach (var block in _blocks)
                {
                    block?.Draw(e.Display);
                }

                _pad?.Draw(e.Display);
                _ball?.Draw(e.Display);
                _wall?.Draw(e.Display);

                e.Display.Draw2dText(Math.Round(_currentFps, 0).ToString(), Color.Black, new Point2d(30, 30), false);
            }
            finally
            {
                _isDrawing = false;
            }


        }

    }
}
