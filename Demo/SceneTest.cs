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

        public MeshActor TestMesh;
        public MeshActor TestMesh2;
        public MeshActor TestMesh3;
        public MeshActor TestMesh4;

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
            ImGui.DockBuilderDockWindow("Scene Hierarchy", leftId);
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

            TestMesh  = World.CreateActor<MeshActor>();
            TestMesh2 = World.CreateActor<MeshActor>();
            TestMesh3 = World.CreateActor<MeshActor>();
            TestMesh4 = World.CreateActor<MeshActor>();

            //CreateActor<MeshActor>().Mesh.SetModel(SimpleModel.CreateFromCube());

            TestMesh.LoadFromObj("D:\\TestModels\\Test1\\test.obj");
            //TestMesh.ShowBounds(true);
            var loadTask1 = Task.Factory.StartNew(async () =>
            {
                TestMesh2.LoadFromObj("D:\\TestModels\\neon\\neon.obj");
                TestMesh2.Transform.Position = new Vector3(3, 0, 0);
                //TestMesh2.ShowBounds(true);

                for (int x = 0; x < 100; x++)
                {
                    for (int y = 0; y < 100; y++)
                    {
                        var meshActor = World.CreateActor<MeshActor>();
                        meshActor.Transform.Position = new Vector3(x + 10, 0, y + 10);
                        meshActor.Mesh.SetModel(TestMesh2.Mesh.Model);

                        //await Task.Delay(TimeSpan.FromSeconds(0.01));
                    }
                }

                GC.Collect();
            });

            //TestMesh3.LoadFromObj("D:\\TestModels\\kindred\\kindred.obj");
            //TestMesh3.Transform.Position = new Vector3(5, 0, 0);

            var loadTask2 = Task.Factory.StartNew(() =>
            {
                TestMesh3.LoadFromObj("D:\\TestModels\\Evelynn\\Evelynn.obj");
                TestMesh3.Transform.Position = new Vector3(8, 0, 0);
                TestMesh3.Transform.Scale = new Vector3(0.01f, 0.01f, 0.01f);
                //TestMesh3.ShowBounds(true);

                GC.Collect();
            });
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

            if (ImGui.Begin("Scene Hierarchy"))
            {
                World.DrawHierarchyExplorer();
            }

            ImGui.End();

            if (ImGui.Begin("Scene Inspector"))
            {
                World.DrawActorInspector();

                /*
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
                }*/
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
                if (ImGui.Button("Show Active Cam Bounds"))
                {
                    World.ActiveCamera.ShowBounds(true);
                }

                if (ImGui.Button("Hide Active Cam Bounds"))
                {
                    World.ActiveCamera.ShowBounds(false);
                }
            }
            ImGui.End();
        }
    }
}
