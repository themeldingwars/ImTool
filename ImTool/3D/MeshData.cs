using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
    }
}
