using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid.ImageSharp;
using Veldrid;
using static ImTool.Scene3D.Components.MeshComponent;
using System.Runtime.InteropServices;

namespace ImTool.Scene3D
{
    public partial class SimpleModel
    {

        public static SimpleModel CreateFromObj(string path)
        {
            var model = new SimpleModel();

            var fileStream = File.OpenRead(path);
            var obj        = new Veldrid.Utilities.ObjParser().Parse(fileStream);
            var mtlPath    = Path.Combine(Path.GetDirectoryName(path), obj.MaterialLibName);
            var mtl        = model.LoadObjMtl(mtlPath);
            var vertices   = new List<SimpleVertexDefinition>();
            var indices    = new List<uint>();

            model.MeshSections = new List<MeshSection>();

            uint lastIndice = 0;
            foreach (var group in obj.MeshGroups)
            {
                var mesh = obj.GetMesh(group);
                vertices.AddRange(mesh.Vertices.Select(x => new SimpleVertexDefinition()
                {
                    X = x.Position.X,
                    Y = x.Position.Y,
                    Z = x.Position.Z,

                    U = x.TextureCoordinates.X,
                    V = x.TextureCoordinates.Y,

                    NormX = x.Normal.X,
                    NormY = x.Normal.Y,
                    NormZ = x.Normal.Z,
                }).ToList());


                var groupIndices = mesh.GetIndices();
                // try load textures
                var diffuseTexpath    = group.Material;
                var matData           = mtl.FirstOrDefault(x => x.Name == diffuseTexpath) ?? new MeshSection();
                matData.IndiceStart   = (uint)indices.Count();
                matData.IndicesLength = (uint)groupIndices.Length;
                model.MeshSections.Add(matData);

                /*model.MeshSections.Add(new MeshSection()
                {
                    Name = group.Name,
                    IndiceStart = (uint)indices.Count(),
                    IndicesLength = (uint)groupIndices.Length,
                    DiffuseTex = matData?.DiffuseTex,
                    TexResourceSet = matData?.TexResourceSet
                });*/

                indices.AddRange(new List<uint>(groupIndices.Select(x => (uint)lastIndice + x)));
                lastIndice = (uint)vertices.Count();
            }

            model.FullUpdateVertBuffer(CollectionsMarshal.AsSpan(vertices));
            model.FullUpdateIndices(CollectionsMarshal.AsSpan(indices));

            return model;
        }

        public IEnumerable<MeshSection> LoadObjMtl(string path)
        {
            var lines = File.ReadAllLines(path);
            var mats = new List<MeshSection>();
            var currentMat = new MeshSection();
            var basePath = Path.GetDirectoryName(path);
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
                            var tex     = Resources.RequestTexture(texPath);
                            currentMat.TexResourceSet = CreateTexResourceSet(tex);
                        }
                    }
                }
            }

            return mats;
        }
    }
}
