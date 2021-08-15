using Rhino.Display;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RhinoArkanoid
{
    internal class Drawable
    {
        private readonly Mesh _mesh;
        private BoundingBox _aabb;
        private DisplayMaterial _material;

        public Transform Transform;
        public BoundingBox AABB
        {
            get
            {
                var copy = new BoundingBox(_aabb.Min, _aabb.Max);
                copy.Transform(Transform);
                return copy;
            }
        }


        public Drawable(Color color, params Mesh[] meshes)
        {
            _mesh = new Mesh();
            foreach (var mesh in meshes)
                _mesh.Append(mesh);

            _aabb = _mesh.GetBoundingBox(true);
            _material = new DisplayMaterial(color);
            Transform = Transform.Identity;
        }


        public void Draw(DisplayPipeline dp)
        {
            if (_mesh == null) return;
            dp.PushModelTransform(Transform);
            dp.DrawMeshShaded(_mesh, _material);
            dp.PopModelTransform();
        }

        public Mesh GetMesh()
        {
            var dupMesh = _mesh.DuplicateShallow();
            dupMesh.Transform(Transform);
            return dupMesh as Mesh;
        }

        public bool Collide(Line motionLine)
        {
            return Intersection.MeshLine(GetMesh(), motionLine).Length > 0;
        }
    }
}
