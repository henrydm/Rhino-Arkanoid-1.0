using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.AnimationObjects
{
    abstract class TxObject
    {
        //Drawables for this object
        public List<Drawable> Drawables;
        public List<TxObject> Transformables;



        //public BoundingBox GetBoundingBoxDrawables()
        //{
        //    if (Drawables == null) return BoundingBox.Empty;
        //    return Drawables.GetBoundingBox();
        //}

        public BoundingBox GetBoundingBoxFull()
        {
            var topBox = Drawables == null ? BoundingBox.Empty : Drawables.GetBoundingBoxTransformed();
            Transformables?.ForEach(_ => topBox.Union(_.GetBoundingBoxFull()));
            return topBox;
        }

        public void SetTx(Transform tx)
        {
            Drawables?.ForEach(_ => _.Transform = tx);
            Transformables?.ForEach(_ => _.SetTx(tx));
        }
        public void AppendTx(Transform tx)
        {
            Drawables?.ForEach(_ => _.Transform *= tx);
            Transformables?.ForEach(_ => _.AppendTx(tx));
        }
        public virtual void ProcessFrame(double ellapsedMs)
        {
            Transformables?.ForEach(_ => _.ProcessFrame(ellapsedMs));
        }

        public void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            Drawables?.ForEach(_ => _.Draw(dp, ellapsedMs));
            Transformables?.ForEach(_ => _.Draw(dp, ellapsedMs));
        }

    }
}
