using Rhino.Display;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.Blocks;
using RhinoArkanoid.GameObjects.PowerUps;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace RhinoArkanoid.GameObjects.Levels
{
    class AnubisTemple : Level
    {
        protected override int MaxPowerUps => 3;
        private const double _ballRadius = 2;
        private const double _ballSpeed = 110;
        private const double _padSpeed = 250;
        private const double _lifeLegendSpace = 2;

        private List<Drawable> _scoreBackground;
        private List<Drawable> _livesDrawables;
        private List<GameObjBuildInfo> _lifeObjInfo;

        private Point3d _lifeInitPosition;
        private double _lifeLegendWidth;

        private List<Point3d> _powerUpLegendPositions;
        private double _powerUpLegendWidth;

        public AnubisTemple() : base("Anubis Temple", Resources.AnubisMusic)
        {
            //Score Board
            _scoreBackground = GetObjects("Score", "Ornaments")?.ToDrawable();
            _lifeObjInfo = GetObjects("Score", "Life");
            var scoreBox = _scoreBackground.GetBoundingBoxTransformed();
            var lifeBox = _lifeObjInfo.GetBoundingBoxTransformed();

            _lifeLegendWidth = lifeBox.Max.X - lifeBox.Min.X;
            _lifeInitPosition = scoreBox.GetNormalizedPt( 0.1, 0.75, 0.8);
            //_lifeLegendSpace = _lifeLegendWidth * 0.15;

            _powerUpLegendPositions = new List<Point3d> { scoreBox.GetNormalizedPt(0.1, 0.75, 0.2) };

            var space = _powerUpLegendWidth + _powerUpLegendWidth * 0.15;
            while (_powerUpLegendPositions.Count < MaxPowerUps)
            {
                var lastPt = _powerUpLegendPositions[_powerUpLegendPositions.Count - 1];
                _powerUpLegendPositions.Add(new Point3d(lastPt) { X = lastPt.X + space });

            }
        }


       

        public override List<Ball> CreateBalls()
        {
            return new List<Ball> { new Ball(Mesh.CreateFromSphere(new Sphere(new Point3d(0, 0, _ballRadius), _ballRadius), 10, 10), new DisplayMaterial(Color.IndianRed) { Shine = 0.8 }, _ballSpeed) };
        }

        protected override List<Block> CreateBlocks()
        {
            var sceneObjs = GetScenarioObjects(DrawableType.Blocks);
            var blocks = new List<Block>();
            foreach (var info in sceneObjs)
            {
                var block = new Gem(info.Mesh, info.Material, info.GetAttribute<int>("hits", out var hitsAtt) ? hitsAtt : 1, Resources.clin1, Resources.blade);

                if (info.GetAttribute<string>("powerup", out var powerUpTypeStr))
                {
                    if (Enum.TryParse<PowerType>(powerUpTypeStr, true, out var powerUpType) && _powerUpInfo.TryGetValue(powerUpType, out var gameObjInfo))
                    {
                        var powerUpDrawables = gameObjInfo.ToDrawable();
                        var powerUpBox = powerUpDrawables.GetBoundingBoxTransformed();
                        powerUpDrawables.ForEach(_ => _.Transform = Transform.Translation(block.BoundingBoxTransformed.Center - powerUpBox.Center));

                        block.PowerUp = PowerUp.Create(powerUpDrawables,powerUpType);
                        block.PowerUp._animators.Add(new AnimationObjects.Rotator(0.2, Vector3d.XAxis, 0.5, 0.5, 0.5));

                        //Set power up size
                        if (_powerUpLegendWidth == 0)
                            _powerUpLegendWidth = powerUpBox.Max.X - powerUpBox.Min.X;
                    }
                    else
                    {
                        Rhino.RhinoApp.WriteLine($"Error parsing block power up: {powerUpTypeStr} type doesn't exist.");
                    }

                }

                blocks.Add(block);

            }
            return blocks;
        }

        protected override Pad CreatePad()
        {
            var drawables = GetScenarioDrawables(DrawableType.Pads);
            return new Pad(drawables, _padSpeed,null);
        }

        protected override Wall CreateWall()
        {
            GetWallObjects(out var limits, out var ornaments, out var animatedColliders);
            return new Wall(limits.GetDrawables(), ornaments.GetDrawables(),null);

        }

        protected override void ActivatePowerUp(PowerUp powerUp)
        {
            base.ActivatePowerUp(powerUp);

            for (int i =0; i<_activePowerUps.Count;i++)
            {
                _activePowerUps[i].AppendTx(Transform.Translation(_powerUpLegendPositions[i] - _activePowerUps[i].BoundingBoxTransformed.Center));
            }

        }
        protected override void DeactivatePowerUp(PowerUp powerUp)
        {
            base.DeactivatePowerUp(powerUp);

            for (int i = 0; i < _activePowerUps.Count; i++)
            {
                _activePowerUps[i].AppendTx(Transform.Translation(_powerUpLegendPositions[i] - _activePowerUps[i].BoundingBoxTransformed.Center));
            }
        }


        public override void SetLives(int livesAmount)
        {
            base.SetLives(livesAmount);
            _livesDrawables = new List<Drawable>(livesAmount);
            if (livesAmount < 1) return;

            var targetX = _lifeInitPosition.X;
            for (int i = 0; i < livesAmount; i++)
            {
                var drawables = _lifeObjInfo.ToDrawable();
                var origin = drawables.GetBoundingBoxTransformed().Center;
                var target = new Point3d(targetX, _lifeInitPosition.Y, _lifeInitPosition.Z);
                drawables.ForEach(_ => _.Transform = Transform.Translation(target - origin));
                _livesDrawables.AddRange(drawables);
                targetX += _lifeLegendWidth + _lifeLegendSpace;
            }

        }

        public override void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            base.Draw(dp, ellapsedMs);
            _scoreBackground?.ForEach(_ => _.Draw(dp, ellapsedMs));
            _activePowerUps?.ForEach(_ => _.Draw(dp, ellapsedMs));  
            _livesDrawables?.ForEach(_ => _.Draw(dp, ellapsedMs));
        }
    }
}
