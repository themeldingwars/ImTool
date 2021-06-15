using System.Numerics;
using ImGuiNET;
using ImTool;

namespace Demo
{
    public class ExtraWidgetsTests
    {
        public static float      TestFloat  = 0f;
        public static float      TestFloat2 = 0f;
        public static Vector3    Vec3Test   = new Vector3(1f, 1f, 1f);
        public static Vector2    Vec2Test   = new Vector2(1f, 1f);
        public static Vector4    Vec4Test   = new Vector4(1f, 1f, 1f, 1f);
        public static Quaternion QuatTest   = new Quaternion(1f, 1f, 1f, 1f);

        public static Matrix3x2 Matrix3x2Test = new Matrix3x2();
        public static Matrix4x4 Matrix4x4Test = new Matrix4x4();

        public static string FileDialogResult = "";

        public static bool ShowPopup = true;

        public static void Draw()
        {
            //ImGui.SetNextWindowSize(new Vector2(400, 500));
            ImGui.Begin("Arkiis extensions test :>");

            ImGui.Text("Floats");
            Widgets.FloatLabel(ref TestFloat, "Test");
            ImGui.SameLine();
            Widgets.FloatLabel(ref TestFloat2, "Test 2");

            ImGui.Text("Vector3");
            Widgets.Vector3(ref Vec3Test, nameof(Vec3Test));

            ImGui.Text("Floats");
            Widgets.Vector2(ref Vec2Test, nameof(Vec2Test));

            ImGui.Text("Vector4");
            Widgets.Vector4(ref Vec4Test, nameof(Vec4Test));

            ImGui.Text("Quaternion");
            Widgets.Quaternion(ref QuatTest, nameof(QuatTest));

            ImGui.Text("Matrix3x2");
            Widgets.Matrix3x2(ref Matrix3x2Test, nameof(Matrix3x2Test));

            ImGui.Text("Matrix4x4");
            Widgets.Matrix4x4(ref Matrix4x4Test, nameof(Matrix4x4Test));

            ImGui.Text("File Browser");

            if (ImGui.Button("Open File")) {
                FileBrowser.OpenFile((fielPath) => { FileDialogResult = fielPath; });
            }

            ImGui.SameLine();

            if (ImGui.Button("Open Files")) {
                FileBrowser.OpenFiles((fielPaths) => { FileDialogResult = string.Join(", ", fielPaths); });
            }

            ImGui.SameLine();

            if (ImGui.Button("Select Dir")) {
                FileBrowser.SelectDir((fielPath) => { FileDialogResult = fielPath; });
            }

            ImGui.SameLine();

            if (ImGui.Button("Save File")) {
                FileBrowser.SaveFile((fielPath) => { FileDialogResult = fielPath; });
            }

            ImGui.Text($"Dialog result: {FileDialogResult}");

            FileBrowser.Draw();

            ImGui.End();
        }
    }
}