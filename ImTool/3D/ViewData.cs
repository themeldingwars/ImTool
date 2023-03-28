using System.Numerics;
using System.Runtime.InteropServices;

namespace ImTool.Scene3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ViewData
    {
        public static uint SIZE = (64 * 2) + (12 * 2) + (4 * 6);

        public Matrix4x4 View;
        public Matrix4x4 Proj;
        public Vector3 CamPos;
        private float _padding1;
        public Vector3 CamDir;
        private float _padding2;
        public float CamNearDist;
        public float CamFarDist;

        private float _padding3;
        private float _padding4;
    }
}
