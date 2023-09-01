using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;
using Vortice.Mathematics;

namespace ImTool.Scene3D
{
    public class SelectedOutlinePostProcess
    {
        public DeviceBuffer SettingsBuffer;
        public ResourceSet ItemResourceSet;
        private DeviceBuffer VertBuffer;
        private DeviceBuffer IndexBuffer;
        private Pipeline Pipeline;
        private ShaderSetDescription ShaderSet;
        private ResourceLayout PerItemResourceLayout;

        public void Init()
        {
            CreateResources();
        }

        private void CreateResources()
        {
            var halfSize = 1f;
            var verts = new VertexDefinition[]
            {
                new (1f, 1f, 0, 1f, 1f),
                new (-1f, -1f, 0f, 0f, 0f),
                new (-1, 1f, 0, 0f, 1f),
                new (-1, -1, 0, 0f, 0f),
                new (1, 1, 0, 1f, 1f),
                new (1, -1, 0, 1f, 0f)
            };

            var indices = new ushort[]
            {
                0, 1, 2, 3, 4, 5
            };

            VertBuffer = Resources.GD.ResourceFactory.CreateBuffer(new BufferDescription((uint)(VertexDefinition.SizeInBytes * verts.Length), BufferUsage.VertexBuffer));
            Resources.GD.UpdateBuffer(VertBuffer, 0, verts);

            IndexBuffer = Resources.GD.ResourceFactory.CreateBuffer(new BufferDescription(sizeof(ushort) * (uint)indices.Length, BufferUsage.IndexBuffer));
            Resources.GD.UpdateBuffer(IndexBuffer, 0, indices);

            SettingsBuffer = Resources.GD.ResourceFactory.CreateBuffer(new BufferDescription(OutlineData.SIZE, BufferUsage.UniformBuffer));

            ShaderSet = CreateShaderSet();
            PerItemResourceLayout = CreatePerItemResourceLayout();

            var sampler = Resources.GD.ResourceFactory.CreateSampler(SamplerDescription.Point);
            //ItemResourceSet = Resources.GD.ResourceFactory.CreateResourceSet(new ResourceSetDescription(PerItemResourceLayout, WorldBuffer,
            //World.GetVieewports()[0].ActorIdTex, sampler));

            //var world = Matrix4x4.CreateTranslation(Vector3.Zero);
            var settings = new OutlineData()
            {
                Color     = Colors.DarkOrange.ToVector4(),
                Thickness = 4
            };
            Resources.GD.UpdateBuffer(SettingsBuffer, 0, ref settings);

            Pipeline = Resources.GD.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                new DepthStencilStateDescription(false, false, ComparisonKind.Never),
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                ShaderSet,
                new[] { Resources.ProjViewLayout, PerItemResourceLayout },
                Resources.MainFrameBufferOutputDescription));
        }

        private ShaderSetDescription CreateShaderSet()
        {
            //ImTool.Shaders.SPIR_V._3D.Grid.GridFrag.glsl
            var gridVert = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Fullscreen.Outline.vert", ShaderStages.Vertex);
            var gridFrag = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Fullscreen.Outline.frag", ShaderStages.Fragment);
            var gridShaders = Resources.GD.ResourceFactory.CreateFromSpirv(gridVert, gridFrag);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Uv", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                        )
                },
                gridShaders);

            return shaderSet;
        }

        private ResourceLayout CreatePerItemResourceLayout()
        {
            ResourceLayout worldTextureLayout = Resources.GD.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("OutlineData", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("IdBuffer", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("DiffusIdBufferSampler", ResourceKind.Sampler, ShaderStages.Fragment)
                    )
                );

            return worldTextureLayout;
        }

        public void UdpateFBResources(MainFrameBufferResource fbRes)
        {
            //var sampler = Resources.GD.ResourceFactory.CreateSampler(SamplerDescription.Point);

            if (ItemResourceSet != null && !ItemResourceSet.IsDisposed)
            {
                ItemResourceSet.Dispose();
            }

            ItemResourceSet = Resources.GD.ResourceFactory.CreateResourceSet(new ResourceSetDescription(PerItemResourceLayout, SettingsBuffer,
            fbRes.SelectedMaskTex, Resources.GD.Aniso4xSampler));
        }

        public void Render(CommandList cmdList, MainFrameBufferResource fbRes)
        {
            cmdList.SetPipeline(Pipeline);

            cmdList.SetVertexBuffer(0, VertBuffer);
            cmdList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt16);

            cmdList.SetGraphicsResourceSet(1, ItemResourceSet);
            cmdList.DrawIndexed(6, 1, 0, 0, 0);
        }

        public struct VertexDefinition
        {
            public const uint SizeInBytes = 20;

            public float X;
            public float Y;
            public float Z;

            public float U;
            public float V;

            public VertexDefinition(float x, float y, float z, float u, float v)
            {
                X = x;
                Y = y;
                Z = z;

                U = u;
                V = v;
            }
        }

        public struct OutlineData
        {
            public const uint SIZE = 48;

            public Vector4 Color;
            public float Thickness;

            public float _padding1;
            public float _padding2;
            public float _padding3;
        }
    }
}
