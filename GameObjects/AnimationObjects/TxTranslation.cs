//using Rhino.Geometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RhinoArkanoid.GameObjects.AnimationObjects
//{
//    class TxTranslation : TxObject
//    {
//        protected double Speed;
//        protected Vector3d Axis;

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="axis"></param>
//        /// <param name="speed">Units per second.</param>
//        public TxTranslation(Vector3d axis, double speed)
//        {
//            Axis = axis;
//            Speed = speed;
//        }

//        public override void ProcessFrame(double ellapsedMs)
//        {
//            base.ProcessFrame(ellapsedMs);
//            var delta = Speed * (ellapsedMs / 1000);
//            var tx = Transform.Translation(Axis * delta * Speed);
//            AppendTx(tx);
//        }
//    }
//}
