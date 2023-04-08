using System;
using System.Collections.Generic;
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
        private DeviceBuffer VertBuffer;
        private DeviceBuffer IndexBuffer;
        private Pipeline Pipeline;
        private ShaderSetDescription ShaderSet;
        private ResourceLayout PerItemResourceLayout;

        private uint IndiceCount = 0;

        public override void Init(Actor owner)
        {
            base.Init(owner);

            var rf                = owner.World.MainWindow.GetGraphicsDevice().ResourceFactory;
            ShaderSet             = CreateShaderSet(rf);
            PerItemResourceLayout = CreatePerItemResourceLayout(rf);

            WorldBuffer     = rf.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ItemResourceSet = rf.CreateResourceSet(new ResourceSetDescription(PerItemResourceLayout, WorldBuffer));

            Pipeline = rf.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                //new RasterizerStateDescription(FaceCullMode.Front, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, true),
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                ShaderSet,
                new[] { owner.World.ProjViewLayout, PerItemResourceLayout },
                owner.World.GetFBDesc().OutputDescription));

            SetData(new VertexDefinition[0], new uint[0]);
        }

        private ShaderSetDescription CreateShaderSet(ResourceFactory rf)
        {
            var vert    = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Mesh.MeshVert.glsl", ShaderStages.Vertex);
            var frag    = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Mesh.MeshFrag.glsl", ShaderStages.Fragment);
            var shaders = rf.CreateFromSpirv(vert, frag);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Uvs", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("NormUvs", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Norms", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Tangs", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Int1)
                        )
                },
                shaders);

            return shaderSet;
        }

        private ResourceLayout CreatePerItemResourceLayout(ResourceFactory rf)
        {
            ResourceLayout worldTextureLayout = rf.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );

            return worldTextureLayout;
        }

        public void SetData(ReadOnlySpan<VertexDefinition> verts, ReadOnlySpan<uint> indices)
        {
            var gd = Owner.World.MainWindow.GetGraphicsDevice();

            if (VertBuffer != null) gd.DisposeWhenIdle(VertBuffer);
            VertBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(VertexDefinition.SizeInBytes * verts.Length), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(VertBuffer, 0, verts);

            if (IndexBuffer != null) gd.DisposeWhenIdle(IndexBuffer);
            IndexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Length), BufferUsage.IndexBuffer));
            gd.UpdateBuffer(IndexBuffer, 0, indices);

            IndiceCount = (uint)indices.Length;
        }

        public void SetData(MeshData data)
        {
            SetData(CollectionsMarshal.AsSpan(data.Vertices), CollectionsMarshal.AsSpan(data.Indices));
        }

        public override void Render(CommandList cmdList)
        {
            cmdList.SetPipeline(Pipeline);
            cmdList.SetGraphicsResourceSet(0, Owner.World.ProjViewSet);

            var world = Owner.Transform;
            cmdList.UpdateBuffer(WorldBuffer, 0, ref world);

            cmdList.SetVertexBuffer(0, VertBuffer);
            cmdList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt32);

            //cmdList.UpdateBuffer(WorldBuffer, 0, ref Transform.World);
            cmdList.SetGraphicsResourceSet(1, ItemResourceSet);
            cmdList.DrawIndexed(IndiceCount, 1, 0, 0, 0);
        }

        public override void Update(double dt)
        {
            base.Update(dt);
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
