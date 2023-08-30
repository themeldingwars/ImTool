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
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace ImTool.Scene3D
{
    public class Actor : IComparable<Actor>
    {
        public World World { get; private set; }
        public string Name = "";
        public ActorFlags Flags;
        public Transform Transform = new();
        public BoundingBox BoundingBox;
        public DebugShapesComp.Shape BoundsDebugHandle;
        public float RenderOrderBoost = 0;
        public uint ID;

        public List<Component> Components = new();

        public Actor() { }

        public virtual void Init(World world)
        {
            World              = world;
            Name               = GetType().Name;
            BoundingBox        = new BoundingBox(-Vector3.One / 2, Vector3.One / 2);
            Transform.OnChange = () => OnTransformChanged(true);

            foreach (var component in Components)
            {
                component.Init(this);
            }

            OnTransformChanged(true);
        }

        public T AddComponet<T>() where T : Component, new()
        {
            var comp = new T();
            comp.Init(this);
            Components.Add(comp);
            UpdateBoundingBox();

            return comp;
        }

        public void RemoveComponent(Component component)
        {
            Components.Remove(component);
            UpdateBoundingBox();
        }

        public void ShowBounds(bool show)
        {
            if (show && BoundsDebugHandle == null)
            {
                SetBoundsShape();
            }
            else
            {
                BoundsDebugHandle.Remove();
                BoundsDebugHandle = null;
            }
        }

        protected virtual void SetBoundsShape()
        {
            var boundsShape = World.DebugShapes.AddCube(Transform.Position);
            boundsShape.FromBoundingBox(BoundingBox);
            BoundsDebugHandle = boundsShape;
        }

        public void UpdateBoundingBox()
        {
            var bBox = new BoundingBox();
            foreach (var component in Components)
            {
                var transformedBBox = BoundingBox.Transform(component.BoundingBox, component.Transform.World);
                bBox = BoundingBox.Combine(transformedBBox, bBox);
            }

            BoundingBox = BoundingBox.Transform(bBox, Transform.World);
        }

        public virtual void OnTransformChanged(bool updateComponets)
        {
            UpdateBoundingBox();
            if (updateComponets)
            {
                foreach (var component in Components)
                {
                    component.OnTransformChanged();
                }
            }

            World.OnTransformChanged(this);
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
