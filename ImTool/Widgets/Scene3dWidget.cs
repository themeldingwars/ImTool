
using ImTool.Scene3D;

namespace ImTool
{
    public class Scene3dWidget : SceneWidget
    {
        protected bool IsExternalWorld;
        protected World WorldScene;
        protected CameraActor Camera;

        public bool ShowDebugInfo = true;

        // Create a scene to render a world and manage the world itself
        public Scene3dWidget(Window win) : base(win)
        {
            IsExternalWorld = false;
            WorldScene = new World(win);
            WorldScene.Init(this);
            WorldScene.RegisterViewport(this);
        }

        // Crate a scene for an exteranly managed world, eg a camera view into one
        public Scene3dWidget(Window win, World world) : base(win)
        {
            IsExternalWorld = true;
            WorldScene = world;
        }

        public void SetCamera(CameraActor camera)
        {
            Camera = camera;
        }

        public CameraActor GetCamera() => Camera;

        public override void Render(double dt)
        {
            base.Render(dt);

            WorldScene.CurrentSceneViewport = this;

            if (!IsExternalWorld)
                WorldScene.Update(dt);

            WorldScene.Render(dt, CommandList, Camera);
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
