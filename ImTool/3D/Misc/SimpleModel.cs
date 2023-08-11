using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;
using Vulkan;
using static ImTool.Scene3D.Components.MeshComponent;

namespace ImTool.Scene3D
{
    public partial class SimpleModel : IDisposable
    {
        public ResourceSet ItemResourceSet;
        public DeviceBuffer VertBuffer;
        public DeviceBuffer IndexBuffer;
        public List<MeshSection> MeshSections = new List<MeshSection>();

        public Pipeline Pipeline;
        private ShaderSetDescription ShaderSet;
        public ResourceLayout PerItemResourceLayout;
        public ResourceLayout PerSectionResLayout;
        private ResourceSet DefaultPerSectionResSet;

        public SimpleModel()
        {
            Init();
        }

        public void Init()
        {
            var rf                = Resources.GD.ResourceFactory;
            ShaderSet             = CreateShaderSet(rf);
            PerItemResourceLayout = CreatePerItemResourceLayout(rf);

            PerSectionResLayout = rf.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("DiffuseTex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("DiffuseSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            )
            );

            DefaultPerSectionResSet = CreateTexResourceSet(Resources.GetMissingTex());

            Pipeline = rf.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                new RasterizerStateDescription(FaceCullMode.Front, PolygonFillMode.Solid, FrontFace.CounterClockwise, true, true),
                //RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleList,
                ShaderSet,
                new[] { Resources.ProjViewLayout, PerItemResourceLayout, PerSectionResLayout },
                Resources.MainFrameBufferOutputDescription));
        }

        private ShaderSetDescription CreateShaderSet(ResourceFactory rf)
        {
            var vert = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Mesh.MeshVert.glsl", ShaderStages.Vertex);
            var frag = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Mesh.MeshFrag.glsl", ShaderStages.Fragment);
            var shaders = rf.CreateFromSpirv(vert, frag);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    // SimpleVertexDefinition
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Uvs", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Norms", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
                        )
                },
                shaders);

            return shaderSet;
        }

        public ResourceSet CreateTexResourceSet(Texture tex)
        {
            var rf = Resources.GD.ResourceFactory;
            var resSet = rf.CreateResourceSet(new ResourceSetDescription(PerSectionResLayout,
                tex,
                Resources.GD.Aniso4xSampler
            ));

            return resSet;
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

        public void FullUpdateVertBuffer(ReadOnlySpan<SimpleVertexDefinition> verts)
        {
            if (VertBuffer != null) Resources.GD.DisposeWhenIdle(VertBuffer);
            VertBuffer = Resources.GD.ResourceFactory.CreateBuffer(new BufferDescription((uint)(SimpleVertexDefinition.SizeInBytes * verts.Length), BufferUsage.VertexBuffer));
            Resources.GD.UpdateBuffer(VertBuffer, 0, verts);
        }

        public void FullUpdateIndices(ReadOnlySpan<uint> indices)
        {
            if (IndexBuffer != null) Resources.GD.DisposeWhenIdle(IndexBuffer);
            IndexBuffer = Resources.GD.ResourceFactory.CreateBuffer(new BufferDescription((uint)(sizeof(uint) * indices.Length), BufferUsage.IndexBuffer));
            Resources.GD.UpdateBuffer(IndexBuffer, 0, indices);
        }

        public static SimpleModel CreateFromCube()
        {
            var model = new SimpleModel();
            var verts = new SimpleVertexDefinition[] 
            {
                // Top
                new(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                new(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                new(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 1)),
                new(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 1)),
                // Bottom                                                             
                new(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 0)),
                new(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 0)),
                new(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                new(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                // Left                                                               
                new SimpleVertexDefinition(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                new SimpleVertexDefinition(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
                new SimpleVertexDefinition(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
                new SimpleVertexDefinition(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                // Right                                                              
                new SimpleVertexDefinition(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
                new SimpleVertexDefinition(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                new SimpleVertexDefinition(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                new SimpleVertexDefinition(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
                // Back                                                               
                new SimpleVertexDefinition(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                new SimpleVertexDefinition(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                new SimpleVertexDefinition(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                new SimpleVertexDefinition(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                // Front                                                              
                new SimpleVertexDefinition(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
                new SimpleVertexDefinition(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
                new SimpleVertexDefinition(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
                new SimpleVertexDefinition(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
            };
            var indices = new uint[]
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23,
            };
            var section = new MeshSection()
            {
                Name           = "Main",
                IndiceStart    = 0,
                IndicesLength  = (uint)indices.Length,
                DiffuseTex     = Resources.GetMissingTex(),
                TexResourceSet = model.CreateTexResourceSet(Resources.GetMissingTex())
            };

            model.MeshSections.Add(section);

            model.FullUpdateVertBuffer(verts);
            model.FullUpdateIndices(indices);

            return model;
        }

        public void Dispose()
        {
            ItemResourceSet.Dispose();
            VertBuffer.Dispose();
            IndexBuffer.Dispose();
            MeshSections.Clear();
            PerSectionResLayout.Dispose();
            DefaultPerSectionResSet.Dispose();
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
    }
}
