using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace ImTool
{
    public abstract class Tab
    {
        public abstract string Name { get; }
        
        public uint DockSpaceID => ImGui.GetID($"{Name}TabDockspace");
        
        public ImGuiDockNodeFlags DockSpaceFlags = ImGuiDockNodeFlags.None;
        private bool resetDockSpace = false;
        

        public abstract void SubmitContent();
        public virtual void SubmitSettings(bool active) { }
        public virtual void SubmitMainMenu() { }
        protected virtual void CreateDockSpace(Vector2 size) { }
        public void ResetDockSpace() => resetDockSpace = true;
        internal unsafe void SubmitDockSpace(Vector2 pos, Vector2 size)
        {
            if (resetDockSpace || ImGui.DockBuilderGetNode(DockSpaceID).NativePtr == null)
            {
                resetDockSpace = false;
                if (ImGui.DockBuilderGetNode(DockSpaceID).NativePtr != null)
                    ImGui.DockBuilderRemoveNode(DockSpaceID);
            
                ImGui.SetCursorPos(pos);
                ImGui.DockBuilderAddNode(DockSpaceID);
                ImGui.DockBuilderSetNodeSize(DockSpaceID, size);
            
                CreateDockSpace(size);
            
                ImGui.DockBuilderFinish(DockSpaceID);
            }
            
            ImGui.SetCursorPos(pos);
            ImGui.DockSpace(DockSpaceID, size, DockSpaceFlags);
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