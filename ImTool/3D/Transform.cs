using ImGuiNET;
using System;
using System.Numerics;

namespace ImTool.Scene3D
{
    public struct Transform
    {
        public Vector3 Up      => Vector3.Transform(Vector3.UnitY, Rotation);
        public Vector3 Left    => Vector3.Transform(Vector3.UnitX, Rotation);
        public Vector3 Forward => Vector3.Transform(Vector3.UnitZ, Rotation);

        public Matrix4x4 World;

        public Vector3 Position
        {
            get
            {
                return World.Translation;
            }

            set
            {
                World.Translation = value;
            }
        }

        public Vector3 Scale
        {
            get
            {
                var col1Len = new Vector4(World.M11, World.M12, World.M13, World.M14).Length();
                var col2Len = new Vector4(World.M21, World.M22, World.M23, World.M24).Length();
                var col3Len = new Vector4(World.M31, World.M32, World.M33, World.M34).Length();
                var scale   = new Vector3(col1Len, col2Len, col3Len);

                return scale;
            }

            set
            {
                SetPosRotScale(Position, Rotation, value);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                var rot = Quaternion.CreateFromRotationMatrix(World);
                return rot;
            }

            set
            {
                SetPosRotScale(Position, value, Scale);
            }
        }

        // Get the rotation as yaw, pitch, roll in degrees
        public Vector3 RotationEuler
        {
            get
            {
                var yaw   = MathF.Atan2(2.0f * (Rotation.Y * Rotation.W + Rotation.X * Rotation.Z), 1.0f - 2.0f * (Rotation.X * Rotation.X + Rotation.Y * Rotation.Y));
                var pitch = MathF.Asin(2.0f * (Rotation.X * Rotation.W - Rotation.Y * Rotation.Z));
                var roll  = MathF.Atan2(2.0f * (Rotation.X * Rotation.Y + Rotation.Z * Rotation.W), 1.0f - 2.0f * (Rotation.X * Rotation.X + Rotation.Z * Rotation.Z));

                yaw   = (float)((180 / Math.PI) * yaw);
                pitch = (float)((180 / Math.PI) * pitch);
                roll  = (float)((180 / Math.PI) * roll);

                var rot = new Vector3(yaw, pitch, roll);
                return rot;
            }

            set
            {
                var yaw   = (float)((Math.PI / 180) * value.X);
                var pitch = (float)((Math.PI / 180) * value.Y);
                var roll  = (float)((Math.PI / 180) * value.Z);

                Rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            }
        }

        public Transform()
        {
            SetPosRotScale(Vector3.Zero, Quaternion.Identity, Vector3.One);
        }

        public void SetPosRotScale(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            var tMat = Matrix4x4.CreateTranslation(pos);
            var sMat = Matrix4x4.CreateScale(scale);
            var rMat = Matrix4x4.CreateFromQuaternion(rot);

            World = sMat * rMat * tMat;
        }

        public void Decompose()
        {
            var pos = World.Translation;

            var col1Len = new Vector4(World.M11, World.M12, World.M13, World.M14).Length();
            var col2Len = new Vector4(World.M21, World.M22, World.M23, World.M24).Length();
            var col3Len = new Vector4(World.M31, World.M32, World.M33, World.M34).Length();
            var scale = new Vector3(col1Len, col2Len, col3Len);

            var rot = Quaternion.CreateFromRotationMatrix(World);
        }

        // Draw an imgui widget for the values of this transform
        public void DrawImguiWidget(bool euler = true, bool showMatrix = false)
        {
            ImGui.Text("Position ");
            ImGui.SameLine();
            var pos = Position;
            if (Widgets.Vector3(ref pos, "Position"))
                Position = pos;

            ImGui.Text("Scale       ");
            ImGui.SameLine();
            var scale = Scale;
            if (Widgets.Vector3(ref scale, "Scale"))
                Scale = scale;

            ImGui.Text("Rotation");
            ImGui.SameLine();
            if (euler)
            {
                var rot = RotationEuler;
                if (Widgets.Vector3(ref rot, "Rotation"))
                    RotationEuler = rot;
            }
            else
            {
                var rot = Rotation;
                if (Widgets.Quaternion(ref rot, "Rotation"))
                    Rotation = rot;
            }

            if (showMatrix)
            {
                ImGui.NewLine();
                ImGui.Text("World Matrix");
                Widgets.Matrix4x4(ref World, "WorldMatrix");
            }
        }
    }
}
