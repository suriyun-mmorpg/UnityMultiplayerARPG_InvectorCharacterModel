using Invector;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorCharacterModel : BaseCharacterModel
    {
        // NOTE: Get animator parameters from `vAnimatorParameters`
        public Animator animator;
        [Range(0f, 1f)]
        public float strafeAnimationSmooth = 0.2f;
        [Range(0f, 1f)]
        public float freeAnimationSmooth = 0.2f;
        [Tooltip("Rotation speed of the character while strafing")]
        public float strafeRotationSpeed = 20f;
        [Tooltip("Rotation speed of the character while not strafing")]
        public float freeRotationSpeed = 20f;
        [Tooltip("Control the speed of the Animator Layer OnlyArms Weight")]
        public float onlyArmsSpeed = 25f;
        [Tooltip("Time to keep aiming after shot")]
        public float hipfireAimTime = 2f;
        [Tooltip("While in Free Locomotion the character will lean to left/right when steering")]
        public bool useLeanMovementAnim = true;
        [Tooltip("Smooth value for the Lean Movement animation")]
        [Range(0.01f, 0.1f)]
        public float leanSmooth = 0.05f;
        [Tooltip("Check this to use the TurnOnSpot animations while the character is stading still and rotating in place")]
        public bool useTurnOnSpotAnim = true;

        protected float onlyArmsLayerWeight = 0f;
        protected float aimTimming = 0f;
        protected int onlyArmsLayer;
        protected bool dirtyIsDead = true;
        /// <summary>
        /// Animator Hash for IsShoot parameter 
        /// </summary>
        internal readonly int IsShoot = Animator.StringToHash("Shoot");
        /// <summary>
        /// Animator Hash for Reload parameter 
        /// </summary>
        internal readonly int Reload = Animator.StringToHash("Reload");
        /// <summary>
        /// Animator Hash for ReloadID parameter 
        /// </summary>
        internal readonly int ReloadID = Animator.StringToHash("ReloadID");
        /// <summary>
        /// Animator Hash for IsCrawling parameter
        /// </summary>
        internal readonly int IsCrawling = Animator.StringToHash("IsCrawling");
        /// <summary>
        /// sets the rotationMagnitude to update the animations in the animator controller
        /// </summary>
        internal float rotationMagnitude;
        /// <summary>
        /// Last angle of the character used to calculate rotationMagnitude
        /// </summary>
        protected Vector3 lastCharacterAngle;

        protected override void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            onlyArmsLayer = animator.GetLayerIndex("OnlyArms");
        }

        private void Update()
        {
            onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, (animator.GetFloat(vAnimatorParameters.UpperBody_ID) > 0f) ? 1f : 0f, onlyArmsSpeed * Time.deltaTime);
            animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
            if (aimTimming > 0)
            {
                aimTimming -= Time.deltaTime;
                if (aimTimming <= 0f)
                    animator.SetBool(vAnimatorParameters.IsAiming, false);
            }
        }

        public bool GetAttackAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            if (GameInstance.Singleton.InvectorSwordWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorTwoHandSwordWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorDualSwordWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorPistolWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.233f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorRifleWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.133f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorShotgunWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.6f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorSniperWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.6f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorRpgWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.6f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorBowWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.208f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            // Unarmed
            totalDuration = 0.542f;
            triggerDurations = new float[1] { totalDuration * 0.5f };
            return true;
        }

        public bool GetReloadAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            if (GameInstance.Singleton.InvectorPistolWeaponTypes.Contains(dataId))
            {
                totalDuration = 2.567f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorRifleWeaponTypes.Contains(dataId))
            {
                totalDuration = 2.8f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorShotgunWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.723f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorSniperWeaponTypes.Contains(dataId))
            {
                totalDuration = 2.8f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorRpgWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (GameInstance.Singleton.InvectorBowWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            totalDuration = 0;
            triggerDurations = new float[0];
            return false;
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetAttackAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            animationIndex = 0;
            return GetAttackAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetReloadAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetAttackAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            animationIndex = 0;
            return GetAttackAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetReloadAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override SkillActivateAnimationType GetSkillActivateAnimationType(int dataId)
        {
            throw new System.NotImplementedException();
        }

        public override void PlaySkillCastClip(int dataId, float duration)
        {
            // TODO: May implement this?
        }

        public override void StopSkillCastAnimation()
        {
            // TODO: May implement this?
        }

        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            // TODO: Implement this, for only throwing item and bow
        }

        public override void StopWeaponChargeAnimation()
        {
            // TODO: Implement this, for only throwing item and bow
        }

        public void PlayAttackAnimation(int dataId)
        {
            if (GameInstance.Singleton.InvectorSwordWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                animator.SetInteger(vAnimatorParameters.AttackID, 1);
                animator.SetTrigger(vAnimatorParameters.WeakAttack);
            }
            else if (GameInstance.Singleton.InvectorTwoHandSwordWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                animator.SetInteger(vAnimatorParameters.AttackID, 4);
                animator.SetTrigger(vAnimatorParameters.WeakAttack);
            }
            else if (GameInstance.Singleton.InvectorDualSwordWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                animator.SetInteger(vAnimatorParameters.AttackID, 5);
                animator.SetTrigger(vAnimatorParameters.WeakAttack);
            }
            else if (GameInstance.Singleton.InvectorPistolWeaponTypes.Contains(dataId))
            {
                animator.SetBool(vAnimatorParameters.IsAiming, true);
                animator.SetFloat(vAnimatorParameters.Shot_ID, 1);
                animator.SetTrigger(IsShoot);
                aimTimming = hipfireAimTime;
            }
            else if (GameInstance.Singleton.InvectorRifleWeaponTypes.Contains(dataId))
            {
                animator.SetBool(vAnimatorParameters.IsAiming, true);
                animator.SetFloat(vAnimatorParameters.Shot_ID, 2);
                animator.SetTrigger(IsShoot);
                aimTimming = hipfireAimTime;
            }
            else if (GameInstance.Singleton.InvectorShotgunWeaponTypes.Contains(dataId))
            {
                animator.SetBool(vAnimatorParameters.IsAiming, true);
                animator.SetFloat(vAnimatorParameters.Shot_ID, 3);
                animator.SetTrigger(IsShoot);
                aimTimming = hipfireAimTime;
            }
            else if (GameInstance.Singleton.InvectorSniperWeaponTypes.Contains(dataId))
            {
                animator.SetBool(vAnimatorParameters.IsAiming, true);
                animator.SetFloat(vAnimatorParameters.Shot_ID, 4);
                animator.SetTrigger(IsShoot);
                aimTimming = hipfireAimTime;
            }
            else if (GameInstance.Singleton.InvectorRpgWeaponTypes.Contains(dataId))
            {
                animator.SetBool(vAnimatorParameters.IsAiming, true);
                animator.SetFloat(vAnimatorParameters.Shot_ID, 5);
                animator.SetTrigger(IsShoot);
                aimTimming = hipfireAimTime;
            }
            else if (GameInstance.Singleton.InvectorBowWeaponTypes.Contains(dataId))
            {
                animator.SetBool(vAnimatorParameters.IsAiming, true);
                animator.SetFloat(vAnimatorParameters.Shot_ID, 6);
                animator.SetTrigger(IsShoot);
                aimTimming = hipfireAimTime;
            }
            else
            {
                animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                animator.SetInteger(vAnimatorParameters.AttackID, 0);
                animator.SetTrigger(vAnimatorParameters.WeakAttack);
            }
        }

        public void PlayReloadAnimation(int dataId)
        {
            if (GameInstance.Singleton.InvectorPistolWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 1);
                animator.SetTrigger(Reload);
            }
            if (GameInstance.Singleton.InvectorRifleWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 2);
                animator.SetTrigger(Reload);
            }
            if (GameInstance.Singleton.InvectorShotgunWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 3);
                animator.SetTrigger(Reload);
            }
            if (GameInstance.Singleton.InvectorSniperWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 2);
                animator.SetTrigger(Reload);
            }
            if (GameInstance.Singleton.InvectorRpgWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 4);
                animator.SetTrigger(Reload);
            }
            if (GameInstance.Singleton.InvectorBowWeaponTypes.Contains(dataId))
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 5);
                animator.SetTrigger(Reload);
            }
        }

        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            switch (animActionType)
            {
                case AnimActionType.AttackLeftHand:
                case AnimActionType.AttackRightHand:
                    PlayAttackAnimation(dataId);
                    break;
                case AnimActionType.ReloadLeftHand:
                case AnimActionType.ReloadRightHand:
                    PlayReloadAnimation(dataId);
                    break;
            }
        }

        public override void StopActionAnimation()
        {
            throw new System.NotImplementedException();
        }

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            IWeaponItem rightHandWeaponItem = equipWeapons.GetRightHandWeaponItem();
            int dataId = rightHandWeaponItem != null ? rightHandWeaponItem.DataId : 0;
            if (GameInstance.Singleton.InvectorPistolWeaponTypes.Contains(dataId))
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 1);
            }
            if (GameInstance.Singleton.InvectorRifleWeaponTypes.Contains(dataId))
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 2);
            }
            else if (GameInstance.Singleton.InvectorShotgunWeaponTypes.Contains(dataId))
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 3);
            }
            else if (GameInstance.Singleton.InvectorSniperWeaponTypes.Contains(dataId))
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 2);
            }
            else if (GameInstance.Singleton.InvectorRpgWeaponTypes.Contains(dataId))
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 4);
            }
            else if (GameInstance.Singleton.InvectorBowWeaponTypes.Contains(dataId))
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 5);
            }
            else
            {
                animator.SetFloat(vAnimatorParameters.UpperBody_ID, 0);
            }
        }

        public override void PlayMoveAnimation()
        {
            if (dirtyIsDead != isDead)
            {
                dirtyIsDead = isDead;
                ResetAnimatorParameters();
                animator.SetBool(vAnimatorParameters.IsDead, isDead);
            }

            if (isDead)
                return;

            bool isStrafing = movementState.Has(MovementState.Left) || movementState.Has(MovementState.Right) || movementState.Has(MovementState.Backward);
            bool isSprinting = extraMovementState == ExtraMovementState.IsSprinting;
            // NOTE: Actually has no `isSliding` usage
            bool isSliding = false;
            // NOTE: Actually has no `isRolling` usage
            bool isRolling = false;
            bool isCrouching = extraMovementState == ExtraMovementState.IsCrouching;
            bool isCrawling = extraMovementState == ExtraMovementState.IsCrawling;
            bool isGrounded = movementState.Has(MovementState.IsGrounded);
            // NOTE: Cannot find ground distance and ground angle from direction yet
            float groundDistance = 0f;
            float groundAngleFromDirection = 0f;
            float verticalVelocity = 0f;
            float horizontalSpeed = 0f;
            float verticalSpeed = 0f;
            float inputMagnitude = 0f;
            float stopMoveWeight = 0f;

            var eulerDifference = transform.eulerAngles - lastCharacterAngle;
            var magnitude = (eulerDifference.NormalizeAngle().y / (isStrafing ? strafeRotationSpeed : freeRotationSpeed));
            rotationMagnitude = magnitude;
            lastCharacterAngle = transform.eulerAngles;

            if (movementState.Has(MovementState.Forward))
            {
                verticalSpeed = 1f;
                inputMagnitude = 1f;
            }
            if (movementState.Has(MovementState.Backward))
            {
                verticalSpeed = -1f;
                inputMagnitude = 1f;
            }
            if (movementState.Has(MovementState.Right))
            {
                horizontalSpeed = 1f;
                inputMagnitude = 1f;
            }
            if (movementState.Has(MovementState.Left))
            {
                horizontalSpeed = -1f;
                inputMagnitude = 1f;
            }

            if (movementState.Has(MovementState.IsJump))
            {
                if (inputMagnitude < 0.1f)
                {
                    animator.CrossFadeInFixedTime("Jump", 0.1f);
                }
                else
                {
                    animator.CrossFadeInFixedTime("JumpMove", .2f);
                }
            }
            else if (!movementState.Has(MovementState.IsGrounded))
            {
                // Falling
                verticalVelocity = 10f;
            }

            if (extraMovementState == ExtraMovementState.IsSprinting)
                inputMagnitude *= 1.5f;

            animator.SetBool(vAnimatorParameters.IsStrafing, isStrafing);
            animator.SetBool(vAnimatorParameters.IsSprinting, isSprinting);
            animator.SetBool(vAnimatorParameters.IsSliding, isSliding && !isRolling);
            animator.SetBool(vAnimatorParameters.IsCrouching, isCrouching);
            animator.SetBool(IsCrawling, isCrawling);
            animator.SetBool(vAnimatorParameters.IsGrounded, isGrounded);
            animator.SetFloat(vAnimatorParameters.GroundDistance, groundDistance);
            animator.SetFloat(vAnimatorParameters.GroundAngle, groundAngleFromDirection);
            animator.SetBool(vAnimatorParameters.CanAim, true);

            if (!isGrounded)
            {
                animator.SetFloat(vAnimatorParameters.VerticalVelocity, verticalVelocity);
            }

            if (isStrafing)
            {
                animator.SetFloat(vAnimatorParameters.InputHorizontal, horizontalSpeed, strafeAnimationSmooth, Time.unscaledDeltaTime);
                animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, strafeAnimationSmooth, Time.unscaledDeltaTime);
            }
            else
            {
                animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, freeAnimationSmooth, Time.unscaledDeltaTime);
                animator.SetFloat(vAnimatorParameters.InputHorizontal, 0, freeAnimationSmooth, Time.unscaledDeltaTime);
            }

            animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(inputMagnitude, 0f, stopMoveWeight), isStrafing ? strafeAnimationSmooth : freeAnimationSmooth, Time.unscaledDeltaTime);

            if (useLeanMovementAnim && inputMagnitude > 0.1f)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, leanSmooth, Time.fixedDeltaTime);
            }
            else if (useTurnOnSpotAnim)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, rotationMagnitude == 0 ? 0.1f : 0.01f, Time.fixedDeltaTime);
            }
        }

        public void ResetAnimatorParameters()
        {
            animator.SetBool(vAnimatorParameters.IsSprinting, false);
            animator.SetBool(vAnimatorParameters.IsSliding, false);
            animator.SetBool(vAnimatorParameters.IsCrouching, false);
            animator.SetBool(IsCrawling, false);
            animator.SetBool(vAnimatorParameters.IsGrounded, true);
            animator.SetFloat(vAnimatorParameters.GroundDistance, 0f);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0);
            animator.SetFloat(vAnimatorParameters.InputMagnitude, 0);
            animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0);
        }
    }
}
