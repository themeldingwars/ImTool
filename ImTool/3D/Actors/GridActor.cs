﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
//using Veldrid.SPIRV;

namespace ImTool.Scene3D
{
    public class GridActor : Actor
    {
        private DeviceBuffer VertBuffer;
        private DeviceBuffer IndexBuffer;
        private Pipeline Pipeline;
        private ShaderSetDescription ShaderSet;
        private ResourceLayout PerItemResourceLayout;

        public override void Init(World world)
        {
            base.Init(world);

            CreateResources();
        }

        private void CreateResources()
        {
            var halfSize = 10f;
            var verts = new VertexDefinition[]
            {
                new (-halfSize, 0f, +halfSize, 0f, 0f),
                new (+halfSize, 0f, +halfSize, 1f, 0f),
                new (+halfSize, 0f, -halfSize, 1f, 1f),
                new (-halfSize, 0f, -halfSize, 0f, 1f),
                new (-halfSize, 0f, -halfSize, 0f, 1f),
                new (-halfSize, 0f, -halfSize, 0f, 1f)
            };

            var indices = new ushort[]
            {
                0, 1, 2, 3, 4, 5
            };

            var gd = World.MainWindow.GetGraphicsDevice();
            VertBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(VertexDefinition.SizeInBytes * verts.Length), BufferUsage.VertexBuffer));
            gd.UpdateBuffer(VertBuffer, 0, verts);

            IndexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(sizeof(ushort) * (uint)indices.Length, BufferUsage.IndexBuffer));
            gd.UpdateBuffer(IndexBuffer, 0, indices);

            ShaderSet             = CreateShaderSet();
            PerItemResourceLayout = CreatePerItemResourceLayout();

            Pipeline = gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription(
                BlendStateDescription.SingleAlphaBlend,
                DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                ShaderSet,
                new[] { World.ProjViewLayout, PerItemResourceLayout },
                World.CurrentSceneViewport.GetFramebuffer().OutputDescription));
        }

        private ShaderSetDescription CreateShaderSet()
        {
                                                            //ImTool.Shaders.SPIR_V._3D.Grid.GridFrag.glsl
            var gridVert    = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Grid.GridVert.glsl", ShaderStages.Vertex);
            var gridFrag    = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.Grid.GridFrag.glsl", ShaderStages.Fragment);
            var gridShaders = new Shader[2]; // World.MainWindow.GetGraphicsDevice().ResourceFactory.CreateFromSpirv(gridVert, gridFrag);

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
            ResourceLayout worldTextureLayout = World.MainWindow.GetGraphicsDevice().ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    )
                );

            return worldTextureLayout;
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
    }
}