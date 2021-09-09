using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Threading;

namespace RhinoArkanoid.GameObjects
{
    internal class Drawable : IDrawable
    {
        protected readonly Mesh _meshOriginal;
        public BoundingBox BoundingBoxOriginal { get; }
        public DisplayMaterial _material;
        public Transform Transform;

        protected void SetMaterial(DisplayMaterial material)
        {
            _material = material;
        }

        public Drawable(Mesh mesh, DisplayMaterial material, Transform tx)
        {
            _meshOriginal = mesh;
            BoundingBoxOriginal = _meshOriginal == null ? BoundingBox.Empty : _meshOriginal.GetBoundingBox(true);
            _material = material;
            Transform = tx;
        }
        public Drawable(Mesh mesh, DisplayMaterial material) : this(mesh, material, Transform.Identity)
        {

        }
        public Drawable(Drawable drawable) : this(drawable?._meshOriginal, drawable?._material, drawable.Transform)
        {

        }

        public BoundingBox BoundingBoxTransformed
        {
            get
            {
                var copy = new BoundingBox(BoundingBoxOriginal.Min, BoundingBoxOriginal.Max);
                copy.Transform(Transform);
                return copy;
            }
        }

        public Mesh MeshTransformed
        {
            get
            {
                var dupMesh = _meshOriginal?.DuplicateShallow();
                dupMesh?.Transform(Transform);
                return dupMesh as Mesh;
            }
        }

        public virtual void Draw(DisplayPipeline dp, double ellapsedMs)
        {
            if (_meshOriginal == null || _material == null) return;
            dp?.PushModelTransform(Transform);
            dp?.DrawMeshShaded(_meshOriginal, _material);
            dp?.PopModelTransform();
        }

        public bool Collide(Line motionLine, double radius, out Point3d intersectionPt, out Vector3d normal)
        {
            normal = Vector3d.Unset;
            intersectionPt = Point3d.Unset;
            if (_meshOriginal == null) return false;

            var samples = 3.0;
            for (int i = 0; i < samples; i++)
            {
                var pt = motionLine.PointAt(i / samples);
                var mp = MeshTransformed.ClosestMeshPoint(pt, radius);
                if (mp == null) continue;
                intersectionPt = mp.Point;
                normal = MeshTransformed.NormalAt(mp);
                return true;
            }


            return false;
        }

        //public bool Collide(Mesh other, out Point3d intersectionPt, out Vector3d normal)
        //{
        //    normal = Vector3d.Unset;
        //    intersectionPt = Point3d.Unset;

        //    if (Intersection.MeshMesh(new[] { MeshTransformed, other }, 0.01, out var intersectionsPolys, false, out var overlapPolys, false, out var overlapMesh, null, CancellationToken.None, null) && intersectionsPolys != null && intersectionsPolys.Length > 0)
        //    {
        //        var mp = MeshTransformed.ClosestMeshPoint(intersectionsPolys[0][0], 0);
        //        normal = MeshTransformed.NormalAt(mp);
        //        return true;
        //    }

        //    return false;
        //}
    }
}
