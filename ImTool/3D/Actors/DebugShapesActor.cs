using ImTool.Scene3D;
using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static ImTool.Scene3D.Components.DebugShapesComp;

namespace ImTool.Scene3D
{
    public class DebugShapesActor : Actor
    {
        public DebugShapesComp DebugShapes;

        public override void Init(World world)
        {
            base.Init(world);

            DebugShapes = AddComponet<DebugShapesComp>();
        }

        public Cube AddCube(Vector3 pos, Vector3? size = null, Vector4? color = null, float thickness = 2f) => DebugShapes.AddCube(pos, size, color, thickness);
        public DebugShapesComp.Rect AddRect(Vector3 pos, Vector2 size, Vector4? color = null, float thickness = 2f) => DebugShapes.AddRect(pos, size, color, thickness);
        public Cricle AddCricle(Vector3 pos, float radius = 1f, ushort sides = 16, Vector4? color = null, float thickness = 2f) => DebugShapes.AddCricle(pos, radius, sides, color, thickness);
        public Cylnder AddCylnder(Vector3 pos, float radius = 1f, float height = 2f, ushort sides = 16, Vector4? color = null, float thickness = 2f) => DebugShapes.AddCylnder(pos, radius, height, sides, color, thickness);
        public Sphere AddSphere(Vector3 pos, float radius = 1f, ushort sides = 16, Vector4? color = null, float thickness = 2f) => DebugShapes.AddSphere(pos, radius, sides, color, thickness);
        public Arrow AddArrow(Vector3 pos, Vector3 dir, ushort sides = 16, Vector4? color = null, float thickness = 2f) => DebugShapes.AddArrow(pos, dir, sides, color, thickness);
    }
}
