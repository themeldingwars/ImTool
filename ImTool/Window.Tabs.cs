using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace ImTool
{
    public partial class Window
    {
        private List<Tab> tabs = new();
        private Tab activeTab;
        private object key = new();

        public void AddTab(Tab tab)
        {
            lock (key)
            {
                if (!tabs.Contains(tab))
                {
                    tab.Load();
                    tabs.Add(tab);
                }
            }
        }

        public void RemoveTab(Tab tab)
        {
            lock (key)
            {
                if (tabs.Contains(tab))
                {
                    tabs.Remove(tab);
                    tab.Unload();
                }
            }
        }

        private void SubmitTabs()
        {
            lock (key)
            {
                foreach (Tab tab in tabs)
                {
                    TabStyleOverrides(true);
                    if (!ImGui.BeginTabItem(tab.Name))
                    {
                        ImGui.DockSpace(tab.DockSpaceID, new Vector2(0, 0), ImGuiDockNodeFlags.KeepAliveOnly);
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

                    int mainMenuHeight = hasMainMenuBar ? (int) ImGui.GetItemRectSize().Y : 0;
                    Vector2 dockPos = new Vector2(borderThickness, borderThickness + titlebarHeight + mainMenuHeight);
                    Vector2 dockSize = contentBounds.Size - new Vector2(0, mainMenuHeight);

                    tab.InternalSubmitDockSpace(dockPos, dockSize);
                    tab.InternalSubmitContent();
                    ImGui.EndTabItem();

                }
            }

            TabStyleOverrides(false);
        }
    }
}