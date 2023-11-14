using Invector.vCamera;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public vThirdPersonCamera invectorCam;
        public string pitchAxisName = "Mouse Y";
        public float pitchRotateSpeedScale = 1f;
        public string yawAxisName = "Mouse X";
        public float yawRotateSpeedScale = 1f;
        public string zoomAxisName = "Mouse ScrollWheel";
        public float zoomSpeedScale = 1f;

        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public Camera Camera { get; protected set; }
        public Transform CameraTransform { get { return Camera.transform; } }
        public Transform FollowingEntityTransform
        {
            get
            {
                return invectorCam.mainTarget;
            }
            set
            {
                invectorCam.SetMainTarget(value);
                if (!invectorCam.isInit)
                    invectorCam.Init();
            }
        }
        public Vector3 TargetOffset
        {
            get
            {
                return new Vector3(invectorCam.CameraStateList.tpCameraStates[0].right, invectorCam.CameraStateList.tpCameraStates[0].height, 0f);
            }
            set
            {
                invectorCam.CameraStateList.tpCameraStates[0].right = value.x;
                invectorCam.CameraStateList.tpCameraStates[0].height = value.y;
            }
        }
        public float CameraFov
        {
            get
            {
                return invectorCam.CameraStateList.tpCameraStates[0].fov;
            }
            set
            {
                invectorCam.CameraStateList.tpCameraStates[0].fov = value;
            }
        }
        public float CameraNearClipPlane
        {
            get
            {
                return Camera.nearClipPlane;
            }
            set
            {
                Camera.nearClipPlane = value;
            }
        }
        public float CameraFarClipPlane
        {
            get
            {
                return Camera.farClipPlane;
            }
            set
            {
                Camera.farClipPlane = value;
            }
        }
        public float MinZoomDistance
        {
            get
            {
                return invectorCam.CameraStateList.tpCameraStates[0].minDistance;
            }
            set
            {
                invectorCam.CameraStateList.tpCameraStates[0].minDistance = value;
            }
        }
        public float MaxZoomDistance
        {
            get
            {
                return invectorCam.CameraStateList.tpCameraStates[0].maxDistance;
            }
            set
            {
                invectorCam.CameraStateList.tpCameraStates[0].maxDistance = value;
            }
        }
        public float CurrentZoomDistance
        {
            get
            {
                return invectorCam.distance;
            }
            set
            {
                invectorCam.distance = value;
            }
        }
        public bool EnableWallHitSpring { get; set; }
        public bool UpdateRotation { get; set; }
        public bool UpdateRotationX { get; set; }
        public bool UpdateRotationY { get; set; }
        public bool UpdateZoom
        {
            get
            {
                return invectorCam.CameraStateList.tpCameraStates[0].useZoom;
            }
            set
            {
                invectorCam.CameraStateList.tpCameraStates[0].useZoom = true;
            }
        }

        private float _pitch;
        private float _yaw;
        private float _zoom;

        public void Init()
        {

        }

        protected virtual void Update()
        {
            if (PlayerCharacterEntity == null)
            {
                return;
            }

            if (invectorCam.selfRigidbody)
            {
                invectorCam.selfRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }

            _pitch = 0f;
            if (UpdateRotation || UpdateRotationX)
            {
                _pitch = InputManager.GetAxis(pitchAxisName, false) * pitchRotateSpeedScale;
            }

            _yaw = 0f;
            if (UpdateRotation || UpdateRotationY)
            {
                _yaw = InputManager.GetAxis(yawAxisName, false) * yawRotateSpeedScale;
            }

            invectorCam.RotateCamera(_yaw, _pitch);

            _zoom = 0f;
            if (UpdateZoom)
            {
                _zoom = InputManager.GetAxis(zoomAxisName, false) * zoomSpeedScale;
            }

            invectorCam.Zoom(_zoom);
        }

        protected virtual void LateUpdate()
        {
        }

        public virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = characterEntity;
            Camera = invectorCam.GetComponentInChildren<Camera>();
        }

        public virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
            FollowingEntityTransform = null;
        }
    }
}
