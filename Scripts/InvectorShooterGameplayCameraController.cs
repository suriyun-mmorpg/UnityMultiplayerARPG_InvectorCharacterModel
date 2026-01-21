using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorShooterGameplayCameraController : InvectorGameplayCameraController, IShooterGameplayCameraController
    {
        public float recoilRateX = 0.001f;
        public float recoilRateY = 0.001f;
        public float recoilRateZ = 0.001f;
        public float recoilReturnSpeed = 2f;
        public float recoilSmoothing = 6f;

        public bool EnableAimAssist { get; set; }
        public bool EnableAimAssistX { get; set; }
        public bool EnableAimAssistY { get; set; }
        public bool AimAssistPlayer { get; set; }
        public bool AimAssistMonster { get; set; }
        public bool AimAssistBuilding { get; set; }
        public bool AimAssistHarvestable { get; set; }
        public float AimAssistRadius { get; set; }
        public float AimAssistXSpeed { get; set; }
        public float AimAssistYSpeed { get; set; }
        public float AimAssistMaxAngleFromFollowingTarget { get; set; }
        public float CameraRotationSpeedScale { get; set; }
        public bool IsLeftViewSide { get; set; }
        public bool IsZoomAimming { get; set; }

        private Vector3 _targetRecoilRotation;
        private Vector3 _currentRecoilRotation;

        public void Recoil(float pitch, float yaw, float roll)
        {
            _targetRecoilRotation += new Vector3(-pitch * recoilRateX, yaw * recoilRateY, roll * recoilRateZ);
        }

        protected override void Update()
        {
            base.Update();
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, Time.fixedDeltaTime * recoilSmoothing);
            invectorCam.RotateCamera(_currentRecoilRotation.y, _currentRecoilRotation.x);
        }
    }
}