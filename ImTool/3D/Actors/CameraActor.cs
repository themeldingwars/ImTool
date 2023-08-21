using ImGuiNET;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Veldrid.Utilities;
using static ImTool.Scene3D.CameraActor;
using static ImTool.Scene3D.Components.DebugShapesComp;

namespace ImTool.Scene3D
{
    public class CameraActor : Actor
    {
        public ViewData ViewData = new();
        public Matrix4x4 ProjectionMat { get; private set; }
        public Matrix4x4 ViewMat { get; set; }
        public CameraType CamType;
        public BoundingFrustum Frustum;

        public float FovDeg
        {
            get => _fov * (180 / MathF.PI);
            set
            {
                _fov = value * (MathF.PI / 180);
            }
        }

        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                _aspectRatio = value;
            }
        }

        public float OrthographicWidth
        {
            get => _orthographicWidth;
            set
            {
                _orthographicWidth = value;
            }
        }

        public CameraType CameraViewType
        {
            get => _cameraType;
            set
            {
                _cameraType = value;
            }
        }

        public float NearPlaneDist
        {
            get => _nearPlaneDist;
            set
            {
                _nearPlaneDist = Math.Clamp(value, 0, float.MaxValue);
            }
        }

        public float FarPlaneDist
        {
            get => _farPlaneDist;
            set
            {
                _farPlaneDist = Math.Clamp(value, 0, float.MaxValue);
            }
        }

        private float _fov;
        private float _aspectRatio;
        private float _orthographicWidth;
        private CameraType _cameraType;
        private float _nearPlaneDist;
        private float _farPlaneDist;

        public CameraActor()
        {
            _cameraType        = CameraType.Perspective;
            _aspectRatio       = 1f;
            _fov               = 1.5f;
            _nearPlaneDist     = 0.1f;
            _farPlaneDist      = 1000f;
            _orthographicWidth = 35f;

            FovDeg = 75;

            Flags = ActorFlags.DontRender;
        }

        public override void Update(double dt)
        {
            var backEndType = World.MainWindow.GetGraphicsDevice().ResourceFactory.BackendType;
            var flipY       = backEndType is Veldrid.GraphicsBackend.Vulkan or Veldrid.GraphicsBackend.OpenGL;

            UpdateProjectionMat(flipY);
            UpdateViewMat(flipY);

            Frustum = new BoundingFrustum(ViewMat * ProjectionMat);

            if (BoundsDebugHandle != null)
            {
                ((Fustrum)BoundsDebugHandle).Frustum = Frustum;
                BoundsDebugHandle.Update();
            }
        }

        protected override void SetBoundsShape()
        {
            BoundsDebugHandle       = World.DebugShapes.AddFustrum(Frustum);
            BoundsDebugHandle.Color = new Vector4(0f, 0f, 1f, 1f);
        }

        public void UpdateProjectionMat(bool flipY)
        {
            if (CamType == CameraType.Perspective)
            {
                var aspectRatio = flipY ? -_aspectRatio : _aspectRatio;
                ProjectionMat = Matrix4x4.CreatePerspectiveFieldOfView(_fov, aspectRatio, _nearPlaneDist, _farPlaneDist);
            }
            else if (CamType == CameraType.Orthographic)
            {
                var aspectRatio = flipY ? (_orthographicWidth / _aspectRatio) : -(_orthographicWidth / _aspectRatio);
                ProjectionMat = Matrix4x4.CreateOrthographic(-_orthographicWidth, aspectRatio, _nearPlaneDist, _farPlaneDist);
            }

            ViewData.Proj        = ProjectionMat;
            ViewData.CamNearDist = _nearPlaneDist;
            ViewData.CamFarDist  = _farPlaneDist;
        }

        public void UpdateViewMat(bool flipY)
        {
            ViewMat         = Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Transform.Forward, flipY ? -Vector3.UnitY : Vector3.UnitY);
            //Frustum         = new BoundingFrustum(ViewMat);

            ViewData.View   = ViewMat;
            ViewData.CamPos = Transform.Position;
            ViewData.CamDir = Transform.Forward;
        }

        public override void DrawInspector()
        {
            base.DrawInspector();

            ImGui.Text("FOV");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderAngle("FOV", ref _fov, 1, 179);

            ImGui.Text("Near Plane");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat("Near Plane", ref _nearPlaneDist, 0.001f, 500f);

            ImGui.Text("Far Plane");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat("Far Plane", ref _farPlaneDist, 1f, 1000f);

            ImGui.Text("Camera Type");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("###CameraType", CamType.ToString()))
            {
                if (ImGui.Selectable("Perspective"))
                    CamType = CameraType.Perspective;

                if (ImGui.Selectable("Orthographic"))
                    CamType = CameraType.Orthographic;

                ImGui.EndCombo();
            }

            bool isFustrumShown = BoundsDebugHandle != null;
            if (ImGui.Checkbox("Show Fustrum", ref isFustrumShown))
                ShowBounds(isFustrumShown);
        }

        public enum CameraType : byte
        {
            Perspective,
            Orthographic
        }
    }
}
