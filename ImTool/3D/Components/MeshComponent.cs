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

        public void LoadFromObj(string objPath)
        {
            var model = SimpleModel.CreateFromObj(objPath);
            SetModel(model);

            return;
        }

        public struct SimpleVertexDefinition
        {
            public const uint SizeInBytes = 12 + 8 + 12;

            public float X;
            public float Y;
            public float Z;

            public float U;
            public float V;

            public float NormX;
            public float NormY;
            public float NormZ;

            public SimpleVertexDefinition(Vector3 pos, Vector2 uv)
            {
                X = pos.X;
                Y = pos.Y;
                Z = pos.Z;
                U = uv.X;
                V = uv.Y;
            }
        }

        public struct VertexDefinition
        {
            public const uint SizeInBytes = 12 + 8 + 8 + 12 + 12 + 4;

            public float X;
            public float Y;
            public float Z;

            public float U;
            public float V;

            public float NormU;
            public float NormV;

            public float NormX;
            public float NormY;
            public float NormZ;

            public float TangX;
            public float TangY;
            public float TangZ;

            public uint Color;

            public VertexDefinition(Vector3 pos, Vector2 uv)
            {
                X = pos.X;
                Y = pos.Y;
                Z = pos.Z;
                U = uv.X;
                V = uv.Y;
            }
        }
    }
}
