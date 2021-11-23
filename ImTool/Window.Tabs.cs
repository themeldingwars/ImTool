using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public partial class Window
    {
        private List<Tab> tabs = new();
        private Tab activeTab;

        public void AddTab(Tab tab)
        {
            if (!tabs.Contains(tab))
            {
                tab.Load();
                tabs.Add(tab);
            }
        }

        public void RemoveTab(Tab tab)
        {
            if (tabs.Contains(tab))
            {
                tabs.Remove(tab);
                tab.Unload();
            }
        }

        private void SubmitTabs()
        {
            foreach (Tab tab in tabs)
            {
                TabStyleOverrides(true);
                if (!ImGui.BeginTabItem(tab.Name))
                {
                    ImGui.DockSpace(tab.DockSpaceID, new Vector2(0,0), ImGuiDockNodeFlags.KeepAliveOnly);
                    TabStyleOverrides(false);
                    continue;
                }
                
                if (activeTab != tab)
                {
                    activeTab = tab;
                }

                bool hasMainMenuBar = false;
                if (tab.IsMainMenuOverridden)
                {
                    hasMainMenuBar = true;
                    BeginMainMenuBar();
                    tab.InternalSubmitMainMenu();
                    EndMainMenuBar();
                }
                else if (OnSubmitGlobalMenuBarOverride != null)
                {
                    hasMainMenuBar = true;
                    BeginMainMenuBar();
                    OnSubmitGlobalMenuBarOverride();
                    EndMainMenuBar();
                }
                
                TabStyleOverrides(false);
                
                Vector2 dockPos = ImGui.GetCursorPos() + (hasMainMenuBar ? new Vector2(1, 17) : new Vector2(1, -3));
                Vector2 dockSize = hasMainMenuBar ? contentBounds.Size - new Vector2(0, 20): contentBounds.Size;
                
                tab.InternalSubmitDockSpace(dockPos, dockSize);
                tab.InternalSubmitContent();
                ImGui.EndTabItem();
                
            }
            
            TabStyleOverrides(false);
        }
    }
}