﻿
using ImTool.Scene3D;

namespace ImTool
{
    public class Scene3dWidget : SceneWidget
    {
        protected bool IsExternalWorld;
        protected World WorldScene;

        public bool ShowDebugInfo = true;

        // Create a scene to render a world and manage the worklld itself
        public Scene3dWidget(Window win) : base(win)
        {
            IsExternalWorld = false;
            WorldScene = new World();
        }

        // Crate a scene for an exteranly managed world, eg a camera view into one
        public Scene3dWidget(Window win, World world) : base(win)
        {
            IsExternalWorld = true;
            WorldScene = world;
        }

        public override void Render(double dt)
        {
            base.Render(dt);

            if (!IsExternalWorld)
                WorldScene.Update(dt);

            WorldScene.Render(dt, CommandList);
        }

        public override void DrawOverlays()
        {
            base.DrawOverlays();

            if (ShowDebugInfo)
                DrawDebugOverlay();
        }

        private void DrawDebugOverlay()
        {

        }
    }
}
