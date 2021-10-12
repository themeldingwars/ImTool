using System;
using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace ImTool
{
    public abstract class Tab
    {
        public abstract string Name { get; }
        
        public uint DockSpaceID => ImGui.GetID($"{Name}TabDockspace");
        public virtual ImGuiDockNodeFlags DockSpaceFlags { get; } = ImGuiDockNodeFlags.None;
        private bool resetDockSpace = false;
        
        public abstract void SubmitContent();
        public virtual void SubmitSettings(bool active) { }
        public virtual void SubmitMainMenu() { }
        public void ResetDockSpace() => resetDockSpace = true;
        protected virtual void CreateDockSpace(Vector2 size) { }
        
        internal unsafe void SubmitDockSpace(Vector2 pos, Vector2 size)
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
                
                CreateDockSpace(size);
            
                ImGui.DockBuilderFinish(DockSpaceID);
            }
        }

        public bool IsMainMenuOverridden 
        {
            get
            {
                MethodInfo m = GetType().GetMethod("SubmitMainMenu");
                return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
            }
        }
    }
}