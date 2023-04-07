using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Vortice.Direct3D;

namespace ImTool.Scene3D
{
    public class Actor
    {
        public World World { get; private set; }
        public string Name;
        public ActorFlags Flags;
        public Transform Transform = new();

        public List<Component> Components = new();

        public Actor() { }

        public virtual void Init(World world)
        {
            World = world;

            foreach (var component in Components)
            {
                component.Init(this);
            }
        }

        public T AddComponet<T>() where T : Component, new()
        {
            var comp = new T();
            comp.Init(this);
            Components.Add(comp);

            return comp;
        }

        public void RemoveComponent(Component component)
        {
            Components.Remove(component);
        }

        // Update logic
        public virtual void Update(double dt)
        {
            foreach (var component in Components)
            {
                if ((component.Flags & ActorFlags.DontUpdate) == 0)
                    component.Update(dt);
            }
        }

        // Render into the command list
        public virtual void Render(CommandList cmdList)
        {
            foreach (var component in Components)
            {
                if ((component.Flags & ActorFlags.DontRender) == 0)
                    component.Render(cmdList);
            }
        }
    }
}
