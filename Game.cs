using Rhino;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects;
using RhinoArkanoid.GameObjects.Levels;
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
        private static Level _currentLevel;
        public static bool Playing { get; private set; }
        public static EventHandler OnStopGame;
        private static double _currentFps;
        private static DateTime _lastDraw;
        private static int MaxFPS = 100;
        private static double FrameRenderMillisecondsMax = 1000.0 / (double)MaxFPS;

        private static BackgroundWorker _bw;

        public static void Run()
        {
            if (Playing) return;
            Playing = true;

            KeyBoard.OnKeyDown += KeyBoard_OnKeyDown;
            KeyBoard.OnKeyUp += KeyBoard_OnKeyDown;
            KeyBoard.Start();
            Rhino.Display.DisplayPipeline.PostDrawObjects += DisplayPipeline_PostDrawObjects;
            Rhino.Display.DisplayPipeline.CalculateBoundingBox += DisplayPipeline_CalculateBoundingBox;
            Rhino.Display.DisplayPipeline.CalculateBoundingBoxZoomExtents += DisplayPipeline_CalculateBoundingBox;

            _bw = new BackgroundWorker();
            _bw.DoWork += _bw_DoWork;
            _bw.RunWorkerAsync();
        }

        private static void KeyBoard_OnKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Space;
        }

        public static void Stop()
        {
            if (!Playing) return;
            Playing = false;
            KeyBoard.OnKeyDown -= KeyBoard_OnKeyDown;
            KeyBoard.OnKeyUp -= KeyBoard_OnKeyDown;
            KeyBoard.Stop();
            Sound.StopBwMusic();
            Rhino.Display.DisplayPipeline.PostDrawObjects -= DisplayPipeline_PostDrawObjects;
            Rhino.Display.DisplayPipeline.CalculateBoundingBox += DisplayPipeline_CalculateBoundingBox;
            Rhino.Display.DisplayPipeline.CalculateBoundingBoxZoomExtents += DisplayPipeline_CalculateBoundingBox;
        }


        private static void _bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();
            var elapsedMilliseconds = 0L;

            //_currentLevel = new CustomImage();
            _currentLevel = new AnubisTemple();
            //_currentLevel = new Atari(); 
            //_currentLevel = new SuperMario(); 

            Reset();
             MaxFPS = 200;
            FrameRenderMillisecondsMax = 1000.0 / Convert.ToDouble(MaxFPS);

            while (Playing)
            {
                sw.Restart();

                _currentLevel.ProcessFrame(elapsedMilliseconds);

                // if (!_isDrawing)
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
            _currentLevel?.Reset(true);
        }


        private static bool _isDrawing;

        private static void DisplayPipeline_CalculateBoundingBox(object sender, Rhino.Display.CalculateBoundingBoxEventArgs e)
        {
            if (_currentLevel == null) return;
            e.IncludeBoundingBox(_currentLevel.BoundingBox);
        }

        private static void DisplayPipeline_PostDrawObjects(object sender, Rhino.Display.DrawEventArgs e)
        {
            var ellapsedMs = (DateTime.Now - _lastDraw).TotalMilliseconds;
            _lastDraw = DateTime.Now;
            e.Display.Draw2dText(Math.Round(_currentFps, 0).ToString(), Color.Black, new Point2d(30, 30), false, 20);

            _currentLevel?.Draw(e.Display, ellapsedMs);
        }

    }
}
