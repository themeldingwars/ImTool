using System;
using System.Numerics;
using ImGuiNET;
using ImTool;
using ImTool.Scene3D;

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

        public static  HexView                  HexViewWidget;
        private static HexView.HighlightSection SelectedHexHighlight;

        public static Scene3dWidget Scene3d;
        private static Transform Transform = new();

        public enum TestLogCategories
        {
            Test,
            Test2,
            Examples,
            Nyaa
        }

        public static LogWindow<TestLogCategories> TestLogWindow = new LogWindow<TestLogCategories>("Test Log Window");

        public static void Draw()
        {
            //ImGui.SetNextWindowSize(new Vector2(400, 500));
            if (ImGui.Begin("Extensions test :>")) {

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
                
                if (Widgets.IconButton("", "Icon button test")) {
                    TestLogWindow.AddLogInfo( TestLogCategories.Test,"Icon button clicked :D");
                }
                
                ImGui.SameLine();
                
                if (Widgets.IconButton("", "Icon 2", new Vector4(1, 0, 0, 1))) {
                    TestLogWindow.AddLogInfo( TestLogCategories.Test,"Icon 2 button clicked :D");
                }
                
                ImGui.SameLine();
                
                if (Widgets.IconButton("")) {
                    TestLogWindow.AddLogInfo( TestLogCategories.Test,"Icon 3 button clicked :D");
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Open Text File")) {
                    FileBrowser.OpenFile((fielPath) => { FileDialogResult = fielPath; }, searchPattern: "*.txt|*.log");
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
            }
            ImGui.End();

            if (ImGui.Begin("Other widgets"))
            {
                Transform.DrawImguiWidget(true, true);
            }

            DrawHexView();
            
            TestLogWindow.DrawWindow();

            Scene3d.DrawWindow("3D Test");
        }

        public static void SetupHexView()
        {
            HexViewWidget = new HexView();
            var bytesData = new byte[512];
            new Random().NextBytes(bytesData);
            HexViewWidget.SetData(bytesData, new HexView.HighlightSection[]
            {
                new HexView.HighlightSection
                {
                    Color     = ImToolColors.RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x542265FF)),
                    HoverName = "Test hover section 1",
                    Length    = 4,
                    Offset    = 0
                },
                
                new HexView.HighlightSection
                {
                    Color     = ImToolColors.RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x416c1fFF)),
                    HoverName = "Test hover section 2",
                    Length    = 20,
                    Offset    = 4
                },
                
                new HexView.HighlightSection
                {
                    Color     = ImToolColors.RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x553accFF)),
                    HoverName = "Test hover section 3",
                    Length    = 8,
                    Offset    = 24
                },
                
                new HexView.HighlightSection
                {
                    Color     = ImToolColors.RGBAToBGR(ImGui.ColorConvertU32ToFloat4(0x746015FF)),
                    HoverName = "Test hover section 4",
                    Length    = 100,
                    Offset    = 32
                }
            });

            HexViewWidget.OnHighlightSectionHover += section => SelectedHexHighlight = section;
            
            TestLogWindow.AddLogInfo(TestLogCategories.Test,"Test message 1 :>");
            TestLogWindow.AddLogTrace(TestLogCategories.Nyaa,"Test message 2 :>");
            TestLogWindow.AddLogWarn(TestLogCategories.Examples,"Test message 3 :>");
            TestLogWindow.AddLogError(TestLogCategories.Test,"Test message 4 :>");
            
            TestLogWindow.AddLogInfo(TestLogCategories.Test,@"Multiple line log test
Line 2
Line 3
Line 4");

            for (int i = 0; i < 500; i++) {
                TestLogWindow.AddLogInfo(TestLogCategories.Test,$"Test message {i + 5} :>");
            }
        }
        
        public static void DrawHexView()
        {
            if (ImGui.Begin("Hex View")) {
                if (ImGui.BeginTable("Hex controls", 2)) {
                    ImGui.TableNextColumn();
                    ImGui.Checkbox("Side Parsed Values", ref HexViewWidget.ShowSideParsedValues);
                    
                    ImGui.TableNextColumn();
                    ImGui.Checkbox("Side Parsed Values in tool tip", ref HexViewWidget.ShowParsedValuesInTT);
                    
                    ImGui.EndTable();
                }
                
                HexViewWidget.Draw();

                for (int i = 0; i < HexViewWidget.HighlightsArr.Length; i++) {
                    if (ImGui.Button($"Select Highlight {i}")) {
                        HexViewWidget.SetHighlightAsSelected(i);
                    }

                    if ((i % 3) != 1) {
                        ImGui.SameLine();
                    }
                }
                
                if (ImGui.Button($"Clear selected highlights")) {
                    HexViewWidget.ClearSelectedHighlights();
                }
                
                ImGui.Text($"Hovered highlight: {SelectedHexHighlight.HoverName}");
                
            }
            ImGui.End();
        }
    }
}