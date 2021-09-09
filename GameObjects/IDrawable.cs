using Rhino.Display;
using Rhino.Geometry;

namespace RhinoArkanoid.GameObjects
{
    interface IDrawable
    {
        void Draw(DisplayPipeline dp, double ellapsedMs);
        bool Collide(Line motionLine, double ballRadius, out Point3d intersectionPt, out Vector3d normal);
    }
}
