using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImTool.Scene3D
{
    public static class NumericsExtrensions
    {
        public static float[] ToFloatArrray(this Matrix4x4 mat)
        {
            var arr = new float[]
            {
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
            };

            return arr;
        }

        public static void FromFloatArray(this ref Matrix4x4 mat, float[] values)
        {
            mat.M11 = values[0];
            mat.M12 = values[1];
            mat.M13 = values[2];
            mat.M14 = values[3];

            mat.M21 = values[4];
            mat.M22 = values[5];
            mat.M23 = values[6];
            mat.M24 = values[7];

            mat.M31 = values[8];
            mat.M32 = values[9];
            mat.M33 = values[10];
            mat.M34 = values[11];

            mat.M41 = values[12];
            mat.M42 = values[13];
            mat.M43 = values[14];
            mat.M44 = values[15];
        }
    }
}
