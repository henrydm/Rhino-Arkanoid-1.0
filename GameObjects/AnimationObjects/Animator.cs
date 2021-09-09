using System.Collections.Generic;

namespace RhinoArkanoid.GameObjects.AnimationObjects
{
    interface IAnimator
    {
          void ProcessFrame(List<Drawable> drawables, double ellapsedMs);

    }
}
