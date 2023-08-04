﻿using ImTool.Scene3D.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    public class MeshActor : Actor
    {
        public MeshComponent Mesh;

        public override void Init(World world)
        {
            base.Init(world);

            Mesh = AddComponet<MeshComponent>();
        }
    }
}