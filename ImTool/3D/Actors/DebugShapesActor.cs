using ImTool.Scene3D;
using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    public class DebugShapesActor : Actor
    {
        public override void Init(World world)
        {
            base.Init(world);

            AddComponet<DebugShapesComp>();
        }

        public override void Render(CommandList cmdList)
        {
            base.Render(cmdList);
        }

        public override void Update(double dt)
        {
            base.Update(dt);
        }
    }
}
