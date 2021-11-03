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
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override SkillActivateAnimationType GetSkillActivateAnimationType(int dataId)
        {
            throw new System.NotImplementedException();
        }

        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            throw new System.NotImplementedException();
        }

        public override void PlaySkillCastClip(int dataId, float duration)
        {
            throw new System.NotImplementedException();
        }

        public override void StopSkillCastAnimation()
        {
            throw new System.NotImplementedException();
        }

        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            throw new System.NotImplementedException();
        }

        public override void StopWeaponChargeAnimation()
        {
            throw new System.NotImplementedException();
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
            // NOTE: Cannot find ground distance and ground angle from direction
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
