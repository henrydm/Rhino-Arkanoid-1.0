using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.AnimationObjects
{
    /// <summary>
    /// Local rotation (relative to the object)
    /// </summary>
    class TxRotation : TxObject
    {
        protected double Speed;
        protected Vector3d Axis;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="speed">Turns per second.</param>
        public TxRotation(Vector3d axis, double speed)
        {
            Axis = axis;
            Speed = speed;
        }

        public override void ProcessFrame(double ellapsedMs)
        {
            base.ProcessFrame(ellapsedMs);

            var box = GetBoundingBoxFull();
            var rad = Math.PI * 2 * Speed * (ellapsedMs / 1000);
            var tx = Transform.Rotation(rad, Axis, box.Center);
            SetTx(tx);

        }
    }
}
