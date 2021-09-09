using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoArkanoid.GameObjects.Blocks
{
    class Brick : Block
    {
        public Brick(Color color, Mesh mesh, byte[] hitSound = null) : base(mesh, new DisplayMaterial(color), 1, hitSound, hitSound)
        {

        }

    }
}
