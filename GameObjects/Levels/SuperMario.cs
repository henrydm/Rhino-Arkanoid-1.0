using Rhino.DocObjects;
using RhinoArkanoid.GameObjects.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.Levels
{
    class SuperMario : Level
    {
        public SuperMario() : base("Super Mario", null)
        {

        }

        private const double _ballSpeed = 110;
        private const double _padSpeed = 250;

        public override List<Ball> CreateBalls()
        {
            var sceneObjs = GetScenarioObjects(DrawableType.Ball);
            var biggest = sceneObjs.PullBiggest();
            return new List<Ball> { new Ball(biggest.Mesh, biggest.Material, _ballSpeed) { Ornaments = sceneObjs.ToDrawable() } };
        }

        protected override List<Block> CreateBlocks()
        {
            var sceneObjs = GetScenarioObjects(DrawableType.Blocks);
            var blocks = new List<Block>();

            var _marioBoxes = new Dictionary<Group, List<GameObjBuildInfo>>();


            foreach (var info in sceneObjs)
            {

                if (info.GetAttribute<string>("MisteryBox", out _))
                {
                    var groupIndices = info.RhinoAttributes.GetGroupList();

                    if (groupIndices != null)
                    {
                        var group = _f3dm.AllGroups.FindIndex(groupIndices[0]);
                        if (group == null) continue;

                        _marioBoxes.SafeAdd(group, info);
                    }
                }
                else
                {
                    blocks.Add(new Block(info.Mesh, info.Material, info.GetHits(), Resources.clin1, Resources.blade));
                }


                //var block = new Gem(info.Mesh, info.Material, info.GetAttribute<int>("hits", out var hitsAtt) ? hitsAtt : 1, Resources.clin1, Resources.blade);

                //if (info.GetAttribute<string>("powerup", out var powerUpTypeStr))
                //{
                //    if (Enum.TryParse<PowerType>(powerUpTypeStr, true, out var powerUpType) && _powerUpInfo.TryGetValue(powerUpType, out var gameObjInfo))
                //    {
                //        var powerUpDrawables = gameObjInfo.ToDrawable();
                //        var powerUpBox = powerUpDrawables.GetBoundingBoxTransformed();
                //        powerUpDrawables.ForEach(_ => _.Transform = Transform.Translation(block.BoundingBoxTransformed.Center - powerUpBox.Center));

                //        block.PowerUp = PowerUp.Create(powerUpType);
                //        block.PowerUp.Drawables = powerUpDrawables;

                //        //Set power up size
                //        if (_powerUpLegendWidth == 0)
                //            _powerUpLegendWidth = powerUpBox.Max.X - powerUpBox.Min.X;
                //    }
                //    else
                //    {
                //        Rhino.RhinoApp.WriteLine($"Error parsing block power up: {powerUpTypeStr} type doesn't exist.");
                //    }

                //}

                //blocks.Add(block);

            }



            foreach (var kvp in _marioBoxes)
            {

                var biggestInfo = kvp.Value.PullBiggest();

                var gem = new Gem(biggestInfo.Mesh, biggestInfo.Material, biggestInfo.GetHits(), Resources.clin1, Resources.blade) { Ornaments = kvp.Value.ToDrawable() };
                blocks.Add(gem);

            }
            return blocks;
        }
        protected override Pad CreatePad()
        {
            var drawables = GetScenarioDrawables(DrawableType.Pads);
            return new Pad(drawables, _padSpeed, null);
        }

        protected override Wall CreateWall()
        {
            GetWallObjects(out var limits, out var ornaments, out var animatedColliders);
            return new Wall(limits.GetDrawables(), ornaments.GetDrawables(), null);

        }

    }
}
