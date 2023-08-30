using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImTool.Scene3D
{
    public struct PerItemData
    {
        public const int SIZE = 64 + 4 + (4 * 3);

        public Matrix4x4 Mat;
        public SelectableID SelectionId;
        public float _padding1;
        public float _padding2;
        public float _padding3;
    }
}
