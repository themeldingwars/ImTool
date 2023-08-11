using ImGuiNET;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Vortice.Direct3D;

namespace ImTool.Scene3D
{
    public class Actor : IComparable<Actor>
    {
        public World World { get; private set; }
        public string Name = "";
        public ActorFlags Flags;
        public Transform Transform    = new();
        public float RenderOrderBoost = 0;

        public List<Component> Components = new();

        public Actor() { }

        public virtual void Init(World world)
        {
            World = world;
            Name = GetType().Name;

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

        public virtual void DrawInspector()
        {
            ImGui.Text("Name");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-150);
            ImGui.InputText("###Name", ref Name, 128);

            ImGui.SameLine();
            ImGui.Text("Flags");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo("###Flags", "..."))
            {
                int flags = (int)Flags;
                if (ImGui.CheckboxFlags("Don't Update", ref flags, (int)(ActorFlags.DontUpdate)))
                    Flags ^= ActorFlags.DontUpdate;

                if (ImGui.CheckboxFlags("Don't Render", ref flags, (int)(ActorFlags.DontRender)))
                    Flags ^= ActorFlags.DontRender;

                ImGui.EndCombo();
            }

            Transform.DrawImguiWidget();

            foreach (var component in Components)
            {
                if (ImGui.CollapsingHeader($"{component.Name} ({component.GetType().Name})"))
                {
                    component.DrawInspector();
                }
            }
        }

        public int CompareTo(Actor other)
        {
            return RenderOrderBoost < other.RenderOrderBoost ? 1 : -1;
        }
    }
}
