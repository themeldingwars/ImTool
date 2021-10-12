using System;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public abstract class WorkspaceTab : Tab
    {
        internal new ImGuiDockNodeFlags DockSpaceFlags { get; } =  ImGuiDockNodeFlags.PassthruCentralNode;
        
        public abstract string WorkspaceName { get; }
        protected virtual WorkspaceFlags Flags { get; } = WorkspaceFlags.None;
        protected virtual void SubmitWorkspaceContent() {}
        internal override void InternalSubmitContent()
        {
            if (Flags != WorkspaceFlags.None)
            {
                ImGuiDockNodeFlags overrideFlags = ImGuiDockNodeFlags.None;

                if (Flags.HasFlag(WorkspaceFlags.HideTabBar))
                    overrideFlags = ImGuiDockNodeFlags.NoWindowMenuButton | ImGuiDockNodeFlags.NoCloseButton | ImGuiDockNodeFlags.NoDockingOverMe;

                if (Flags.HasFlag(WorkspaceFlags.NoDocking))
                    overrideFlags |= ImGuiDockNodeFlags.NoDockingOverMe | ImGuiDockNodeFlags.NoDockingSplitMe | ImGuiDockNodeFlags.NoDockingOverOther | ImGuiDockNodeFlags.NoDockingSplitOther;
                
                if (Flags.HasFlag(WorkspaceFlags.NoDockingOver))
                    overrideFlags |= ImGuiDockNodeFlags.NoDockingOverMe;
                
                ImGuiWindowClass windowClass = new ImGuiWindowClass();
                windowClass.DockNodeFlagsOverrideSet = overrideFlags;
                unsafe
                {
                    ImGuiNative.igSetNextWindowClass(&windowClass);
                }
            }
            
            ImGui.Begin($"{WorkspaceName}###WS_{Name}_{WorkspaceName}", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBackground);
            SubmitWorkspaceContent();
            ImGui.End();
            
            SubmitContent();
        }

        internal override void InternalCreateDockSpace(Vector2 size)
        {
            ImGui.DockBuilderDockWindow($"{WorkspaceName}###WS_{Name}_{WorkspaceName}", DockSpaceID);
            CreateDockSpace(size);
        }

        [Flags]
        public enum WorkspaceFlags
        {
            None = 0,
            HideTabBar = 1,
            NoDocking = 2,
            NoDockingOver = 4,
            SingleWorkspace = HideTabBar | NoDocking
        }
    }
}