
using ImGuiNET;
using ImTool.Scene3D;
using Octokit;
using System.Diagnostics;
using System.Numerics;

namespace ImTool
{
    public class Scene3dWidget : SceneWidget
    {
        protected bool IsExternalWorld;
        protected World WorldScene;
        protected CameraActor Camera;
        private Vector2 LastMousePos;

        protected float CamMoveSpeed    = 10.0f;
        protected float MouseSenstivity = 5f;

        public bool ShowDebugInfo = true;

        // Create a scene to render a world and manage the world itself
        public Scene3dWidget(Window win) : base(win)
        {
            IsExternalWorld = false;
            WorldScene = new World(win);
            WorldScene.RegisterViewport(this);
            WorldScene.Init(this);
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

        public void HandleInput(double dt)
        {
            if (IsHovered && ImGui.IsMouseDown(ImGuiMouseButton.Middle))
            {
                var cammoveSpeed = (float)(CamMoveSpeed * dt);

                if (ImGui.IsKeyDown((int)Veldrid.Key.W))
                    Camera.Transform.Position += Camera.Transform.Forward * cammoveSpeed;
                else if (ImGui.IsKeyDown((int)Veldrid.Key.S))
                    Camera.Transform.Position += Camera.Transform.Forward * -cammoveSpeed;

                if (ImGui.IsKeyDown((int)Veldrid.Key.A))
                    Camera.Transform.Position += Camera.Transform.Left * cammoveSpeed;
                else if (ImGui.IsKeyDown((int)Veldrid.Key.D))
                    Camera.Transform.Position += Camera.Transform.Left * -cammoveSpeed;

                if (ImGui.IsKeyDown((int)Veldrid.Key.Q))
                    Camera.Transform.Position += Camera.Transform.Up * cammoveSpeed;
                else if (ImGui.IsKeyDown((int)Veldrid.Key.E))
                    Camera.Transform.Position += Camera.Transform.Up * -cammoveSpeed;

                var mouseDelta = LastMousePos - ImGui.GetMousePos();
                LastMousePos = ImGui.GetMousePos();
                var angles = Camera.Transform.RotationEuler;
                angles.Y += (float)(-mouseDelta.Y * (MouseSenstivity * dt));
                angles.X += (float)(mouseDelta.X * (MouseSenstivity * dt));
                Camera.Transform.RotationEuler = angles;
            }

            LastMousePos = ImGui.GetMousePos();
        }

        public override void Render(double dt)
        {
            base.Render(dt);

            // Update camera aspect
            Camera.AspectRatio = (float)FrameBuff.Width / (float)FrameBuff.Height;

            HandleInput(dt);

            if (!IsExternalWorld)
                WorldScene.Update(dt);

            WorldScene.Render(dt, CommandList, Camera);
        }

        public override void DrawOverlays(double dt)
        {
            base.DrawOverlays(dt);

            if (ShowDebugInfo)
                DrawDebugOverlay();
        }

        private void DrawDebugOverlay()
        {

        }
    }
}
