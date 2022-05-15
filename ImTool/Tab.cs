using System;
using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace ImTool
{
    public abstract class Tab
    {
        public abstract string Name { get; }

        public string DisplayName { get; set; } = null;
        
        public uint DockSpaceID => ImGui.GetID($"{DisplayName ?? Name}###{Name}TabDockspace");
        public virtual ImGuiDockNodeFlags DockSpaceFlags { get; } = ImGuiDockNodeFlags.None;
        private bool resetDockSpace = false;
        
        public void ResetDockSpace() => resetDockSpace = true;
        
        public virtual void Load() { }
        public virtual void Unload() { }
        
        protected abstract void SubmitContent();
        protected virtual void SubmitSettings(bool active) { }
        protected virtual void SubmitMainMenu() { }
        protected virtual void CreateDockSpace(Vector2 size) { }


        internal virtual void InternalSubmitContent() => SubmitContent();
        internal virtual void InternalSubmitSettings(bool active) => SubmitSettings(active);
        internal virtual void InternalSubmitMainMenu() => SubmitMainMenu();
        internal virtual void InternalCreateDockSpace(Vector2 size) => CreateDockSpace(size);
        internal unsafe void InternalSubmitDockSpace(Vector2 pos, Vector2 size)
        {
            bool first = ImGui.DockBuilderGetNode(DockSpaceID).NativePtr == null;
            
            if (!first)
            {
                ImGui.SetCursorPos(pos);
                ImGui.DockSpace(DockSpaceID, size, DockSpaceFlags);
            }
            
            if (resetDockSpace || first)
            {
                resetDockSpace = false;
                if (ImGui.DockBuilderGetNode(DockSpaceID).NativePtr != null)
                    ImGui.DockBuilderRemoveNode(DockSpaceID);
            
                ImGui.SetCursorPos(pos);
                ImGui.DockBuilderAddNode(DockSpaceID, DockSpaceFlags | ImGuiDockNodeFlags.DockSpace);
                ImGui.DockBuilderSetNodeSize(DockSpaceID, size);
                
                InternalCreateDockSpace(size);
            
                ImGui.DockBuilderFinish(DockSpaceID);
            }
        }

        public bool IsMainMenuOverridden 
        {
            get
            {
                MethodInfo m = GetType().GetMethod("SubmitMainMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
            }
        }
    }
}