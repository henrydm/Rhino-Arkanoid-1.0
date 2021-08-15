using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid
{
    public static class Extensions
    {
        public static bool Center(this List<GeometryBase> geometries)
        {
            if (geometries == null) return false;
            var aabb = geometries.GetAABB();
            return geometries.Transform(Rhino.Geometry.Transform.Translation(new Vector3d(-aabb.Center)));
        }
        public static BoundingBox GetAABB(this List<GeometryBase> geometries)
        {
            var aabb = BoundingBox.Empty;
            if (geometries == null) return aabb;

            foreach (var geo in geometries)
            {
                if (geo == null) continue;
                aabb.Union(geo.GetBoundingBox(true));
            }

            return aabb;
        }

        public static bool Transform(this List<GeometryBase> geometries, Transform tx)
        {
            if (geometries == null || geometries.Count == 0) return false;

            var res = true;
            foreach (var geo in geometries)
            {
                if (geo == null) continue;
                res &= geo.Transform(tx);
            }

            return res;

        }

        public static Vector3d GetReflected(this Vector3d inVector,Point3d pt, Vector3d normal)
        {

            //var normal = inVector.Rotate(Math.PI * 0.5, Vector3d.ZAxis);
            var planeVector = new Vector3d(inVector.X, 0, inVector.Z);
            planeVector.Unitize();

            planeVector.Transform(Rhino.Geometry.Transform.Mirror(pt, normal));
            if (planeVector == inVector)
            {
                planeVector.Transform(Rhino.Geometry.Transform.Mirror(pt, -normal));
                Rhino.RhinoApp.WriteLine("ff");
            }
            return planeVector;

            //var dotProduct = inVector.X * normal.X + inVector.Z * normal.Z;
            //var dotNormalX = dotProduct * normal.X;
            //var dotNormalY = dotProduct * normal.Z;
            //var ret = new Vector3d(inVector.X - (dotNormalX * 2), 0, inVector.Z - (dotNormalY * 2));
            //if (ret.X == inVector.X && ret.Z== inVector.Z)
            //{
            //    ret.X = ret.X * -1;
            //}

            //return ret;

        }
    }
}
