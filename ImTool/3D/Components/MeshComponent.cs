using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;
using Vulkan;
using static System.Net.Mime.MediaTypeNames;

namespace ImTool.Scene3D.Components
{
    public class MeshComponent : Component
    {
        public DeviceBuffer WorldBuffer;
        public ResourceSet ItemResourceSet;
        public SimpleModel Model;

        public override unsafe void Init(Actor owner)
        {
            base.Init(owner);
        }

        public void SetModel(SimpleModel model)
        {
            Model = model;

            WorldBuffer     = Resources.GD.ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ItemResourceSet = Resources.GD.ResourceFactory.CreateResourceSet(new ResourceSetDescription(Model.PerItemResourceLayout, WorldBuffer));
        }

        public override void Render(CommandList cmdList)
        {
            if (Model == null)
                return;

            cmdList.SetPipeline(Model.Pipeline);
            cmdList.SetGraphicsResourceSet(0, Owner.World.ProjViewSet);

            var world = Owner.Transform;
            cmdList.UpdateBuffer(WorldBuffer, 0, ref world);

            cmdList.SetVertexBuffer(0, Model.VertBuffer);
            cmdList.SetIndexBuffer(Model.IndexBuffer, IndexFormat.UInt32);

            cmdList.SetGraphicsResourceSet(1, ItemResourceSet);

            foreach (var meshSection in Model.MeshSections)
            {
                if (meshSection.TexResourceSet != null)
                    cmdList.SetGraphicsResourceSet(2, meshSection.TexResourceSet);

                cmdList.DrawIndexed(meshSection.IndicesLength, 1, meshSection.IndiceStart, 0, 0);
            }
        }

        public override void Update(double dt)
        {
            base.Update(dt);
        }
    }
}
