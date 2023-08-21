using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;
using Vulkan;
using static ImTool.Scene3D.Components.DebugShapesComp;

namespace ImTool.Scene3D.Components
{
    public class DebugShapesComp : Component
    {
        public DeviceBuffer WorldBuffer;
        public ResourceSet ItemResourceSet;
        private DeviceBuffer VertBuffer;
        private DeviceBuffer IndexBuffer;
        private Pipeline Pipeline;
        private ShaderSetDescription ShaderSet;
        private ResourceLayout PerItemResourceLayout;

        public List<Shape> Shapes = new();

        private List<VertexDefinition> Verts                    = new();
        private List<uint> Indices                              = new();
        private bool ShouldRecreateBuffers                      = false;
        private bool ShouldReuploadBuffers                      = false;

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
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.LineList,
                ShaderSet,
                new[] { Resources.ProjViewLayout, PerItemResourceLayout },
                Resources.MainFrameBufferOutputDescription));

            ResizeBuffers();
        }

        public ShaderSetDescription CreateShaderSet(ResourceFactory rf)
        {
            var vert      = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.DebugShapes.DebugShapesVert.glsl", ShaderStages.Vertex);
            var geo       = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.DebugShapes.DebugShapesGeo.glsl", ShaderStages.Geometry);
            var frag      = Resources.LoadEmbeddedShader("ImTool.Shaders.SPIR_V._3D.DebugShapes.DebugShapesFrag.glsl", ShaderStages.Fragment);
            var shaders   = rf.CreateFromSpirv(vert, frag);
            var geoShader = rf.CreateFromSpirv(geo);

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                        new VertexElementDescription("Thickness", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1))
                },
                new Shader[] { shaders[0], shaders[1], geoShader });

            return shaderSet;
        }

        public ResourceLayout CreatePerItemResourceLayout(ResourceFactory rf)
        {
            ResourceLayout worldTextureLayout = rf.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("WorldBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );

            return worldTextureLayout;
        }

        public override void Render(CommandList cmdList)
        {
            base.Render(cmdList);

            cmdList.SetPipeline(Pipeline);
            cmdList.SetGraphicsResourceSet(0, Owner.World.ProjViewSet);

            var world = Owner.Transform.World;
            cmdList.UpdateBuffer(WorldBuffer, 0, ref world);

            cmdList.SetVertexBuffer(0, VertBuffer);
            cmdList.SetIndexBuffer(IndexBuffer, IndexFormat.UInt32);

            cmdList.UpdateBuffer(WorldBuffer, 0, ref world);
            cmdList.SetGraphicsResourceSet(1, ItemResourceSet);
            cmdList.DrawIndexed((uint)Indices.Count, 1, 0, 0, 0);
        }

        public override void Update(double dt)
        {
            base.Update(dt);

            if (ShouldRecreateBuffers)
                RecreateBuffers();
        }

        public void RecreateBuffers()
        {
            Verts.Clear();
            Indices.Clear();

            uint numIndices = 0;
            for (int i = 0; i < Shapes.Count; i++)
            {
                var shape = Shapes[i];
                var verts = shape.GetShapeVerts();
                var indices = shape.GetIndices(numIndices);
                numIndices += (uint)(verts.Length);

                Verts.AddRange(verts);
                Indices.AddRange(indices);
            }

            ResizeBuffers();
            var gd = Owner.World.MainWindow.GetGraphicsDevice();
            gd.UpdateBuffer(VertBuffer, 0, CollectionsMarshal.AsSpan(Verts));
            gd.UpdateBuffer(IndexBuffer, 0, CollectionsMarshal.AsSpan(Indices));

            ShouldRecreateBuffers = false;
        }

        private void ResizeBuffers()
        {
            var gd = Owner.World.MainWindow.GetGraphicsDevice();
            var vertsSizeInBytes = (VertexDefinition.SizeInBytes * (uint)Verts.Count);
            var indiceSizeInBytes = sizeof(uint) * (uint)Indices.Count;

            if (VertBuffer == null || vertsSizeInBytes > VertBuffer.SizeInBytes)
            {
                if (VertBuffer != null) gd.DisposeWhenIdle(VertBuffer);
                VertBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(vertsSizeInBytes, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            }

            if (IndexBuffer == null || indiceSizeInBytes > IndexBuffer.SizeInBytes)
            {
                if (IndexBuffer != null) gd.DisposeWhenIdle(IndexBuffer);
                IndexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription(indiceSizeInBytes, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            }
        }

        #region Shape add functions

        public Cube AddCube(Vector3 pos, Vector3? size = null, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Cube(this, pos, size, color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        public Rect AddRect(Vector3 pos, Vector2 size, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Rect(this, pos, new Vector3(size.X, 0, size.Y), color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        public Line AddLine(Vector3 start, Vector3 end, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Line(this, start, end, color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        public Cricle AddCricle(Vector3 pos, float radius = 1f, ushort sides = 16, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Cricle(this, pos, radius, sides, color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        public Cylnder AddCylnder(Vector3 pos, float radius = 1f, float height = 2f, ushort sides = 16, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Cylnder(this, pos, radius, height, sides, color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        public Sphere AddSphere(Vector3 pos, float radius = 1f, ushort sides = 16, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Sphere(this, pos, radius, sides, color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        public Arrow AddArrow(Vector3 pos, Vector3 dir, ushort sides = 16, Vector4? color = null, float thickness = 2f)
        {
            var shape = new Arrow(this, pos, dir, color, thickness);
            Shapes.Add(shape);
            ShouldRecreateBuffers = true;
            return shape;
        }

        #endregion

        #region Shape Classes

        public class Shape
        {
            public Transform Transform = new();
            public Vector4 Color;
            public float Thickness;
            protected DebugShapesComp Owner;

            private readonly Vector4 DefaultColor = new(1f, 0.2f, 0.2f, 1);

            public Shape(DebugShapesComp owner, Vector3 pos, Vector4? color = null, float thickness = 2f)
            {
                Owner              = owner;
                Transform.Position = pos;
                Transform.Rotation = Quaternion.Identity;
                Transform.Scale    = Vector3.One;
                Thickness          = thickness;
                Color              = color ?? DefaultColor;
            }

            public void Remove()
            {
                Owner.Shapes.Remove(this);
                Owner.ShouldRecreateBuffers = true;
            }

            public virtual VertexDefinition[] GetShapeVerts()
            {
                return null;
            }

            public virtual uint[] GetIndices(uint start)
            {
                return null;
            }
        }

        public class Line : Shape
        {
            public Vector3 End;

            public Line(DebugShapesComp owner,  Vector3 start, Vector3 end, Vector4? color = null, float thickness = 2) : base(owner, start, color, thickness)
            {
                End   = end;
            }

            public static VertexDefinition[] GenVerts(Transform transform, Vector3 start, Vector3 end, Vector4 color, float thickness)
            {
                VertexDefinition[] values =
                {
                    new(start, color, thickness),
                    new(end, color, thickness),
                };

                return values;
            }

            public static uint[] GenIndices(uint offset)
            {
                var indices = new[]
                {
                    (uint) (offset + 0), (uint) (offset + 1),
                    (uint) (offset + 1), (uint) (offset + 2),
                };

                return indices;
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                return GenVerts(Transform, Transform.Position, End, Color, Thickness);
            }

            public override uint[] GetIndices(uint offset)
            {
                return GenIndices(offset);
            }
        }

        public class Rect : Shape
        {
            public Vector3 Extents = new(0.5f, 0f, 0.5f);

            public Rect(DebugShapesComp owner, Vector3 pos, Vector3? extents, Vector4? color = null, float thickness = 2) : base(owner, pos, color, thickness)
            {
                Extents = extents ?? Extents;
            }

            public static VertexDefinition[] GenVerts(Transform transform, Vector3 extents, Vector4 color, float thickness)
            {
                VertexDefinition[] values =
                {
                    new(Vector3.Transform(new Vector3(-extents.X, +extents.Y, -extents.Z), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(+extents.X, +extents.Y, -extents.Z), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(+extents.X, +extents.Y, +extents.Z), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(-extents.X, +extents.Y, +extents.Z), transform.World), color, thickness)
                };

                return values;
            }

            public static uint[] GenIndices(uint offset)
            {
                var indices = new[]
                {
                    (uint) (offset + 0), (uint) (offset + 1),
                    (uint) (offset + 1), (uint) (offset + 2),
                    (uint) (offset + 2), (uint) (offset + 3),
                    (uint) (offset + 3), (uint) (offset + 0)
                };

                return indices;
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                return GenVerts(Transform, Extents, Color, Thickness);
            }

            public override uint[] GetIndices(uint offset)
            {
                return GenIndices(offset);
            }
        }

        public class Cube : Shape
        {
            public Vector3 Extents = new(0.5f, 0.5f, 0.5f);

            public Cube(DebugShapesComp owner, Vector3 pos, Vector3? extents, Vector4? color = null, float thickness = 2f) : base(owner, pos, color, thickness)
            {
                Extents = extents ?? Extents;
            }

            public void FromBoundingBox(BoundingBox bbox)
            {
                var size           = bbox.GetDimensions();
                var center         = bbox.GetCenter();
                Transform          = new Transform();
                Transform.Position = Transform.Position  + (center);
                Extents            = size / 2;
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                var values = new VertexDefinition[8];
                var extents = Extents;
                var top = Rect.GenVerts(Transform, extents, Color, Thickness).AsSpan();
                extents.Y = -Extents.Y;
                var bottom = Rect.GenVerts(Transform, extents, Color, Thickness).AsSpan();

                var valuesSpan = values.AsSpan();
                top.CopyTo(valuesSpan.Slice(0, 4));
                bottom.CopyTo(valuesSpan.Slice(4, 4));

                return values;
            }

            public override uint[] GetIndices(uint start)
            {
                var indices = new uint[24];
                var connectingLines = new[]
                {
                    (uint) (start + 0), (uint) (start + 4),
                    (uint) (start + 1), (uint) (start + 5),
                    (uint) (start + 2), (uint) (start + 6),
                    (uint) (start + 3), (uint) (start + 7)
                }.AsSpan();

                var indicesSpan = indices.AsSpan();
                Rect.GenIndices(start).AsSpan().CopyTo(indicesSpan);
                Rect.GenIndices((uint)(start + 4)).AsSpan().CopyTo(indicesSpan.Slice(8, 8));
                connectingLines.CopyTo(indicesSpan.Slice(16, 8));

                return indices;
            }
        }

        public class Cricle : Shape
        {
            public float Radius = 1f;
            public ushort Sides = 16;

            public Cricle(DebugShapesComp owner, Vector3 pos, float radius = 1f, ushort sides = 16, Vector4? color = null, float thickness = 2) : base(owner, pos, color, thickness)
            {
                Radius = radius;
                Sides = sides;
            }

            public static VertexDefinition[] GenVerts(Transform transform, float radius, ushort sides, Vector4 color, float thickness)
            {
                var verts = new VertexDefinition[sides];
                var segmentRad = (MathF.PI * 2) / sides;
                for (int i = 0; i < sides; i++)
                {
                    var pos = new Vector3(MathF.Sin(segmentRad * i) * radius, 0, MathF.Cos(segmentRad * i) * radius);
                    verts[i] = new(Vector3.Transform(pos, transform.World), color, thickness);
                }

                return verts;
            }

            public static uint[] GenIndices(uint offset, ushort sides)
            {
                var vals = new uint[sides * 2];
                for (int i = 0; i < sides; i++)
                {
                    var idx = i * 2;
                    var idx2 = idx + 1;
                    vals[idx] = (uint)(offset + i);
                    vals[idx2] = (uint)(offset + i + 1);
                }

                vals[(sides * 2) - 1] = (uint)(offset);

                return vals;
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                return GenVerts(Transform, Radius, Sides, Color, Thickness);
            }

            public override uint[] GetIndices(uint start)
            {
                return GenIndices(start, Sides);
            }
        }

        public class Cylnder : Cricle
        {
            public float Height = 2f;

            public Cylnder(DebugShapesComp owner, Vector3 pos, float radius = 1, float height = 2f, ushort sides = 16, Vector4? color = null, float thickness = 2) : base(owner, pos, radius, sides, color, thickness)
            {
                Height = height;
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                var orgPos = Transform.Position;
                Transform.Position = new Vector3(orgPos.X, orgPos.Y + (Height / 2), orgPos.Z);
                var top = GenVerts(Transform, Radius, Sides, Color, Thickness);
                Transform.Position = new Vector3(orgPos.X, orgPos.Y - (Height / 2), orgPos.Z);
                var bottom = GenVerts(Transform, Radius, Sides, Color, Thickness);

                Transform.Position = orgPos;

                var verts = new VertexDefinition[top.Length * 2];
                var vertsSpan = verts.AsSpan();
                top.AsSpan().CopyTo(vertsSpan.Slice(0, top.Length));
                bottom.AsSpan().CopyTo(vertsSpan.Slice(top.Length, bottom.Length));

                return verts;
            }

            public override uint[] GetIndices(uint start)
            {
                var topIndices = GenIndices(start, Sides);
                var bottomIndices = GenIndices((uint)(start + Sides), Sides);

                var values = new uint[(topIndices.Length * 2) + Sides * 2];
                var vSapn = values.AsSpan();
                topIndices.AsSpan().CopyTo(vSapn.Slice(0, topIndices.Length));
                bottomIndices.AsSpan().CopyTo(vSapn.Slice(topIndices.Length, bottomIndices.Length));

                var baseIdx = topIndices.Length + bottomIndices.Length;
                for (int i = 0; i < Sides; i += 2)
                {
                    values[baseIdx + i] = (uint)(start + i);
                    values[baseIdx + i + 1] = (uint)(start + Sides + i);
                }

                return values;
            }
        }

        public class Sphere : Cricle
        {
            public Sphere(DebugShapesComp owner, Vector3 pos, float radius = 1, ushort sides = 16, Vector4? color = null, float thickness = 2) : base(owner, pos, radius, sides, color, thickness)
            {
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                var verts = new     VertexDefinition[Sides * 3];
                var circleX = GenVerts(Transform, Radius, Sides, Color, Thickness);
                Transform.RotationEuler = new Vector3(0, 0, 90);
                var circleY = GenVerts(Transform, Radius, Sides, Color, Thickness);
                Transform.RotationEuler = new Vector3(0, 90, 0);
                var circleZ = GenVerts(Transform, Radius, Sides, Color, Thickness);

                circleX.AsSpan().CopyTo(verts.AsSpan().Slice(0, circleX.Length));
                circleY.AsSpan().CopyTo(verts.AsSpan().Slice(circleX.Length, circleY.Length));
                circleZ.AsSpan().CopyTo(verts.AsSpan().Slice(circleY.Length * 2, circleZ.Length));

                return verts;
            }

            public override uint[] GetIndices(uint start)
            {
                var vals = new uint[(Sides * 3) * 2];
                var offset = 0;
                for (int i = 0; i < 3; i++)
                {
                    var idxVals = GenIndices((uint)(start + (i * Sides)), Sides);
                    idxVals.AsSpan().CopyTo(vals.AsSpan().Slice(offset, idxVals.Length));
                    offset += idxVals.Length;
                }

                return vals;
            }
        }

        public class Arrow : Shape
        {
            public Vector3 Dir = Vector3.UnitZ;
            public float Length = 1f;

            public Arrow(DebugShapesComp owner, Vector3 pos, Vector3 dir, Vector4? color = null, float thickness = 2) : base(owner,pos, color, thickness)
            {
                var pitch = MathF.Asin(-dir.Y);
                var yaw = MathF.Atan2(dir.X, dir.Z);
                //Transform.Positon  += dir;
                Transform.Rotation = Quaternion.CreateFromYawPitchRoll(-yaw, -pitch, 0);

                var offset = Vector3.Transform(Transform.Forward, Transform.Rotation);
                Transform.Position -= offset;
                Length = dir.Length();
            }

            public static VertexDefinition[] GenVerts(Transform transform, Vector4 color, float thickness, float length = 1f, float headLength = 0.4f, float headWidth = 0.2f)
            {
                var verts = new VertexDefinition[]
                {
                    new(Vector3.Transform(new Vector3(0, 0, 0f), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(0, 0, length), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(0, -headWidth, headLength), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(0, headWidth, headLength), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(-headWidth, 0f, headLength), transform.World), color, thickness),
                    new(Vector3.Transform(new Vector3(headWidth, 0f, headLength), transform.World), color, thickness)
                };

                return verts;
            }

            public static uint[] GenIndices(uint offset)
            {
                var vals = new uint[]
                {
                    offset, offset + 1,
                    offset, offset + 2,
                    offset, offset + 3,
                    offset, offset + 4,
                    offset, offset + 5
                };

                return vals;
            }

            public override VertexDefinition[] GetShapeVerts()
            {
                return GenVerts(Transform, Color, Thickness, Length);
            }

            public override uint[] GetIndices(uint start)
            {
                return GenIndices(start);
            }
        }



        #endregion

        public struct VertexDefinition
        {
            public const uint SizeInBytes = 12 + 16 + 4;

            public Vector3 Position;
            public Vector4 Color;
            public float Thickness;

            public VertexDefinition(Vector3 pos, Vector4 color, float thickness)
            {
                Position = pos;
                Color = color;
                Thickness = thickness;
            }
        }
    }
}
