using Rhino;
using Rhino.Commands;
using Rhino.Input.Custom;

namespace RhinoArkanoid
{
    public class RhinoArkanoidCommand : Command
    {
        public RhinoArkanoidCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RhinoArkanoidCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "Arkanoid";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var options = new GetOption();
            options.SetCommandPrompt(EnglishName);

            var indexSound = options.AddOption("Sound");
            var indexFx = options.AddOption("FX");
            var indexReset = options.AddOption("Reset");
            var indexExit = options.AddOption("Exit");

            Game.Run();
            Game.OnStopGame += (o, e) => RhinoApp.SendKeystrokes("!", true);
            while (Game.Playing)
            {
                //options.Get();
                var slectedOption = options.Option();

                if (slectedOption?.Index == indexFx)
                {
                    //Game.Fx = !Game.Fx;
                }
                else if (slectedOption?.Index == indexSound)
                {
                    //Game.SetMusic(!Game.Music);
                }
                else if (slectedOption?.Index == indexReset)
                {
                    Game.Reset();
                }
                else if (slectedOption?.Index == indexExit)
                {
                    Game.Stop();
                   
                }
            }


    
            return Result.Success;
        }
    }
}
