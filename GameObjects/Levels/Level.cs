using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.Blocks;
using RhinoArkanoid.GameObjects.PowerUps;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects
{
    internal abstract class Level
    {
        protected virtual int MaxPowerUps => 0;

        public enum DrawableType { Wall, Pads, Blocks, Ornaments,Ball };

        protected static readonly File3dm _f3dm = File3dm.FromByteArray(Resources.Arkanoid);

        public BoundingBox BoundingBox => _wall.LimitsBox;

        public int Lives { get; private set; }
        private readonly int _initialLives;
        public string Title { get; }

        private byte[] _music { get; }

        public List<Ball> Balls;
        List<Block> _blocks;

        /// <summary>
        /// Active power ups (falling down)
        /// </summary>
        private List<PowerUp> _fallingPowerUps;
        protected List<PowerUp> _activePowerUps;
        Wall _wall;
        public Pad Pad;
        protected readonly Dictionary<PowerType, List<GameObjBuildInfo>> _powerUpInfo;

        public Level(string title, byte[] music)
        {
            Title = title;
         
            _fallingPowerUps = new List<PowerUp>();
            _activePowerUps = new List<PowerUp>();
            _powerUpInfo = GetPowerUps();
            _blocks = CreateBlocks();
            _wall = CreateWall();
            Pad = CreatePad();
            Balls = CreateBalls();
            _music = music;


            _initialLives = 5;
            SetLives(_initialLives);
        }

        protected abstract List<Block> CreateBlocks();
        protected abstract Wall CreateWall();
        protected abstract Pad CreatePad();

        public virtual void SetLives(int livesAmount)
        {
            Lives = livesAmount.ClampMin(0);
        }

        /// <summary>
        /// Create initial balls
        /// </summary>
        /// <returns></returns>
        public abstract List<Ball> CreateBalls();


        protected void GetWallObjects(out List<GameObjBuildInfo> limits, out List<GameObjBuildInfo> ornaments, out List<GameObjBuildInfo> animatedCollides)
        {
            limits = _f3dm?.GetObjects(Title, "Wall", "Colliders");
            ornaments = _f3dm?.GetObjects(Title, "Wall", "Ornaments");
            animatedCollides = new List<GameObjBuildInfo>();
        }

        protected Dictionary<PowerType, List<GameObjBuildInfo>> GetPowerUps()
        {
            var powerUps = new Dictionary<PowerType, List<GameObjBuildInfo>>();
            var powerTypeLayers = _f3dm?.GetSubLayers(_f3dm?.GetLayer(Title, "PowerUps"));

            foreach (var powerUpLayer in powerTypeLayers)
            {
                if (!Enum.TryParse<PowerType>(powerUpLayer.Name, true, out var powerUpType))
                {
                    Rhino.RhinoApp.WriteLine($"Power up layer: {powerUpLayer.Name} doesn't have any powerUp type assigned.");
                    continue;
                }
                if (powerUps.ContainsKey(powerUpType))
                {
                    continue;
                }

                powerUps.Add(powerUpType, new List<GameObjBuildInfo>());
                _f3dm?.Objects?.FindByLayer(powerUpLayer)?.ToList()?.ForEach(_ => powerUps[powerUpType].SafeAdd(_f3dm?.GetObjInfo(_)));
            }

            return powerUps;
        }

        protected List<GameObjBuildInfo> GetScenarioObjects(DrawableType type)
        {
            return _f3dm?.GetLayerObjects(Title, type.ToString());
        }
        protected List<GameObjBuildInfo> GetObjects(params string[] path)
        {
            var newPath = new List<string>(path);
            newPath.Insert(0, Title);
            return _f3dm?.GetObjects(newPath.ToArray());
        }
        protected List<Drawable> GetScenarioDrawables(DrawableType type)
        {
            var ret = new List<Drawable>();
            _f3dm?.GetLayerObjects(Title, type.ToString())?.ForEach(_ => ret.SafeAdd(new Drawable(_.Mesh, _.Material)));
            return ret;
        }

        protected virtual void ActivatePowerUp(PowerUp powerUp)
        {
            powerUp.Activate(this);

            //Add to the active, except if it's non activable (live), 
            if (!powerUp.IsActive) return;

            if (_activePowerUps.Count ==MaxPowerUps)
            {
                _activePowerUps[0].Deactivate(this);
                _activePowerUps.RemoveAt(0);
            }

            _activePowerUps.SafeAdd(powerUp);
        }
        protected virtual void DeactivatePowerUp(PowerUp powerUp)
        {
            powerUp.Deactivate(this);
            _activePowerUps.SafeRemove(powerUp);
        }
        public virtual void ProcessFrame(double ellapsedMs)
        {
            //Power Ups
            _fallingPowerUps?.ForEach(_ => _.ProcessFrame(ellapsedMs));

            //Move Pad and check if any power up is collected
            if (Pad.ProcessFrame(ellapsedMs, _wall, _fallingPowerUps, out var padCollisionPowerUps, out var lostPowerUps))
            {
                //Remove from falling
                _fallingPowerUps.SafeRemove(padCollisionPowerUps);

                foreach (var powerUp in padCollisionPowerUps)
                {
                    //Activate power up
                    if (_activePowerUps.Any(_ => _.Type == powerUp.Type)) continue;
                    ActivatePowerUp(powerUp);
                }
            }

            //Remove lost Power Ups
            _fallingPowerUps.SafeRemove(lostPowerUps);

            //Remove expired power ups
            foreach (var powerUp in new List<PowerUp>(_activePowerUps))
            {
                if (powerUp.IsActive) continue;
                DeactivatePowerUp(powerUp);
            }

            //Move balls 
            var blocksToRemove = new List<Block>();
            foreach (var currentBall in new List<Ball>(Balls))
            {
                var ballResult = currentBall.ProcessFrame(ellapsedMs, _wall, Pad, _blocks, _activePowerUps.Any(_ => _ is StickyBall));

                switch (ballResult.Result)
                {
                    case CollisionResult.ResultType.Wall:
                        _wall.PlayHitSound();
                        break;
                    case CollisionResult.ResultType.Pad:
                        Pad.PlayHitSound();
                        break;
                    case CollisionResult.ResultType.Block:
                        var block = ballResult.Object as Block;
                        block.RemainingHits--;
                        block.PlayHitSound();
                        if (block.RemainingHits < 1)
                        {
                            blocksToRemove.SafeAdd(block);
                            _fallingPowerUps.SafeAdd(block.PowerUp);
                        }
                        break;
                    case CollisionResult.ResultType.Loose:
                        Balls.SafeRemove(currentBall);
                        break;
                }
            }
            _blocks.SafeRemove(blocksToRemove);

            //Win
            if (_blocks.Count == 0)
            {
            }

            //Loose live
            if (Balls.Count == 0)
            {
                SetLives((Lives - 1).ClampMin(0));

                //Loose
                if (Lives == 0)
                {

                }


                Reset();
            }
        }

        public void Reset(bool resetBlocks = false)
        {
            Balls = CreateBalls();
            Pad?.Reset();
            
            if (resetBlocks)
            {
                _blocks = CreateBlocks();
                Sound.PlayBgMusic(_music);
                SetLives(_initialLives);
            }

        }

        public virtual void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            Balls?.ForEach(_ => _?.Draw(dp, ellapsedMs));
            _blocks?.ForEach(_ => _?.Draw(dp, ellapsedMs));
            _fallingPowerUps?.ForEach(_ => _?.Draw(dp, ellapsedMs));
            _wall?.Draw(dp, ellapsedMs);
            Pad?.Draw(dp, ellapsedMs);
        }
    }
}
