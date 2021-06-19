using System;
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
            Title = "Demo";
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
            config.AdditionalIntToBeSaved = 1234123;
            
            window.AddWindowButton(new WindowButton("Test button", () =>
            {
                Console.WriteLine("Test window button clicked :>");
            }));
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
    }
}