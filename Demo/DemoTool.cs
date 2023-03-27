using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using ImTool;
using ImTool.SDL;
using Veldrid.ImageSharp;

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
            DisableSettingsPane   = false;
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
            
            // You can also load custom fonts and add different sizes to the default fonts here
            // FontManager.AddFont(Font font)
            // FontManager.Fonts["Regular"].AddSize(50);
            //
            // ImTool fonts:
            // - ProggyClean:   Default ImGui font, not built to be scaled, 13px
            // - FAS:           FontAwesome Solid
            // - FAR:           FontAwesome Regular
            // - FAB:           FontAwesome Brands
            // - Regular:       SourceSansPro Regular
            // - Bold:          SourceSansPro Bold
            // - Italic:        SourceSansPro Italic
            // - BoldItalic:    SourceSansPro Bold Italic
            // - FreeSans:      FreeSans Regular (huge number of glyphs)
            //
            // To switch between fonts: 
            // FontManager.PushFont("FAB");
            // FontManager.PushFont("Regular", 18);
            // FontManager.PopFont();
            
            return true;
        }

        protected override void Load()
        {
            ExtraWidgetsTests.SetupHexView();
            ThemeManager.OnThemeChanged += () => ExtraWidgetsTests.HexViewWidget.SetupSizes();

            ExtraWidgetsTests.Scene3d = new Scene3dWidget(Window);

            // tool window has been created at this point
            // its time to load your tabs now
            Window.AddTab(new DemoWorkspaceTab(this));
            Window.AddTab(new DemoTab());
            Config.AdditionalIntToBeSaved = 1234123;
            
            Window.AddWindowButton("Test button", () =>
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
    
    public class DemoWorkspaceTab : WorkspaceTab
    {
        // if access to tool, config, updater or main window is needed
        // pass a ref to the tool in the constructor, the rest can be reached from there
        private DemoTool tool;
        public DemoWorkspaceTab(DemoTool tool) => this.tool = tool;

        public override string WorkspaceName { get; } = "Workspace";
        protected override WorkspaceFlags Flags { get; } = WorkspaceFlags.None;
        public override string Name { get; } = "Workspace demo";
        
        // if you want a custom default docking layout, this is the place to do that
        protected override void CreateDockSpace(Vector2 size)
        {
            // split
            ImGui.DockBuilderSplitNode(DockSpaceID, ImGuiDir.Left, 0.30f, out uint leftId, out uint centerId);
            ImGui.DockBuilderSplitNode(centerId, ImGuiDir.Down, 0.20f, out uint centerBottomId, out uint centerTopId);
            ImGui.DockBuilderSplitNode(centerTopId, ImGuiDir.Right, 0.40f, out uint centerRightId, out uint centerLeftId);
            ImGui.DockBuilderSplitNode(centerRightId, ImGuiDir.Down, 0.40f, out uint centerRightBottomId, out uint centerRightTopId);

            // assign
            //ImGui.DockBuilderDockWindow("Hex View", topLeftId);
            ImGui.DockBuilderDockWindow("Test Log Window", centerBottomId);
            ImGui.DockBuilderDockWindow("Dear ImGui Demo", leftId);
            ImGui.DockBuilderDockWindow("Dear ImGui Metrics/Debugger", centerRightTopId);
            ImGui.DockBuilderDockWindow("Extensions test :>", centerRightBottomId);
            
        }

        // load gets called when the tab gets added to the window
        // this is a good time to load resources, images etc needed by the tab

        private ImageSharpTexture testImage;
        public override void Load()
        {
            // image test
            Stream resFilestream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Demo.Images.Merch.png");
            testImage = new ImageSharpTexture(resFilestream);
        }

        // unload gets called when the tab is removed from the window
        // time to do some cleanup :>
        public override void Unload()
        {
            // image test
            tool.Window.DisposeTextureBinding(testImage);
            testImage = null;
        }

        // here you can submit your own windows
        // happens on every frame
        protected override void SubmitContent()
        {
            ImGui.ShowDemoWindow();
            ImGui.ShowMetricsWindow();
            ExtraWidgetsTests.Draw();
            
            // image test
            ImGui.Begin("Test Image");
            ImGui.Image(tool.Window.GetOrCreateTextureBinding(testImage), new Vector2(96, 96));
            ImGui.End();
            
            // fullscreen test
            ImGui.Begin("Test Fullscreen");
            if (ImGui.Button($"{(tool.Window.FullscreenMode ? "Disable fullscreen" : "Enable Fullscreen")}###FullScreenToggle"))
            {
                tool.Window.FullscreenMode = !tool.Window.FullscreenMode;
            }
            ImGui.End();
        }

        // the "workspace" is the central node / free space in a tab
        // you can change its behavior by overriding the Flags property on this tab
        // for instance, WorkspaceFlags.SingleWorkspace is great if you have a small tool with a single tab
        // since it will disable docking in the workspace and remove the inner tab bar
        // it might also be a good idea to set AllowFloatingWindows = false in the config constructor to disable viewports
        // if you're using this mode
        //
        // you can sumbit controls directly to the workspace from this override as the workspace is contained in a imgui window
        protected override void SubmitWorkspaceContent()
        {
            
        }

        // anything you submit here appears on the settings pane
        // the "active" bool tells you if this tab is currently active
        protected override void SubmitSettings(bool active)
        {
            ImGui.Text($"Submitted from WorkspaceTab.SubmitSettings({active})");
        }
        
        // submit your file menu etc from here :)
        protected override void SubmitMainMenu()
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
    
    public class DemoTab : Tab
    {
        public override string Name { get; } = "Second tab";
        
        public override void Load()
        {
            
        }
        public override void Unload()
        {
            
        }
        
        protected override void SubmitContent()
        {
            ImGui.Begin("Winduu");
            ImGui.ShowStyleEditor();
            ImGui.End();
            
        }

        protected override void SubmitSettings(bool active)
        {
            ImGui.Text($"Submitted from DemoTab.SubmitSettings({active})");
        }


        protected override void SubmitMainMenu()
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