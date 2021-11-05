using Invector;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorCharacterModel : BaseCharacterModel
    {
        [System.Serializable]
        public struct ClipLengthData
        {
            public float clipLength;
            [Range(0.01f, 1f)]
            public float triggerAtRate;

            public ClipLengthData(float clipLength, float triggerAtRate)
            {
                this.clipLength = clipLength;
                this.triggerAtRate = triggerAtRate;
            }

            public ClipLengthData(float clipLength) : this(clipLength, 0.5f)
            {
            }
        }

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
        [Tooltip("If this is true it will play attack animation clips by order, no random")]
        public bool playAttackClipByOrder = true;

        [Header("Animation Clip Length")]
        public ClipLengthData[] unarmedAttackClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.542f),
            new ClipLengthData(0.542f),
        };
        public ClipLengthData[] swordAttackClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(1f),
            new ClipLengthData(0.792f),
            new ClipLengthData(0.625f),
        };
        public ClipLengthData[] twoHandSwordAttackClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.6f),
            new ClipLengthData(0.9f),
            new ClipLengthData(1.833f),
        };
        public ClipLengthData[] dualSwordAttackClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(1f),
            new ClipLengthData(0.792f),
            new ClipLengthData(0.625f),
        };
        public ClipLengthData[] pistolRecoilClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.233f),
        };
        public ClipLengthData[] rifleRecoilClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.133f),
        };
        public ClipLengthData[] shotgunRecoilClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.6f),
        };
        public ClipLengthData[] sniperRecoilClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.6f),
        };
        public ClipLengthData[] rpgRecoilClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.6f),
        };
        public ClipLengthData[] bowRecoilClipLengths = new ClipLengthData[]
        {
            new ClipLengthData(0.208f),
        };
        public ClipLengthData pistolReloadClipLength = new ClipLengthData(2.567f);
        public ClipLengthData rifleReloadClipLength = new ClipLengthData(2.8f);
        public ClipLengthData shotgunReloadClipLength = new ClipLengthData(0.723f);
        public ClipLengthData sniperReloadClipLength = new ClipLengthData(2.8f);
        public ClipLengthData rpgReloadClipLength = new ClipLengthData(1f);
        public ClipLengthData bowReloadClipLength = new ClipLengthData(1f);

        protected float onlyArmsLayerWeight = 0f;
        protected float aimTimming = 0f;
        protected int onlyArmsLayer;
        protected int currentAttackClipIndex = 0;
        protected bool dirtyIsDead = true;
        protected bool jumpFallen = true;
        protected float upperBodyId = 0f;
        /// <summary>
        /// Animator Hash for RandomAttack parameter 
        /// </summary>
        internal readonly int RandomAttack = Animator.StringToHash("RandomAttack");
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
            base.Awake();
        }

        private void Update()
        {
            onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, upperBodyId > 0f ? 1f : 0f, onlyArmsSpeed * Time.deltaTime);
            animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
            if (aimTimming > 0)
            {
                aimTimming -= Time.deltaTime;
                if (aimTimming <= 0f)
                    animator.SetBool(vAnimatorParameters.IsAiming, false);
            }
        }

        public ClipLengthData[] GetAttackClipLengths(int dataId)
        {
            if (!GameInstance.WeaponTypes.ContainsKey(dataId))
            {
                return unarmedAttackClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sword)
            {
                return swordAttackClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.TwoHandSword)
            {
                return twoHandSwordAttackClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.DualSword)
            {
                return dualSwordAttackClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
            {
                return pistolRecoilClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
            {
                return rifleRecoilClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
            {
                return shotgunRecoilClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
            {
                return sniperRecoilClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
            {
                return rpgRecoilClipLengths;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
            {
                return bowRecoilClipLengths;
            }
            // Unarmed
            return unarmedAttackClipLengths;
        }

        public ClipLengthData GetReloadClipLength(int dataId)
        {
            if (!GameInstance.WeaponTypes.ContainsKey(dataId))
            {
                return default;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
            {
                return pistolReloadClipLength;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
            {
                return rifleReloadClipLength;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
            {
                return shotgunReloadClipLength;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
            {
                return sniperReloadClipLength;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
            {
                return rpgReloadClipLength;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
            {
                return bowReloadClipLength;
            }
            // Unknow
            return default;
        }

        public bool GetAttackAnimation(int dataId, int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            ClipLengthData tempClipLengthData = GetAttackClipLengths(dataId)[animationIndex];
            totalDuration = tempClipLengthData.clipLength;
            triggerDurations = new float[1] { totalDuration * tempClipLengthData.triggerAtRate };
            return true;
        }

        public bool GetReloadAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            ClipLengthData tempClipLengthData = GetReloadClipLength(dataId);
            totalDuration = tempClipLengthData.clipLength;
            triggerDurations = new float[1] { totalDuration * tempClipLengthData.triggerAtRate };
            return true;
        }

        public bool GetAttackAnimation(int dataId, out int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            ClipLengthData[] tempClipLengths = GetAttackClipLengths(dataId);
            if (playAttackClipByOrder)
            {
                if (currentAttackClipIndex >= tempClipLengths.Length)
                    currentAttackClipIndex = 0;
                animationIndex = currentAttackClipIndex;
            }
            else
            {
                animationIndex = Random.Range(0, tempClipLengths.Length);
            }
            return GetAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetAttackAnimation(dataId, out animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetReloadAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetAttackAnimation(dataId, out animationIndex, out triggerDurations, out totalDuration);
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

        public void PlayAttackAnimation(int dataId, int animationIndex)
        {
            if (!GameInstance.WeaponTypes.ContainsKey(dataId))
            {
                animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                animator.SetInteger(vAnimatorParameters.AttackID, 0);
                animator.SetTrigger(vAnimatorParameters.WeakAttack);
                animator.SetInteger(RandomAttack, animationIndex);
            }
            else
            {
                if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sword)
                {
                    animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(vAnimatorParameters.AttackID, 1);
                    animator.SetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(RandomAttack, animationIndex);
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.TwoHandSword)
                {
                    animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(vAnimatorParameters.AttackID, 4);
                    animator.SetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(RandomAttack, animationIndex);
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.DualSword)
                {
                    animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(vAnimatorParameters.AttackID, 5);
                    animator.SetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(RandomAttack, animationIndex);
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 1);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 2);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 3);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 4);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 5);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 6);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    aimTimming = hipfireAimTime;
                }
                else
                {
                    animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(vAnimatorParameters.AttackID, 0);
                    animator.SetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(RandomAttack, animationIndex);
                }
            }
            currentAttackClipIndex++;
        }

        public void PlayReloadAnimation(int dataId)
        {
            if (!GameInstance.WeaponTypes.ContainsKey(dataId))
            {
                // No defined weapon type, don't play it
                return;
            }
            if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 1);
                animator.SetTrigger(Reload);
            }
            else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 2);
                animator.SetTrigger(Reload);
            }
            else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 3);
                animator.SetTrigger(Reload);
            }
            else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 2);
                animator.SetTrigger(Reload);
            }
            else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
            {
                animator.ResetTrigger(Reload);
                animator.SetInteger(ReloadID, 4);
                animator.SetTrigger(Reload);
            }
            else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
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
                    PlayAttackAnimation(dataId, index);
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
            IWeaponItem rightHandWeaponItem = equipWeapons.GetRightHandWeaponItem();
            int dataId = rightHandWeaponItem != null ? rightHandWeaponItem.WeaponType.DataId : 0;
            if (!GameInstance.WeaponTypes.ContainsKey(dataId))
            {
                upperBodyId = 0;
            }
            else
            {
                if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
                {
                    upperBodyId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
                {
                    upperBodyId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
                {
                    upperBodyId = 3;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
                {
                    upperBodyId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
                {
                    upperBodyId = 4;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
                {
                    upperBodyId = 5;
                }
                else
                {
                    upperBodyId = 0;
                }
            }
            if (!enabled)
            {
                animator.SetLayerWeight(onlyArmsLayer, upperBodyId > 0f ? 1f : 0f);
            }
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, upperBodyId);
            currentAttackClipIndex = 0;
            base.SetEquipWeapons(equipWeapons);
        }

        private void OnDisable()
        {
            animator.SetLayerWeight(onlyArmsLayer, upperBodyId > 0f ? 1f : 0f);
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, upperBodyId);
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

            float deltaTime = Time.deltaTime;
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

            if (jumpFallen && movementState.Has(MovementState.IsJump))
            {
                jumpFallen = false;
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

            if (!jumpFallen && movementState.Has(MovementState.IsGrounded))
                jumpFallen = true;

            if (movementState.Has(MovementState.IsGrounded) && extraMovementState == ExtraMovementState.IsSprinting)
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
                animator.SetFloat(vAnimatorParameters.InputHorizontal, horizontalSpeed, strafeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, strafeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(inputMagnitude, 0f, stopMoveWeight), strafeAnimationSmooth, deltaTime);
            }
            else
            {
                animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, freeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputHorizontal, 0, freeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(inputMagnitude, 0f, stopMoveWeight), freeAnimationSmooth, deltaTime);
            }

            if (useLeanMovementAnim && inputMagnitude > 0.1f)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, leanSmooth, deltaTime);
            }
            else if (useTurnOnSpotAnim)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, rotationMagnitude == 0 ? 0.1f : 0.01f, deltaTime);
            }
        }

        public void ResetAnimatorParameters()
        {
            animator.SetBool(vAnimatorParameters.IsStrafing, false);
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
