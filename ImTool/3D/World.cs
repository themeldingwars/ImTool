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
        protected double LastFrameTime;
        protected double LastDeltaTime;

        public CameraActor ActiveCamera;

        public List<Actor> UpdateableActors = new();

        public World(Window window)
        {
            MainWindow    = window;
            LastFrameTime = (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond);

            ActiveCamera = CreateActor<CameraActor>();
        }

        // Create a new actor in the world
        public T CreateActor<T>() where T : Actor, new()
        {
            var actor = new T();
            actor.Init(this);

            if ((actor.Flags & ActorFlags.CanNeverUpdate) != 0)
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

        public void Render(double dt, CommandList cmdList)
        {

        }
    }
}
