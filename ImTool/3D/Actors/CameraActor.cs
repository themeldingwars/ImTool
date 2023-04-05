using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ImTool.Scene3D.CameraActor;

namespace ImTool.Scene3D
{
    public class CameraActor : Actor
    {
        public ViewData ViewData = new();
        public Matrix4x4 ProjectionMat { get; private set; }
        public Matrix4x4 ViewMat { get; private set; }
        public CameraType CamType;

        public float FovDeg
        {
            get => _fov * (180 / MathF.PI);
            set
            {
                _fov = value * (MathF.PI / 180);
                UpdateProjectionMat();
            }
        }

        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                _aspectRatio = value;
                UpdateProjectionMat();
            }
        }

        public float OrthographicWidth
        {
            get => _orthographicWidth;
            set
            {
                _orthographicWidth = value;
                UpdateProjectionMat();
            }
        }

        public CameraType CameraViewType
        {
            get => _cameraType;
            set
            {
                _cameraType = value;
                UpdateProjectionMat();
            }
        }

        public float NearPlaneDist
        {
            get => _nearPlaneDist;
            set
            {
                _nearPlaneDist = value;
                UpdateProjectionMat();
            }
        }

        public float FarPlaneDist
        {
            get => _farPlaneDist;
            set
            {
                _farPlaneDist = value;
                UpdateProjectionMat();
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
            _nearPlaneDist     = 0.01f;
            _farPlaneDist      = 1000f;
            _orthographicWidth = 35f;

            FovDeg = 75;
        }

        public override void Update(double dt)
        {
            UpdateProjectionMat();
            UpdateViewMat();
        }

        public void UpdateProjectionMat()
        {
            if (CamType == CameraType.Perspective)
            {
                ProjectionMat = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, _nearPlaneDist, _farPlaneDist);
            }
            else if (CamType == CameraType.Orthographic)
            {
                ProjectionMat = Matrix4x4.CreateOrthographic(_orthographicWidth, _orthographicWidth / _aspectRatio, _nearPlaneDist, _farPlaneDist);
            }

            ViewData.Proj        = ProjectionMat;
            ViewData.CamNearDist = _nearPlaneDist;
            ViewData.CamFarDist  = _farPlaneDist;
        }

        public void UpdateViewMat()
        {
            ViewMat = Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Transform.Forward, Vector3.UnitY);

            ViewData.View   = ViewMat;
            ViewData.CamPos = Transform.Position;
            ViewData.CamDir = Transform.Forward;
        }

        public enum CameraType : byte
        {
            Perspective,
            Orthographic
        }
    }
}
