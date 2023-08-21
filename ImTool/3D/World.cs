using ImGuiNET;
using ImGuizmoNET;
using ImTool.Scene3D.Components;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Utilities;

namespace ImTool.Scene3D
{
    // Hold actors and componets to render a scene
    // not the fastest
    public class World
    {
        public Window MainWindow;
        private List<Scene3dWidget> Viewports = new();
        public List<Scene3dWidget> GetVieewports() => Viewports;
        public Framebuffer GetFBDesc() => Viewports.First().GetFramebuffer();

        protected double LastFrameTime;
        protected double LastDeltaTime;

        private DeviceBuffer ViewBuffer;       // active cameras view matrix
        private DeviceBuffer ProjectionBuffer; // active camera projection matrix
        public ResourceSet ProjViewSet;
        private DeviceBuffer ViewStateBuffer;

        public CameraActor ActiveCamera;
        public GridActor Grid;
        public DebugShapesActor DebugShapes;
        public MeshActor TestMesh;
        public MeshActor TestMesh2;
        public MeshActor TestMesh3;
        public MeshActor TestMesh4;

        public List<Actor> UpdateableActors = new();
        private List<Actor> RenderList      = new();

        private List<Actor> SelectedActors = new();

        private DebugShapesComp.Cube SelectionDisplayCube;

        public World(Window window)
        {
            MainWindow    = window;
            LastFrameTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);

            var factory = MainWindow.GetGraphicsDevice().ResourceFactory;

            ProjectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewBuffer       = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewStateBuffer  = factory.CreateBuffer(new BufferDescription(ViewData.SIZE, BufferUsage.UniformBuffer));
            ProjViewSet      = factory.CreateResourceSet(new ResourceSetDescription(Resources.ProjViewLayout, ViewStateBuffer));
        }

        // Call after a scene widget widget has been created
        public virtual void Init(Scene3dWidget sceneWidget)
        {
            Grid         = CreateActor<GridActor>();
            DebugShapes  = CreateActor<DebugShapesActor>();
            TestMesh     = CreateActor<MeshActor>();
            TestMesh2    = CreateActor<MeshActor>();
            TestMesh3    = CreateActor<MeshActor>();
            TestMesh4    = CreateActor<MeshActor>();

            //CreateActor<MeshActor>().Mesh.SetModel(SimpleModel.CreateFromCube());

            TestMesh.LoadFromObj("D:\\TestModels\\Test1\\test.obj");
            //TestMesh.ShowBounds(true);
            var loadTask1 = Task.Factory.StartNew(() =>
            {
                TestMesh2.LoadFromObj("D:\\TestModels\\neon\\neon.obj");
                TestMesh2.Transform.Position = new Vector3(3, 0, 0);
                //TestMesh2.ShowBounds(true);

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

            //TestMesh.Mesh.SetModel(SimpleModel.CreateFromCube());

            var rand = new Random();
            SelectionDisplayCube = DebugShapes.AddCube(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            //DebugShapes.AddCube(new Vector3(0, 0, 0), new Vector3(2, 2, 2));
            //DebugShapes.AddCube(new Vector3(1, 1, 1), new Vector3(10, 10, 20), new Vector4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1f), 5);
        }

        public void RegisterViewport(Scene3dWidget viewport)
        {
            Viewports.Add(viewport);

            var cam = CreateActor<CameraActor>();
            viewport.SetCamera(cam);
        }

        public void UnregisterViewport(Scene3dWidget viewport)
        {
            Viewports.Remove(viewport);
            viewport.SetCamera(null);
        }

        // Create a new actor in the world
        public T CreateActor<T>() where T : Actor, new()
        {
            var actor = new T();
            actor.Init(this);

            if ((actor.Flags & ActorFlags.CanNeverUpdate) == 0)
                UpdateableActors.Add(actor);

            return actor;
        }

        // Remove and destroy an actor from the world
        public void DestroyActor(Actor actor)
        {
            UpdateableActors.Remove(actor);
        }

        public void Tick()
        {
            LastDeltaTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) - LastFrameTime;
            LastFrameTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);

            Update(LastDeltaTime);
        }

        public void Update(double dt)
        {
            foreach (var actor in UpdateableActors)
            {
                if ((actor.Flags & ActorFlags.DontUpdate) == 0)
                    actor.Update(dt);
            }
        }

        public void Render(double dt, CommandList cmdList, CameraActor camera)
        {
            ActiveCamera = camera;
            BuildRenderList(camera);

            cmdList.UpdateBuffer(ViewStateBuffer, 0, camera.ViewData);
            cmdList.ClearDepthStencil(1f);

            foreach (var actor in RenderList)
            {
                actor.Render(cmdList);
            }
        }

        public unsafe void DrawTransform(OPERATION op, MODE mode, ref float[] snap)
        {
            var view  = ActiveCamera.ViewMat.ToFloatArrray();
            var proj  = ActiveCamera.ProjectionMat.ToFloatArrray();
            proj[5]   = -proj[5];
            proj[6]   = -proj[6];

            if (SelectedActors.Count > 0)
            {
                var transform = SelectedActors[0].Transform.World.ToFloatArrray();
                var deltaMat  = new float[16];
                float* snaps = null;
                if (ImGuizmo.Manipulate(ref view[0], ref proj[0], op, mode, ref transform[0], ref deltaMat[0], ref snap != null ? ref snap[0] : ref snaps[0]))
                {
                    if (ImGui.IsWindowFocused())
                    {
                        SelectedActors[0].Transform.World.FromFloatArray(transform);
                        SelectedActors[0].Transform.OnChange();
                    }
                }

                var size = (SelectedActors[0].BoundingBox.GetDimensions() / 2);
                SelectionDisplayCube.Transform = SelectedActors[0].Transform;
                //TestDebugCube.Transform.Position = new Vector3(TestDebugCube.Transform.Position.X, TestDebugCube.Transform.Position.Y + size.Y, TestDebugCube.Transform.Position.Z);
                //TestDebugCube.Extents = size;

                SelectedActors[0].UpdateBoundingBox();
                //var bBox = BoundingBox.Transform(SelectedActors[0].BoundingBox, SelectedActors[0].Transform.World);
                SelectionDisplayCube.FromBoundingBox(SelectedActors[0].BoundingBox);
                DebugShapes.Recreate();
            }
        }

        private void BuildRenderList(CameraActor camera)
        {
            RenderList.Clear();

            foreach (var actor in UpdateableActors)
            {
                if ((actor.Flags & ActorFlags.DontRender) == 0)
                {
                    RenderList.Add(actor);
                }
            }

            RenderList.Sort();
        }

        public void DrawHierarchyExplorer()
        {
            foreach (var actor in UpdateableActors)
            {
                DrawActorLabelForHierarchy(actor);
            }
        }

        public void DrawActorInspector()
        {
            if (SelectedActors.Count > 0)
            {
                var actor = SelectedActors[0];
                actor.DrawInspector();
            }
        }

        private void DrawActorLabelForHierarchy(Actor actor)
        {
            var name       = actor.Name ?? actor.GetType().Name;
            var isSelected = ImGui.Selectable($"{name}###{actor.GetHashCode()}", SelectedActors.Contains(actor));
            //ImGui.SameLine();
            //ImGui.Text(actor.Name ?? actor.GetType().Name);

            if (isSelected)
            {
                SelectedActors.Clear();
                SelectedActors.Add(actor);
            }
        }
    }
}
