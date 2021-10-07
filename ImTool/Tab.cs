using System.Reflection;
using ImGuiNET;

namespace ImTool
{
    public abstract class Tab
    {

        public abstract string Name { get; } 
        public uint DockspaceID => ImGui.GetID($"{Name}TabDockspace");
        public ImGuiDockNodeFlags DockspaceFlags = ImGuiDockNodeFlags.None;
        
        public abstract void SubmitContent();
        public virtual void SubmitSettings(bool active) { }
        public virtual void SubmitMainMenu() { }
        
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