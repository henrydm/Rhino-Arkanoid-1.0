using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid
{
    internal static class Extensions
    {
        public static bool Center(this List<GeometryBase> geometries)
        {
            if (geometries == null) return false;
            var aabb = geometries.GetBoundingBox();
            return geometries.Transform(Rhino.Geometry.Transform.Translation(new Vector3d(-aabb.Center)));
        }
        public static BoundingBox GetBoundingBox(this List<GeometryBase> geometries)
        {
            var aabb = BoundingBox.Empty;
            if (geometries == null) return aabb;
            geometries.ForEach(_ => aabb.Union(_.GetBoundingBox(true)));
            return aabb;
        }

        public static BoundingBox GetBoundingBoxOriginal(this List<Drawable> drawables)
        {
            var aabb = BoundingBox.Empty;
            if (drawables == null) return aabb;
            drawables.ForEach(_ => aabb.Union(_.BoundingBoxOriginal));
            return aabb;
        }
        public static BoundingBox GetBoundingBoxTransformed(this List<Drawable> drawables)
        {
            var aabb = BoundingBox.Empty;
            if (drawables == null) return aabb;
            drawables.ForEach(_ => aabb.Union(_.BoundingBoxTransformed));
            return aabb;
        }
        public static BoundingBox GetBoundingBoxTransformed(this List<GameObjBuildInfo> gameObjList)
        {
            var aabb = BoundingBox.Empty;
            if (gameObjList == null) return aabb;
            gameObjList.ForEach(_ => aabb.Union(_.Mesh.GetBoundingBox(true)));
            return aabb;
        }
        public static Point3d GetNormalizedPt(this BoundingBox box, double x, double y, double z)
        {
            if (!box.IsValid) return Point3d.Unset;
            var sizeX = box.Max.X - box.Min.X;
            var sizeY = box.Max.Y - box.Min.Y;
            var sizeZ = box.Max.Z - box.Min.Z;
            return new Point3d(box.Min.X + sizeX * x.Clamp(0, 1), box.Min.Y + sizeY * y.Clamp(0, 1), box.Min.Z + sizeZ * z.Clamp(0, 1));
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
            var planeVector = new Vector3d(inVector.X, 0, inVector.Z);
            planeVector.Unitize();

            planeVector.Transform(Rhino.Geometry.Transform.Mirror(pt, normal));
            if (planeVector == inVector)
            {
                planeVector.Transform(Rhino.Geometry.Transform.Mirror(pt, -normal));
            }
            planeVector.Y = 0;
            planeVector.Unitize();
            return planeVector;
        }

        public static void SafeAdd<T>(this IList<T> list, T objectToAdd)
        {
            if (objectToAdd != null && !list.Contains(objectToAdd))
                list.Add(objectToAdd);
        }
        public static void SafeAdd<T>(this IList<T> list, List<T> objectsToAdd)
        {
            if (list == null || objectsToAdd == null) return;
            objectsToAdd.ForEach(_ => list.SafeAdd(_));
           
        }
        public static bool SafeRemove<T>(this IList<T> list, T objectToRemove)
        {
            if (list == null) return false;
            if (list.Contains(objectToRemove)) return list.Remove(objectToRemove);
            return false;
        }

        public static bool SafeRemove<T>(this IList<T> list, IList<T> objectsToRemove)
        {
            if (list == null || objectsToRemove == null) return false;
            return objectsToRemove.All(_ => list.SafeRemove(_));
        }
        public static void SafeAdd<T, TW>(this IDictionary<T, List<TW>> dictionary, T key, TW value)
        {
            if (key == null) return;
            if (dictionary.ContainsKey(key) && dictionary[key] != null)
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<TW> { value });
            }
        }

        public static List<Drawable> ToDrawable(this List<GameObjBuildInfo> gameObjs)
        {
            var drawables = new List<Drawable>();
            gameObjs?.ForEach(_ => drawables.Add(_.GetDrawable()));
            return drawables;
        }
        /// <summary>
        /// Check if passes valueis smaller or bigger than specified value and set the <see cref="min"/> or <see cref="max"/> if so.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="min">Minimum allowed value.</param>
        /// <param name="max">Maximum allowed value.</param>
        /// <returns>The value clamped.</returns>
        public static double Clamp(this double value, double min, double max)
        {
            return value.ClampMin(min).ClampMax(max);
        }
        
        /// <summary>
        /// Check if passes value is bigger than specified value and set the <see cref="max"/> if so.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="max">Maximum allowed value.</param>
        /// <returns>The value clamped.</returns>
        public static double ClampMax(this double value, double max)
        {
            return value > max ? max : value;
        }
        
        public static int ClampMin(this int value, int min)
        {
            return value < min ? min : value;
        }

        /// <summary>
        /// Check if passes value is smaller than specified value and set the <see cref="min"/> if so.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <param name="min">Minimum allowed value.</param>
        /// <returns>The value clamped.</returns>
        public static double ClampMin(this double value, double min)
        {
            return value < min ? min : value;
        }

        public static List<Drawable> GetDrawables(this List<GameObjBuildInfo> infoList)
        {
            var ret = new List<Drawable>();
            infoList?.ForEach(_ => ret.Add(_.GetDrawable()));
            return ret;
        }

        public static List<GameObjBuildInfo> GetLayerObjects(this File3dm _f3dm, string topLevel, string subLevel)
        {
            //Due to Rhino is throwing NullRef exception at methods layer.GetChildren() and layer.IsChildOf() on version 7.9, using custom sublayer search methods
            var subLayer = _f3dm?.GetSubLayers(_f3dm?.AllLayers?.FindName(topLevel, Guid.Empty))?.First(_ => _.Name == subLevel);
            var ret = new List<GameObjBuildInfo>();
            _f3dm?.Objects?.FindByLayer(subLayer).ToList().ForEach(_ => ret.Add(_f3dm?.GetObjInfo(_)));
            return ret;
        }
       
        public static GameObjBuildInfo GetObjInfo(this File3dm _f3dm,File3dmObject f3dmObj)
        {
            //Get Attributes
            var dictAtt = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(f3dmObj.Attributes.Name))
            {
                var tokens = f3dmObj.Attributes.Name.Split(new char[] { ' ' });
                if (tokens != null)
                {
                    foreach (var token in tokens)
                    {
                        var kvp = token.Split(new char[] { '=' });
                        if (kvp == null || kvp.Length != 2) continue;
                        dictAtt.Add(kvp[0], kvp[1]);
                    }
                }
            }

            //Rhino Block Instances (Casted directly to Mesh)
            if (f3dmObj.Geometry is InstanceReferenceGeometry instance)
            {
                var def = _f3dm.AllInstanceDefinitions.FindId(instance.ParentIdefId);
                var ids = def.GetObjectIds();
                var obj = _f3dm.Objects.FindId(ids[0]);
                //if (obj.UserDictionary.Count > 1)
                //{

                //}
                var mesh = (obj.Geometry as Mesh).DuplicateMesh();
                mesh?.Transform(instance.Xform);

                var mat = _f3dm.AllMaterials.FindIndex(obj.Attributes.MaterialIndex);

                return new GameObjBuildInfo(f3dmObj.Attributes, mesh, new DisplayMaterial(mat), dictAtt);
            }
            //Other Geometry (Casted directly to Mesh)
            else
            {

                if (f3dmObj.Attributes.MaterialSource == ObjectMaterialSource.MaterialFromLayer)
                {
                    var layer = _f3dm.AllLayers.FindIndex(f3dmObj.Attributes.LayerIndex);
                    var layerMatIndex = layer.RenderMaterialIndex;
                    if (layerMatIndex < 0) layerMatIndex = 0;
                    var mat = _f3dm.AllMaterials.FindIndex(layerMatIndex);
                    return new GameObjBuildInfo(f3dmObj.Attributes, f3dmObj.Geometry as Mesh, new DisplayMaterial(mat), dictAtt);
                }
                else if (f3dmObj.Attributes.MaterialSource == ObjectMaterialSource.MaterialFromObject)
                {
             
                    return new GameObjBuildInfo(f3dmObj.Attributes, f3dmObj.Geometry as Mesh, new DisplayMaterial(_f3dm.AllMaterials.FindIndex(f3dmObj.Attributes.MaterialIndex)), dictAtt);
                }
            }

            return default;
        }

        /// <summary>
        /// Pull the biggest bounding box object, removes it from the passed list and returned.
        /// </summary>
        /// <param name="objs">Objects with meshes.</param>
        /// <returns></returns>
        public static GameObjBuildInfo PullBiggest(this IList<GameObjBuildInfo> objs)
        {
            if (objs == null || !objs.Any()) return null;

            var max = double.MinValue;
            GameObjBuildInfo biggestInfo = null;
            foreach (var info in objs)
            {
                if (info.Mesh == null) continue;
                var box = info.Mesh.GetBoundingBox(false);
                var size = box.Diagonal.SquareLength;
                if (size < max) continue;
                max = size;
                biggestInfo = info;
            }
           
            if (biggestInfo != null) objs.Remove(biggestInfo);
            return biggestInfo;
        }
        /// <summary>
        /// Pull the biggest bounding box object, removes it from the passed list and returned.
        /// </summary>
        /// <param name="objs">Objects with meshes.</param>
        /// <returns></returns>
        public static Drawable PullBiggest(this IList<Drawable> objs)
        {
            if (objs == null || !objs.Any()) return null;

            var max = double.MinValue;
            Drawable biggestInfo = null;
            foreach (var info in objs)
            {
                var size = info.BoundingBoxOriginal.Diagonal.SquareLength;
                if (size < max) continue;
                max = size;
                biggestInfo = info;
            }

            if (biggestInfo != null) objs.Remove(biggestInfo);
            return biggestInfo;
        }
        public static Layer GetLayer(this File3dm _f3dm, params string[] path)
        {
            Layer targetLayer = null;
            if (path == null) return null;

            var layers = _f3dm.AllLayers.ToList();
            foreach (var name in path)
            {
                targetLayer = layers.FirstOrDefault(_ => _.Name == name);
                if (targetLayer == null) break;
                layers = _f3dm.GetSubLayers(targetLayer);
            }

            return targetLayer;
        }

        public static void AddToDoc(this Drawable geo)
        {
            if (geo == null) return;
            Rhino.RhinoDoc.ActiveDoc.Objects.Add(geo.MeshTransformed);
        }
        public static void AddToDoc(this List<Drawable> geos)
        {
            geos?.ForEach(_ => _?.AddToDoc());
        }


        public static List<GameObjBuildInfo> GetObjects(this File3dm _f3dm, params string[] path)
        {
            var ret = new List<GameObjBuildInfo>();
            _f3dm?.Objects?.FindByLayer(_f3dm?.GetLayer(path))?.ToList()?.ForEach(_ => ret.SafeAdd(_f3dm?.GetObjInfo(_)));
            return ret;
        }

        public static List<Layer> GetSubLayers(this File3dm _f3dm, Layer layer)
        {
            var ret = new List<Layer>();
            foreach (var currentLayer in _f3dm.AllLayers)
            {
                if (layer == null || currentLayer == layer || currentLayer.ParentLayerId != layer.Id) continue;
                ret.Add(currentLayer);
            }

            return ret;
        }

    }
}
