using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public static partial class Widgets
    {
        private const uint X_AXIS_COLOR = 0xFF0000b1;
        private const uint Y_AXIS_COLOR = 0xFF00b100;
        private const uint Z_AXIS_COLOR = 0xFFb10000;
        private const uint W_AXIS_COLOR = 0xFF8B008B;

        public static bool FloatLabel(ref float val, string name, uint color = 0xFF0000b1)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);

            var     textSize  = ImGui.CalcTextSize(name);
            Vector2 labelSize = new Vector2(textSize.X + 7f, 2f);

            Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
            Vector2 pMax            = cursorScreenPos + new Vector2(labelSize.X, ImGui.GetFrameHeight());

            var dl = ImGui.GetWindowDrawList();
            dl.AddRectFilled(cursorScreenPos, pMax, color, 6f, ImDrawFlags.RoundCornersLeft);
            dl.AddText(cursorScreenPos + new Vector2(3f, 4f), System.UInt32.MaxValue, name);

            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(cursorPos.X + labelSize.X);
            ImGui.PushItemWidth(80f);
            var wasChanged = ImGui.DragFloat($"###{name}", ref val);
            ImGui.PopStyleVar(1);

            return wasChanged;
        }

        public static bool Vector2(ref Vector2 vec, string name)
        {
            var wasChanged = false;
            ImGui.PushID(name);

            wasChanged |= FloatLabel(ref vec.X, "X", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref vec.Y, "Y", Y_AXIS_COLOR);

            ImGui.PopID();

            return wasChanged;
        }

        public static bool Vector3(ref Vector3 vec, string name)
        {
            var wasChanged = false;
            ImGui.PushID(name);

            wasChanged |= FloatLabel(ref vec.X, "X", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref vec.Y, "Y", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref vec.Z, "Z", Z_AXIS_COLOR);

            ImGui.PopID();

            return wasChanged;
        }

        public static bool Vector4(ref Vector4 vec, string name)
        {
            var wasChanged = false;
            ImGuiNET.ImGui.PushID(name);

            wasChanged |= FloatLabel(ref vec.X, "X", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref vec.Y, "Y", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref vec.Z, "Z", Z_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref vec.W, "W", W_AXIS_COLOR);

            ImGui.PopID();

            return wasChanged;
        }

        public static bool Quaternion(ref Quaternion quat, string name)
        {
            var wasChanged = false;
            ImGuiNET.ImGui.PushID(name);

            wasChanged |= FloatLabel(ref quat.X, "X", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref quat.Y, "Y", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref quat.Z, "Z", Z_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref quat.W, "W", W_AXIS_COLOR);

            ImGui.PopID();

            return wasChanged;
        }

        public static bool Matrix3x2(ref Matrix3x2 mat, string name)
        {
            var wasChanged = false;
            ImGuiNET.ImGui.PushID(name);

            wasChanged |= FloatLabel(ref mat.M11, "M11", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M12, "M12", Y_AXIS_COLOR);

            wasChanged |= FloatLabel(ref mat.M21, "M21", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M22, "M22", Y_AXIS_COLOR);

            wasChanged |= FloatLabel(ref mat.M31, "M31", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M32, "M32", Y_AXIS_COLOR);

            ImGui.PopID();

            return wasChanged;
        }

        public static bool Matrix4x4(ref Matrix4x4 mat, string name)
        {
            var wasChanged = false;
            ImGuiNET.ImGui.PushID(name);

            wasChanged |= FloatLabel(ref mat.M11, "M11", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M12, "M12", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M13, "M13", Z_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M14, "M14", W_AXIS_COLOR);

            wasChanged |= FloatLabel(ref mat.M21, "M21", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M22, "M22", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M23, "M23", Z_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M24, "M24", W_AXIS_COLOR);

            wasChanged |= FloatLabel(ref mat.M31, "M31", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M32, "M32", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M33, "M33", Z_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M34, "M34", W_AXIS_COLOR);

            wasChanged |= FloatLabel(ref mat.M41, "M41", X_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M42, "M42", Y_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M43, "M43", Z_AXIS_COLOR);
            ImGui.SameLine();
            wasChanged |= FloatLabel(ref mat.M44, "M44", W_AXIS_COLOR);


            ImGui.PopID();

            return wasChanged;
        }
    }
}