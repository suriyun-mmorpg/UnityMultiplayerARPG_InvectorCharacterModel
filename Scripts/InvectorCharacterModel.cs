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
        /// <summary>
        /// Attack ID = 0
        /// </summary>
        public WeaponType[] unarmedWeaponTypes;
        /// <summary>
        /// Attack ID = 1
        /// </summary>
        public WeaponType[] swordWeaponTypes;
        /// <summary>
        /// Attack ID = 4
        /// </summary>
        public WeaponType[] twoHandSwordWeaponTypes;
        /// <summary>
        /// Attack ID = 5
        /// </summary>
        public WeaponType[] dualSwordWeaponTypes;
        /// <summary>
        /// Reload ID = 1
        /// Aiming ID = 1
        /// Shot ID = 1
        /// </summary>
        public WeaponType[] pistolWeaponTypes;
        /// <summary>
        /// Reload ID = 2
        /// Aiming ID = 2
        /// Shot ID = 2
        /// </summary>
        public WeaponType[] rifleWeaponTypes;
        /// <summary>
        /// Reload ID = 3
        /// Aiming ID = 3
        /// Shot ID = 3
        /// </summary>
        public WeaponType[] shotgunWeaponTypes;
        /// <summary>
        /// Reload ID = 2
        /// Aiming ID = 2
        /// Shot ID = 4
        /// </summary>
        public WeaponType[] sniperWeaponTypes;
        /// <summary>
        /// Reload ID = 4
        /// Aiming ID = 4
        /// Shot ID = 5
        /// </summary>
        public WeaponType[] rpgWeaponTypes;
        /// <summary>
        /// Reload ID = 5
        /// Aiming ID = 5
        /// Shot ID = 6
        /// </summary>
        public WeaponType[] bowWeaponTypes;
        // From `vCharacter.cs`
        protected vAnimatorParameter hitDirectionHash;
        protected vAnimatorParameter reactionIDHash;
        protected vAnimatorParameter triggerReactionHash;
        protected vAnimatorParameter triggerResetStateHash;
        protected vAnimatorParameter recoilIDHash;
        protected vAnimatorParameter triggerRecoilHash;
        // From `vThirdPersonMother.cs`
        internal AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;
        public int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        public int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        public int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        public int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        public int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        public int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }
        // From `vShooterMeleeInput.cs`
        internal Transform leftHand, rightHand, rightLowerArm, leftLowerArm, rightUpperArm, leftUpperArm;


        private readonly HashSet<int> _unarmedWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _swordWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _twoHandSwordWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _dualSwordWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _pistolWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _rifleWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _shotgunWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _sniperWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _rpgWeaponTypes = new HashSet<int>();
        private readonly HashSet<int> _bowWeaponTypes = new HashSet<int>();

        protected override void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            // Fill data ID hash set
            foreach (WeaponType type in unarmedWeaponTypes)
            {
                _unarmedWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in swordWeaponTypes)
            {
                _swordWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in twoHandSwordWeaponTypes)
            {
                _twoHandSwordWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in dualSwordWeaponTypes)
            {
                _dualSwordWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in pistolWeaponTypes)
            {
                _pistolWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in rifleWeaponTypes)
            {
                _rifleWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in shotgunWeaponTypes)
            {
                _shotgunWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in sniperWeaponTypes)
            {
                _sniperWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in rpgWeaponTypes)
            {
                _rpgWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in bowWeaponTypes)
            {
                _bowWeaponTypes.Add(type.DataId);
            }
        }

        public bool GetAttackAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            if (_unarmedWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.542f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_swordWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_twoHandSwordWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_dualSwordWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_pistolWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.233f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_rifleWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.133f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_shotgunWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.6f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_sniperWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.6f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_rpgWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.6f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_bowWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.208f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            totalDuration = 0;
            triggerDurations = new float[0];
            return false;
        }

        public bool GetReloadAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            if (_pistolWeaponTypes.Contains(dataId))
            {
                totalDuration = 2.567f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_rifleWeaponTypes.Contains(dataId))
            {
                totalDuration = 2.8f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_shotgunWeaponTypes.Contains(dataId))
            {
                totalDuration = 0.723f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_sniperWeaponTypes.Contains(dataId))
            {
                totalDuration = 2.8f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_rpgWeaponTypes.Contains(dataId))
            {
                totalDuration = 1f;
                triggerDurations = new float[1] { totalDuration * 0.5f };
                return true;
            }
            if (_bowWeaponTypes.Contains(dataId))
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

        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {

        }

        public override void StopActionAnimation()
        {
            throw new System.NotImplementedException();
        }

        public override void PlayMoveAnimation()
        {
            baseLayerInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
            underBodyInfo = animator.GetCurrentAnimatorStateInfo(underBodyLayer);
            rightArmInfo = animator.GetCurrentAnimatorStateInfo(rightArmLayer);
            leftArmInfo = animator.GetCurrentAnimatorStateInfo(leftArmLayer);
            upperBodyInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            fullBodyInfo = animator.GetCurrentAnimatorStateInfo(fullbodyLayer);

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
            float rotationMagnitude = 0f;

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
#if MIS_CRAWLING
            animator.SetBool("IsCrawling", isCrawling);
#endif
            animator.SetBool(vAnimatorParameters.IsGrounded, isGrounded);
            animator.SetBool(vAnimatorParameters.IsDead, isDead);
            animator.SetFloat(vAnimatorParameters.GroundDistance, groundDistance);
            animator.SetFloat(vAnimatorParameters.GroundAngle, groundAngleFromDirection);

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
            // TODO: Implement this later
            /*
            if (useLeanMovementAnim && inputMagnitude > 0.1f)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, leanSmooth, Time.fixedDeltaTime);
            }
            else if (useTurnOnSpotAnim)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, rotationMagnitude == 0 ? 0.1f : 0.01f, Time.fixedDeltaTime);
            }
            */
        }

        public virtual void SetActionState(int value)
        {
            animator.SetInteger(vAnimatorParameters.ActionState, value);
        }

        public virtual void ResetInputAnimatorParameters()
        {
            animator.SetBool(vAnimatorParameters.IsSprinting, false);
            animator.SetBool(vAnimatorParameters.IsSliding, false);
            animator.SetBool(vAnimatorParameters.IsCrouching, false);
            animator.SetBool(vAnimatorParameters.IsGrounded, true);
            animator.SetFloat(vAnimatorParameters.GroundDistance, 0f);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0f);
            animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f);
        }
    }
}
