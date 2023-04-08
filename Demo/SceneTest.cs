using ImGuiNET;
using ImTool;
using ImTool.Scene3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ImTool.WorkspaceTab;

namespace Demo
{
    public class SceneTestTab : WorkspaceTab
    {
        private DemoTool tool;
        public SceneTestTab(DemoTool tool) => this.tool = tool;

        public override string WorkspaceName { get; } = "SceneTest";
        protected override WorkspaceFlags Flags { get; } = WorkspaceFlags.None;
        public override string Name { get; } = "Scene demo";

        public World World;
        public Scene3dWidget MainSceneView;
        public Scene3dWidget MainSceneView2;

        // if you want a custom default docking layout, this is the place to do that
        protected override void CreateDockSpace(Vector2 size)
        {
            // split
            ImGui.DockBuilderSplitNode(DockSpaceID, ImGuiDir.Left, 0.30f, out uint leftId, out uint centerId);
            ImGui.DockBuilderSplitNode(centerId, ImGuiDir.Down, 0.20f, out uint centerBottomId, out uint centerTopId);
            ImGui.DockBuilderSplitNode(centerTopId, ImGuiDir.Right, 0.40f, out uint centerRightId, out uint centerLeftId);
            ImGui.DockBuilderSplitNode(centerRightId, ImGuiDir.Down, 0.40f, out uint centerRightBottomId, out uint centerRightTopId);

            // assign
            //ImGui.DockBuilderDockWindow("Hex View", topLeftId);
            ImGui.DockBuilderDockWindow("Main View", centerLeftId);
            ImGui.DockBuilderDockWindow("Test Log Window", centerBottomId);
            ImGui.DockBuilderDockWindow("Scene Hierachy", leftId);
            ImGui.DockBuilderDockWindow("Scene Inspector", centerRightTopId);
            ImGui.DockBuilderDockWindow("Tests", centerRightBottomId);

        }
        public override void Load()
        {
            World = new(tool.Window);

            MainSceneView  = new Scene3dWidget(tool.Window, World);
            MainSceneView2 = new Scene3dWidget(tool.Window, World);

            World.RegisterViewport(MainSceneView);
            World.RegisterViewport(MainSceneView2);
            World.Init(MainSceneView);
        }

        public override void Unload()
        {
        }

        ImTool.Scene3D.Components.DebugShapesComp.Cube TestDebugCube = null;
        protected override void SubmitContent()
        {
            World.Tick();

            MainSceneView2.DrawWindow("Main View 2");

            DrawTests();

            if (ImGui.Begin("Scene Hierachy"))
            {

            }
            ImGui.End();

            if (ImGui.Begin("Scene Inspector"))
            {
                if (World.ActiveCamera != null)
                {
                    //World.ActiveCamera.Transform.DrawImguiWidget();
                }

                if (MainSceneView.GetCamera() != null)
                {
                    ImGui.PushID("Scene1Camera");
                    MainSceneView.GetCamera().Transform.DrawImguiWidget();
                    ImGui.PopID();
                    ImGui.Separator();
                }

                if (MainSceneView2.GetCamera() != null)
                {
                    ImGui.PushID("Scene2Camera");
                    MainSceneView2.GetCamera().Transform.DrawImguiWidget();
                    ImGui.PopID();
                }

                if (World.DebugShapes != null)
                {
                    ImGui.PushID("Debug Shapes");
                    World.DebugShapes.Transform.DrawImguiWidget();
                    ImGui.PopID();
                }

                if (World.DebugShapes != null)
                {
                    ImGui.PushID("Debug Mesh");
                    World.TestMesh.Transform.DrawImguiWidget();
                    ImGui.PopID();
                }

                if (ImGui.Button("Add Cube"))
                {
                    TestDebugCube = World.DebugShapes.AddCube(new Vector3(10, 5, 20), new Vector3(4, 4, 4), new Vector4(0.2f, 0.5f, 0.1f, 1));
                }

                if (ImGui.Button("Remove Cube"))
                {
                    TestDebugCube.Remove();
                }
            }
            ImGui.End();
        }

        protected override void SubmitWorkspaceContent()
        {
            var size = ImGui.GetContentRegionAvail();
            size.X   = size.X <= 0 ? 1 : size.X;
            size.Y   = size.Y <= 0 ? 1 : size.Y;

            MainSceneView.Draw(size);
        }

        protected override void SubmitSettings(bool active)
        {
        }

        // submit your file menu etc from here :)
        protected override void SubmitMainMenu()
        {

        }

        public void DrawTests()
        {
            if (ImGui.Begin("Tests"))
            {

            }
            ImGui.End();
        }
    }
}
