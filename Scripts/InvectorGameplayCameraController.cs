using Invector.vCamera;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public vThirdPersonCamera invectorCam;
        public string pitchAxisName = "Mouse Y";
        public float pitchRotateSpeed = 4f;
        public float pitchRotateSpeedScale = 1f;
        public float pitchBottomClamp = -30f;
        public float pitchTopClamp = 70f;
        public string yawAxisName = "Mouse X";
        public float yawRotateSpeedScale = 1f;
        public string zoomAxisName = "Mouse ScrollWheel";
        public float zoomSpeedScale = 1f;
        public float zoomSmoothTime = 0.25f;
        public float zoomMin = 2f;
        public float zoomMax = 8f;

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
            }
        }
        public Vector3 TargetOffset
        {
            get
            {
                return new Vector3(invectorCam.currentState.right, invectorCam.currentState.height, invectorCam.currentState.forward);
            }
            set
            {
                invectorCam.currentState.right = value.x;
                invectorCam.currentState.height = value.y;
                invectorCam.currentState.forward = value.z;
            }
        }
        public float CameraFov
        {
            get
            {
                return invectorCam.currentState.fov;
            }
            set
            {
                invectorCam.currentState.fov = value;
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
                return invectorCam.currentState.minDistance;
            }
            set
            {
                invectorCam.currentState.minDistance = value;
            }
        }
        public float MaxZoomDistance
        {
            get
            {
                return invectorCam.currentState.maxDistance;
            }
            set
            {
                invectorCam.currentState.maxDistance = value;
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
                return invectorCam.currentState.useZoom;
            }
            set
            {
                invectorCam.currentState.useZoom = true;
            }
        }

        private float pitch;
        private float yaw;
        private float zoom;
        private float zoomVelocity;

        protected virtual void Update()
        {
            if (invectorCam.selfRigidbody)
            {
                invectorCam.selfRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            }

            pitch = 0f;
            if (UpdateRotation || UpdateRotationX)
            {
                pitch = InputManager.GetAxis(pitchAxisName, false) * pitchRotateSpeedScale;
            }

            yaw = 0f;
            if (UpdateRotation || UpdateRotationY)
            {
                yaw = InputManager.GetAxis(yawAxisName, false) * yawRotateSpeedScale;
            }

            invectorCam.RotateCamera(yaw, pitch);

            zoom = 0f;
            if (UpdateZoom)
            {
                zoom = InputManager.GetAxis(zoomAxisName, false) * zoomSpeedScale;
            }

            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
            invectorCam.Zoom(zoom);
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
