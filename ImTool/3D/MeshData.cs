using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.ImageSharp;
using static ImTool.Scene3D.Components.MeshComponent;

namespace ImTool.Scene3D
{
    public class MeshData
    {
        public List<MeshComponent.VertexDefinition> Vertices;
        public List<uint> Indices;

        public static MeshData CreateCube()
        {
            var data = new MeshData
            {
                Vertices = new List<MeshComponent.VertexDefinition>() {
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
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                    // Right                                                              
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
                    // Back                                                               
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, +0.5f, -0.5f), new Vector2(0, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, +0.5f, -0.5f), new Vector2(1, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1, 1)),
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, -0.5f, -0.5f), new Vector2(0, 1)),
                    // Front                                                              
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, +0.5f, +0.5f), new Vector2(0, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, +0.5f, +0.5f), new Vector2(1, 0)),
                    new MeshComponent.VertexDefinition(new Vector3(+0.5f, -0.5f, +0.5f), new Vector2(1, 1)),
                    new MeshComponent.VertexDefinition(new Vector3(-0.5f, -0.5f, +0.5f), new Vector2(0, 1)),
                },

                Indices =
                new List<uint>() {
                    0, 1, 2, 0, 2, 3,
                    4, 5, 6, 4, 6, 7,
                    8, 9, 10, 8, 10, 11,
                    12, 13, 14, 12, 14, 15,
                    16, 17, 18, 16, 18, 19,
                    20, 21, 22, 20, 22, 23,
                }
            };

            return data;
        }

        public static MeshData LoadFromObj(string path)
        {
            var fileStream = File.OpenRead(path);
            var obj = new Veldrid.Utilities.ObjParser().Parse(fileStream);
            var data = new MeshData();

            var mesh = obj.GetFirstMesh();
            data.Indices = new List<uint>(mesh.GetIndices().Select(x => (uint)x));

            var verts = new MeshComponent.VertexDefinition[obj.Positions.Length];
            for (int i = 0; i < obj.Positions.Length; i++)
            {
                verts[i].X = obj.Positions[i].X;
                verts[i].Y = obj.Positions[i].Y;
                verts[i].Z = obj.Positions[i].Z;

                verts[i].NormX = obj.Normals[i].X;
                verts[i].NormY = obj.Normals[i].Y;
                verts[i].NormZ = obj.Normals[i].Z;
            }

            data.Vertices = new List<MeshComponent.VertexDefinition>(verts);

            return data;
        }

        public static MeshData LoadFromObjV2(string path)
        {
            var fileStream = File.OpenRead(path);
            var obj        = new Veldrid.Utilities.ObjParser().Parse(fileStream);
            var data       = new MeshData();
            data.Vertices  = new List<MeshComponent.VertexDefinition>();
            data.Indices   = new List<uint>();

            uint lastIndice = 0;
            foreach (var group in obj.MeshGroups)
            {
                var mesh = obj.GetMesh(group);
                data.Vertices.AddRange(mesh.Vertices.Select(x => new MeshComponent.VertexDefinition()
                {
                    X = x.Position.X,
                    Y = x.Position.Y,
                    Z = x.Position.Z,

                    NormX = x.Normal.X,
                    NormY = x.Normal.Y,
                    NormZ = x.Normal.Z,
                }).ToList());

                data.Indices.AddRange(new List<uint>(mesh.GetIndices().Select(x => (uint)lastIndice + x)));
                lastIndice = (uint)data.Vertices.Count();

                /*foreach (var face in group.Faces)
                {
                    var vert = new MeshComponent.VertexDefinition();
                    vert.X   = obj.Positions[face.Vertex0.PositionIndex -1].X;
                    vert.Y   = obj.Positions[face.Vertex1.PositionIndex -1].Y;
                    vert.Z   = obj.Positions[face.Vertex2.PositionIndex -1].Z;

                    vert.NormX = obj.Normals[face.Vertex0.NormalIndex -1].X;
                    vert.NormY = obj.Normals[face.Vertex1.NormalIndex -1].Y;
                    vert.NormZ = obj.Normals[face.Vertex2.NormalIndex -1].Z;

                    data.Vertices.Add(vert);
                    data.Indices.Add(face.Vertex0.);
                    data.Indices.Add((uint)data.Indices.Count());
                    data.Indices.Add((uint)data.Indices.Count());
                }*/
            }

            return data;
        }

        public static IEnumerable<MeshSection> LoadObjMtl(string path, GraphicsDevice gd, ResourceFactory rf)
        {
            var lines      = File.ReadAllLines(path);
            var mats       = new List<MeshSection>();
            var currentMat = new MeshSection();
            var basePath   = Path.GetDirectoryName(path);
            foreach (var line in lines)
            {
                if (line != null && !line.StartsWith("#"))
                {
                    if (line.StartsWith("newmtl", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentMat != null)
                            mats.Add(currentMat);

                        currentMat = new MeshSection();
                        currentMat.Name = line.Split(' ')[1];
                    }
                    else if (line.StartsWith("map_kd", StringComparison.OrdinalIgnoreCase))
                    {
                        var texPath = Path.Combine(basePath, line.Split(" ")[1]);
                        if (File.Exists(texPath))
                        {
                            var texImg            = new ImageSharpTexture(texPath);
                            currentMat.DiffuseTex = texImg.CreateDeviceTexture(gd, rf);
                        }
                    }
                }
            }

            return mats;
        }
    }
}
