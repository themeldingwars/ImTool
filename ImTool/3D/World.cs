using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace ImTool.Scene3D
{
    // Hold actors and componets to render a scene
    // not the fastest
    public class World
    {
        public Window MainWindow;
        public Scene3dWidget CurrentSceneViewport;
        private List<Scene3dWidget> Viewports = new();

        protected double LastFrameTime;
        protected double LastDeltaTime;

        private DeviceBuffer ViewBuffer;       // active cameras view matrix
        private DeviceBuffer ProjectionBuffer; // active camera projection matrix
        public ResourceSet ProjViewSet;
        private DeviceBuffer ViewStateBuffer;
        public ResourceLayout ProjViewLayout { get; private set; }

        public CameraActor ActiveCamera;
        public GridActor Grid;

        public List<Actor> UpdateableActors = new();

        public World(Window window)
        {
            MainWindow    = window;
            LastFrameTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);

            var factory = MainWindow.GetGraphicsDevice().ResourceFactory;
            ProjViewLayout = factory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("ViewStateBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                    )
                );

            ProjectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewBuffer       = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));
            ViewStateBuffer  = factory.CreateBuffer(new BufferDescription(ViewData.SIZE, BufferUsage.UniformBuffer));
            ProjViewSet      = factory.CreateResourceSet(new ResourceSetDescription(ProjViewLayout, ViewStateBuffer));
        }

        // Call after a scene widget widget has been created
        public virtual void Init(Scene3dWidget sceneWidget)
        {
            CurrentSceneViewport = sceneWidget;

            //ActiveCamera = CreateActor<CameraActor>();
            Grid         = CreateActor<GridActor>();
        }

        public void RegisterViewport(Scene3dWidget viewport)
        {
            Viewports.Add(viewport);

            var cam = CreateActor<CameraActor>();
            viewport.SetCamera(cam);
            //CurrentSceneViewport = viewport;
        }

        public void UnregisterViewport(Scene3dWidget viewport)
        {
            Viewports.Remove(viewport);
            viewport.SetCamera(null);

            if (CurrentSceneViewport == viewport)
            {
                //CurrentSceneViewport = Viewports.FirstOrDefault(x => x != null);
            }
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
            cmdList.UpdateBuffer(ViewStateBuffer, 0, camera.ViewData);
            cmdList.ClearDepthStencil(1f);

            Grid.Render(cmdList);
        }
    }
}
