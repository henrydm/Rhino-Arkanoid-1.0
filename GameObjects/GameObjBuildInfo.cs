using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using RhinoArkanoid.GameObjects.PowerUps;
using System;
using System.Collections.Generic;

namespace RhinoArkanoid.GameObjects
{
    class GameObjBuildInfo
    {
        public Mesh Mesh { get; }
        public DisplayMaterial Material { get; }
        public ObjectAttributes RhinoAttributes { get; }
        Dictionary<string, string> _attributes;

        public GameObjBuildInfo(ObjectAttributes att, Mesh mesh, DisplayMaterial material, Dictionary<string, string> attributes)
        {
            RhinoAttributes = att;
            Mesh = mesh;
            Material = material;
            _attributes = attributes;
        }

        /// <summary>
        /// Get all attributes which the key includes the passed string
        /// </summary>
        /// <typeparam name="T">Type of the value objects.</typeparam>
        /// <param name="key">Key to search if it's contained in attributes.</param>
        /// <param name="coincidences">All objects which contains the passed word on the key.</param>
        /// <returns></returns>
        public bool GetContained<T>(string key, out Dictionary<string, List<T>> coincidences)
        {
            coincidences = new Dictionary<string, List<T>>();
            if (_attributes != null || string.IsNullOrEmpty(key)) return false;

            foreach (var kvp in _attributes)
            {
                if (!kvp.Key.Contains(key) || !Convert<T>(_attributes[kvp.Key], out var value)) continue;
                coincidences.SafeAdd(kvp.Key, value);

            }
            return coincidences.Count > 0;
        }

        public bool GetAttribute<T>(string key, out T value)
        {
            value = default;
            return (_attributes != null && _attributes.ContainsKey(key) && Convert<T>(_attributes[key], out value));
        }

        public int GetHits()
        {
            return GetAttribute<int>("hits", out var hitsAtt) ? hitsAtt : 1;
        }

        public bool GetPowerUp(out PowerType type)
        {
            type = PowerType.None;
            return GetAttribute<string>("powerup", out var powerUpTypeStr) && Enum.TryParse(powerUpTypeStr, out type);
        }

        public bool Convert<T>(string value, out T converted)
        {
            converted = default;
            if (string.IsNullOrEmpty(value)) return false;
            try
            {
                var type = typeof(T);
                if (type == typeof(string)) converted = (T)(object)value;
                else if (type == typeof(int)) converted = (T)(object)System.Convert.ToInt32(value);
                else if (type == typeof(double)) converted = (T)(object)System.Convert.ToBoolean(value);
                else return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public Drawable GetDrawable()
        {
            return new Drawable(Mesh, Material);
        }
    }
}
