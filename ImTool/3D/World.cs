using ImGuiNET;
using ImGuizmoNET;
using ImTool.Scene3D.Components;
using Octokit;
using System;
using System.Collections.Concurrent;
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

        private uint LastActorIdx                         = 0;
        public List<Actor> UpdateableActors               = new();
        public ConcurrentQueue<Actor> PendingAddActors    = new();
        public ConcurrentQueue<Actor> PendingRemoveActors = new();
        public Octree<Actor> Octree                       = new Octree<Actor>(new BoundingBox(Vector3.One * float.MinValue, Vector3.One * float.MaxValue), 50);
        public List<Actor> RenderList                     = new();
        private ActorSortComparer ActorSorterComper       = new ActorSortComparer();

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

            SelectionDisplayCube = DebugShapes.AddCube(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
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
            actor.ID = LastActorIdx++;

            PendingAddActors.Enqueue(actor);

            return actor;
        }

        // Remove and destroy an actor from the world
        public void DestroyActor(Actor actor)
        {
            PendingRemoveActors.Enqueue(actor);
        }

        public void SelectItem(SelectableID id)
        {
            if (id.Id != SelectableID.NO_ID_VALUE)
            {
                ClearSelected();
                var selectedActor = UpdateableActors.FirstOrDefault(x => x.ID == id.Id);
                if (selectedActor != default)
                {
                    SelectedActors.Add(selectedActor);
                    selectedActor.Flags |= ActorFlags.ShowOutline;
                    selectedActor.OnTransformChanged(true);
                }
            }
        }

        public void ClearSelected()
        {
            foreach (var actor in SelectedActors)
            {
                actor.Flags ^= ActorFlags.ShowOutline;
                actor.OnTransformChanged(true);
            }

            SelectedActors.Clear();
        }

        public void Tick()
        {
            LastDeltaTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond) - LastFrameTime;
            LastFrameTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);

            Update(LastDeltaTime);
        }

        public void Update(double dt)
        {
            AddPendingAddedActors();
            RemovePendingRemovedActors();

            foreach (var actor in UpdateableActors)
            {
                if ((actor.Flags & ActorFlags.DontUpdate) == 0)
                    actor.Update(dt);

                //Octree.MoveItem(actor, actor.BoundingBox);
            }

            Octree.ApplyPendingMoves();
        }

        public void OnTransformChanged(Actor actor)
        {
            if ((actor.Flags & ActorFlags.IsInOctree) != 0)
            {
                Octree.MoveItem(actor, actor.BoundingBox);
            }
        }

        private void RemovePendingRemovedActors()
        {
            while (PendingRemoveActors.TryDequeue(out var actor))
            {
                UpdateableActors.Remove(actor);
                Octree.RemoveItem(actor);
            }
        }

        private void AddPendingAddedActors()
        {
            while(PendingAddActors.TryDequeue(out var actor))
            {
                if ((actor.Flags & ActorFlags.CanNeverUpdate) == 0)
                    UpdateableActors.Add(actor);

                Octree.AddItem(actor.BoundingBox, actor);
                actor.Flags |= ActorFlags.IsInOctree;
            }
        }

        public RenderStats Render(double dt, CommandList cmdList, CameraActor camera)
        {
            var stats = new RenderStats();
            ActiveCamera = camera;
            BuildRenderList(camera);

            cmdList.UpdateBuffer(ViewStateBuffer, 0, camera.ViewData);
            cmdList.ClearDepthStencil(1f, 0);

            foreach (var actor in RenderList)
            {
                actor.Render(cmdList);
            }

            stats.NumActors         = UpdateableActors.Count;
            stats.NumRenderedActors = RenderList.Count;
            return stats;
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

                //SelectionDisplayCube.FromBoundingBox(SelectedActors[0].BoundingBox);
                //DebugShapes.Recreate();
            }
        }

        private void BuildRenderList(CameraActor camera)
        {
            RenderList.Clear();

            var fustrum = camera.Frustum;
            List<Actor> actorsInView = new();
            Octree.GetContainedObjects(fustrum, actorsInView);
            foreach (var actor in actorsInView)
            {
                // TODO: Distance based culling
                if ((actor.Flags & ActorFlags.DontRender) == 0)
                {
                    RenderList.Add(actor);
                }
            }

            ActorSorterComper.CameraPosition = camera.Transform.Position;
            RenderList.Sort(ActorSorterComper);
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
                SelectItem(new SelectableID(actor.ID, 0));
            }
        }
    }
}
