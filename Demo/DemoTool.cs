using System;
using System.Numerics;
using ImGuiNET;
using ImTool;
using ImTool.SDL;

namespace Demo
{
    public class DemoConfig : Configuration
    {

        // add new fields to the config, these will be serialized alongside the default fields
        public int AdditionalIntToBeSaved = 123;
        
        // override the default window settings
        public DemoConfig()
        {
            Title                 = "ImTool Demo";
            GithubRepositoryOwner = "themeldingwars";
            GithubRepositoryName  = "ImTool";
            GithubReleaseName     = "Demo";
        }
    }
    
    public class DemoTool : Tool<DemoTool, DemoConfig>
    {
        protected override bool Initialize(string[] args)
        {
            // override this method if you need to parse cmd-line args or do checks before the tool window is created
            // only the config has been loaded at this point
            // returning false will exit the tool
            
            // example use cases: 
            //      parsing cmd-line messages
            //      mutex check to ensure only a single instance of the tool is running
            //      routing cmd-line messages to a single instance tool
            //
            return true;
        }

        protected override void Load()
        {
            ExtraWidgetsTests.SetupHexView();
            ThemeManager.OnThemeChanged += () => ExtraWidgetsTests.HexViewWidget.SetupSizes();

            // tool window has been created at this point
            // its time to load your tabs now
            window.AddTab(new DemoTab());
            window.AddTab(new DemoTab2());
            config.AdditionalIntToBeSaved = 1234123;
            
            window.AddWindowButton("Test button", () =>
            {
                Console.WriteLine("Test window button clicked :>");
            });
        }

        protected override void Unload()
        {
            // time to clean up your shit
            // you can still do edits to the config,
            // changes will be saved to disk when this method returns
            
        }
        

    }


    public class DemoTab : Tab
    {
        public override string Name { get; } = "ImTool Demo Tab";

        protected override void CreateDockSpace(Vector2 size)
        {
            // split
            uint topId;
            uint bottomId;
            uint topLeftId;
            uint topRightId;
            uint topRightTopId;
            uint topRightBottomId;
            
            ImGui.DockBuilderSplitNode(DockSpaceID, ImGuiDir.Up, 0.80f, out topId, out bottomId);
            ImGui.DockBuilderSplitNode(topId, ImGuiDir.Left, 0.60f, out topLeftId, out topRightId);
            ImGui.DockBuilderSplitNode(topRightId, ImGuiDir.Up, 0.70f, out topRightTopId, out topRightBottomId);
            
            // assign
            ImGui.DockBuilderDockWindow("Hex View", topLeftId);
            ImGui.DockBuilderDockWindow("Extensions test :>", topRightTopId);
            ImGui.DockBuilderDockWindow("Dear ImGui Demo", topRightTopId);
            ImGui.DockBuilderDockWindow("Dear ImGui Metrics/Debugger", topRightBottomId);
            ImGui.DockBuilderDockWindow("Test Log Window", bottomId);
            
        }
        
        public override void SubmitContent()
        {
            ImGui.ShowDemoWindow();
            ImGui.ShowMetricsWindow();
            ExtraWidgetsTests.Draw();
        }
        public override void SubmitSettings(bool active)
        {
            ImGui.Text($"Submitted from DemoTab.SubmitSettings({active})");
        }
        public override void SubmitMainMenu()
        {
            if (ImGui.BeginMenu("File"))
            {
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("View"))
            {
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Navigate"))
            {
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Help"))
            {
                ImGui.EndMenu();
            }
        }
        
    }
    
    public class DemoTab2 : Tab
    {
        public override string Name { get; } = "ImTool Demo Tab 2";
        public override void SubmitContent()
        {
            ImGui.Begin("Winduu");
            ImGui.ShowStyleEditor();
            ImGui.End();
            
        }

        public override void SubmitSettings(bool active)
        {
            ImGui.Text($"Submitted from DemoTab2.SubmitSettings({active})");
        }


        public override void SubmitMainMenu()
        {
            if (ImGui.BeginMenu("Fileuu"))
            {
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Edituu"))
            {
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Viewuu"))
            {
                ImGui.EndMenu();
            }
            
            if (ImGui.BeginMenu("Navigateuu"))
            {
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Helpuu"))
            {
                ImGui.EndMenu();
            }
        }
        
    }
}