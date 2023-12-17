using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorShooterGameplayCameraController : InvectorGameplayCameraController, IShooterGameplayCameraController
    {
        public float recoilRateX = 1f;
        public float recoilRateY = 1f;
        public float recoilRateZ = 1f;
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

        private Vector3 _targetRecoilRotation;
        private Vector3 _currentRecoilRotation;

        public void Recoil(float x, float y, float z)
        {
            _targetRecoilRotation += new Vector3(x * recoilRateX, y * recoilRateY, z * recoilRateZ);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            // Update recoiling
            float deltaTime = Time.deltaTime;
            _targetRecoilRotation = Vector3.Lerp(_targetRecoilRotation, Vector3.zero, deltaTime * recoilReturnSpeed);
            _currentRecoilRotation = Vector3.Lerp(_currentRecoilRotation, _targetRecoilRotation, deltaTime * recoilSmoothing);
            invectorCam.transform.eulerAngles += _currentRecoilRotation;
        }
    }
}