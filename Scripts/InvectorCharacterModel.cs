using Invector;
using Invector.IK;
using Invector.vCharacterController;
using Invector.vEventSystems;
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
        public BaseCharacterEntity characterEntity;
        public vHeadTrack headTrack;
        [Range(0f, 1f)]
        public float strafeAnimationSmooth = 0.2f;
        [Range(0f, 1f)]
        public float freeAnimationSmooth = 0.2f;
        [Tooltip("Rotation speed of the character while strafing")]
        public float strafeRotationSpeed = 20f;
        [Tooltip("Rotation speed of the character while not strafing")]
        public float freeRotationSpeed = 20f;
        public float armIKSmoothIn = 10f;
        public float armIKSmoothOut = 25f;
        [Tooltip("Control the speed of the Animator Layer OnlyArms Weight")]
        public float onlyArmsSpeed = 25f;
        [Tooltip("smooth of the right hand when correcting the aim")]
        public float smoothArmIKRotation = 30f;
        [Tooltip("smooth of the right arm when correcting the aim")]
        public float smoothArmAlignWeight = 4f;
        [Tooltip("Limit the maxAngle for the right hand to correct the aim")]
        public float maxAimAngle = 60f;
        [Tooltip("Check this to sync the weapon aim to the camera aim")]
        public bool raycastAimTarget = true;
        [Tooltip("Check this to use IK on the left hand")]
        public bool useLeftIK = true;
        [Tooltip("Check this to use IK on the right hand")]
        public bool useRightIK = true;
        public Vector3 ikRotationOffset;
        public Vector3 ikPositionOffset;
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
        public vAnimatorStateInfos animatorStateInfos;

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

        public bool isReloading
        {
            get
            {
                return IsAnimatorTag("IsReloading") || characterEntity.IsPlayingReloadAnimation();
            }
        }
        public bool isShooting
        {
            get
            {
                return shootTimming > 0f;
            }
        }
        public bool isAiming
        {
            get
            {
                return aimTimming > 0f;
            }
        }
        public bool isAttacking
        {
            get
            {
                return IsAnimatorTag("Attack") || characterEntity.IsPlayingAttackOrUseSkillAnimation();
            }
        }
        public bool isEquipping
        {
            get
            {
                return IsAnimatorTag("IsEquipping");
            }
        }
        protected float onlyArmsLayerWeight = 0f;
        protected float supportIKWeight, weaponIKWeight;
        protected float shootTimming = 0f;
        protected float aimTimming = 0f;
        protected bool ignoreIK = false;
        protected int onlyArmsLayer;
        protected int currentAttackClipIndex = 0;
        protected bool dirtyIsDead = true;
        protected bool jumpFallen = true;
        protected bool isJumping = false;
        protected float movesetId = 0f;
        protected float upperBodyId = 0f;
        protected bool isStrafing = false;
        protected bool isSprinting = false;
        // NOTE: Actually has no `isSliding` usage
        protected bool isSliding = false;
        // NOTE: Actually has no `isRolling` usage
        protected bool isRolling = false;
        protected bool isCrouching = false;
        protected bool isCrawling = false;
        protected bool isGrounded = false;
        protected float verticalVelocity = 0f;
        protected float horizontalSpeed = 0f;
        protected float verticalSpeed = 0f;
        protected float inputMagnitude = 0f;
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

        internal AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;
        internal Transform leftHand, rightHand, rightLowerArm, leftLowerArm, rightUpperArm, leftUpperArm;

        protected float armAlignmentWeight;
        protected float aimWeight;

        protected Quaternion handRotation, upperArmRotation;
        internal GameObject aimAngleReference;

        private Quaternion upperArmRotationAlignment, handRotationAlignment;

        public int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        public int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        public int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        public int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        public int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        public int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }
        public vIKSolver LeftIK { get; set; }
        public vIKSolver RightIK { get; set; }
        private bool _ignoreIKFromAnimator;
        private bool IsIgnoreIK
        {
            get
            {
                return ignoreIK || _ignoreIKFromAnimator;
            }
        }

        protected override void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (characterEntity == null)
                characterEntity = GetComponentInChildren<BaseCharacterEntity>();
            if (headTrack == null)
                headTrack = GetComponentInChildren<vHeadTrack>();
            onlyArmsLayer = animator.GetLayerIndex("OnlyArms");

            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

            aimAngleReference = new GameObject("aimAngleReference");
            aimAngleReference.transform.rotation = transform.rotation;
            aimAngleReference.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Head));
            aimAngleReference.transform.localPosition = Vector3.zero;

            animatorStateInfos = new vAnimatorStateInfos(animator);
            base.Awake();
        }

        protected virtual void OnEnable()
        {
            animatorStateInfos.RegisterListener();
        }

        protected virtual void OnDisable()
        {
            animatorStateInfos.RemoveListener();
            animator.SetLayerWeight(onlyArmsLayer, upperBodyId > 0f ? 1f : 0f);
            animator.SetFloat(vAnimatorParameters.MoveSet_ID, movesetId);
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, upperBodyId);
        }

        public virtual bool IsAnimatorTag(string tag)
        {
            if (animator == null) return false;
            if (animatorStateInfos.HasTag(tag))
            {
                return true;
            }
            if (baseLayerInfo.IsTag(tag)) return true;
            if (underBodyInfo.IsTag(tag)) return true;
            if (rightArmInfo.IsTag(tag)) return true;
            if (leftArmInfo.IsTag(tag)) return true;
            if (upperBodyInfo.IsTag(tag)) return true;
            if (fullBodyInfo.IsTag(tag)) return true;
            return false;
        }

        protected void Update()
        {
            baseLayerInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
            underBodyInfo = animator.GetCurrentAnimatorStateInfo(underBodyLayer);
            rightArmInfo = animator.GetCurrentAnimatorStateInfo(rightArmLayer);
            leftArmInfo = animator.GetCurrentAnimatorStateInfo(leftArmLayer);
            upperBodyInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            fullBodyInfo = animator.GetCurrentAnimatorStateInfo(fullbodyLayer);

            float deltaTime = Time.deltaTime;
            onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, upperBodyId > 0f ? 1f : 0f, onlyArmsSpeed * deltaTime);
            animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
            animator.SetFloat(vAnimatorParameters.MoveSet_ID, movesetId, .2f, deltaTime);
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, upperBodyId);
            if (shootTimming > 0)
            {
                shootTimming -= deltaTime;
            }
            if (aimTimming > 0)
            {
                aimTimming -= deltaTime;
                if (aimTimming <= 0f)
                    animator.SetBool(vAnimatorParameters.IsAiming, false);
            }
            // Check if Animator state need to ignore IK
            _ignoreIKFromAnimator = IsAnimatorTag("IgnoreIK");
        }

        protected void LateUpdate()
        {
            UpdateHeadTrack();
            RotateAimArm();
            RotateAimHand();
            UpdateArmsIK();
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

        public override int GetLeftHandAttackRandomMax(int dataId)
        {
            ClipLengthData[] tempClipLengths = GetAttackClipLengths(dataId);
            return tempClipLengths.Length;
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

        public override int GetRightHandAttackRandomMax(int dataId)
        {
            ClipLengthData[] tempClipLengths = GetAttackClipLengths(dataId);
            return tempClipLengths.Length;
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animSpeedRate = 1f;
            return GetReloadAnimation(dataId, out triggerDurations, out totalDuration);
        }

        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            // TODO: May implement this?
            throw new System.NotImplementedException();
        }

        public override SkillActivateAnimationType GetSkillActivateAnimationType(int dataId)
        {
            // TODO: May implement this?
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
            GetAttackAnimation(dataId, animationIndex, out _, out shootTimming);
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
            ClipLengthData[] tempClipLengths = GetAttackClipLengths(dataId);
            if (currentAttackClipIndex >= tempClipLengths.Length)
                currentAttackClipIndex = 0;
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

        public override void SetEquipWeapons(IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
        {
            EquipWeapons newEquipWeapons;
            if (isWeaponsSheathed || selectableWeaponSets == null || selectableWeaponSets.Count == 0)
            {
                newEquipWeapons = new EquipWeapons();
            }
            else
            {
                if (equipWeaponSet >= selectableWeaponSets.Count)
                {
                    // Issues occuring, so try to simulate data
                    // Create a new list to make sure that changes won't be applied to the source list (the source list must be readonly)
                    selectableWeaponSets = new List<EquipWeapons>(selectableWeaponSets);
                    while (equipWeaponSet >= selectableWeaponSets.Count)
                    {
                        selectableWeaponSets.Add(new EquipWeapons());
                    }
                }
                newEquipWeapons = selectableWeaponSets[equipWeaponSet];
            }
            IWeaponItem rightHandWeaponItem = newEquipWeapons.GetRightHandWeaponItem();
            int dataId = rightHandWeaponItem != null ? rightHandWeaponItem.WeaponType.DataId : 0;
            if (!GameInstance.WeaponTypes.ContainsKey(dataId))
            {
                upperBodyId = 0;
                movesetId = 0;
            }
            else
            {
                if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sword)
                {
                    upperBodyId = 0;
                    movesetId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.TwoHandSword)
                {
                    upperBodyId = 0;
                    movesetId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.DualSword)
                {
                    upperBodyId = 0;
                    movesetId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
                {
                    upperBodyId = 1;
                    movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
                {
                    upperBodyId = 2;
                    movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
                {
                    upperBodyId = 3;
                    movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
                {
                    upperBodyId = 2;
                    movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
                {
                    upperBodyId = 4;
                    movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
                {
                    upperBodyId = 5;
                    movesetId = 1;
                }
                else
                {
                    upperBodyId = 0;
                    movesetId = 0;
                }
            }
            if (!enabled)
            {
                animator.SetLayerWeight(onlyArmsLayer, upperBodyId > 0f ? 1f : 0f);
                animator.SetFloat(vAnimatorParameters.MoveSet_ID, movesetId);
            }
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, upperBodyId);
            currentAttackClipIndex = 0;
            base.SetEquipWeapons(selectableWeaponSets, equipWeaponSet, isWeaponsSheathed);
        }

        public override void PlayJumpAnimation()
        {
            isJumping = true;
        }

        public override void PlayMoveAnimation()
        {
            if (dirtyIsDead != IsDead)
            {
                dirtyIsDead = IsDead;
                ResetAnimatorParameters();
                animator.SetBool(vAnimatorParameters.IsDead, IsDead);
            }

            if (IsDead)
            {
                isJumping = false;
                return;
            }

            float deltaTime = Time.deltaTime;
            isStrafing = MovementState.Has(MovementState.Left) || MovementState.Has(MovementState.Right) || MovementState.Has(MovementState.Backward);
            isSprinting = ExtraMovementState == ExtraMovementState.IsSprinting;
            // NOTE: Actually has no `isSliding` usage
            bool isSliding = false;
            // NOTE: Actually has no `isRolling` usage
            bool isRolling = false;
            isCrouching = ExtraMovementState == ExtraMovementState.IsCrouching;
            isCrawling = ExtraMovementState == ExtraMovementState.IsCrawling;
            isGrounded = MovementState.Has(MovementState.IsGrounded);
            // NOTE: Cannot find ground distance and ground angle from direction yet
            float groundDistance = 0f;
            float groundAngleFromDirection = 0f;
            verticalVelocity = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            inputMagnitude = 0f;
            float stopMoveWeight = 0f;

            var eulerDifference = characterEntity.EntityTransform.eulerAngles - lastCharacterAngle;
            var magnitude = (eulerDifference.NormalizeAngle().y / (isStrafing ? strafeRotationSpeed : freeRotationSpeed));
            rotationMagnitude = magnitude;
            lastCharacterAngle = characterEntity.EntityTransform.eulerAngles;

            if (MovementState.Has(MovementState.Forward))
            {
                verticalSpeed = 1f;
                inputMagnitude = 1f;
            }
            if (MovementState.Has(MovementState.Backward))
            {
                verticalSpeed = -1f;
                inputMagnitude = 1f;
            }
            if (MovementState.Has(MovementState.Right))
            {
                horizontalSpeed = 1f;
                inputMagnitude = 1f;
            }
            if (MovementState.Has(MovementState.Left))
            {
                horizontalSpeed = -1f;
                inputMagnitude = 1f;
            }

            if (jumpFallen && isJumping)
            {
                isJumping = false;
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
            else if (!MovementState.Has(MovementState.IsGrounded))
            {
                // Falling
                verticalVelocity = 10f;
            }

            if (!jumpFallen && MovementState.Has(MovementState.IsGrounded))
                jumpFallen = true;

            if (MovementState.Has(MovementState.IsGrounded) && ExtraMovementState == ExtraMovementState.IsSprinting)
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

        public void UpdateArmsIK(bool isUsingLeftHand = false)
        {
            // create left arm ik solver if equal null
            if (LeftIK == null || !LeftIK.isValidBones) LeftIK = new vIKSolver(animator, AvatarIKGoal.LeftHand);
            if (RightIK == null || !RightIK.isValidBones) RightIK = new vIKSolver(animator, AvatarIKGoal.RightHand);
            vIKSolver targetIK = null;
            BaseEquipmentEntity equipmentEntity;
            if (isUsingLeftHand)
            {
                targetIK = RightIK;
                equipmentEntity = CacheLeftHandEquipmentEntity;
            }
            else
            {
                targetIK = LeftIK;
                equipmentEntity = CacheRightHandEquipmentEntity;
            }

            if ((!equipmentEntity || !useLeftIK || IsIgnoreIK || isEquipping) ||
                (IsAnimatorTag("Shot Fire") && equipmentEntity.disableIkOnShot))
            {
                if (supportIKWeight > 0)
                {
                    supportIKWeight = 0;
                    targetIK.SetIKWeight(0);
                }
                return;
            }

            bool useIkConditions = false;
            if (!isAiming && !isAttacking)
            {
                if (inputMagnitude < 1f)
                    useIkConditions = equipmentEntity.useIkOnIdle;
                else if (isStrafing)
                    useIkConditions = equipmentEntity.useIkOnStrafe;
                else
                    useIkConditions = equipmentEntity.useIkOnFree;
            }
            else if (isAiming && !isAttacking) useIkConditions = equipmentEntity.useIKOnAiming;
            else if (isAttacking) useIkConditions = equipmentEntity.useIkAttacking;

            if (targetIK != null)
            {
                // control weight of ik
                if (equipmentEntity && equipmentEntity.handIKTarget && !isReloading && (isGrounded || isAiming) && useIkConditions)
                    supportIKWeight = Mathf.Lerp(supportIKWeight, 1, armIKSmoothIn * Time.deltaTime);
                else
                    supportIKWeight = Mathf.Lerp(supportIKWeight, 0, armIKSmoothOut * Time.deltaTime);

                if (supportIKWeight <= 0) return;

                // update IK
                targetIK.SetIKWeight(supportIKWeight);
                if (equipmentEntity && equipmentEntity.handIKTarget)
                {
                    var _offset = (equipmentEntity.handIKTarget.forward * ikPositionOffset.z) + (equipmentEntity.handIKTarget.right * ikPositionOffset.x) + (equipmentEntity.handIKTarget.up * ikPositionOffset.y);
                    targetIK.SetIKPosition(equipmentEntity.handIKTarget.position + _offset);
                    var _rotation = Quaternion.Euler(ikRotationOffset);
                    targetIK.SetIKRotation(equipmentEntity.handIKTarget.rotation * _rotation);
                }
            }
        }

        protected virtual bool CanRotateAimArm()
        {
            return /*IsAnimatorTag("Upperbody Pose") && */IsAimAlignWithForward();
        }


        protected virtual bool IsAimAlignWithForward()
        {
            if (characterEntity.AimPosition.type != AimPositionType.Direction) return false;
            var dir = targetArmAligmentDirection;
            dir.Normalize();
            dir.y = 0;
            var angle = Quaternion.LookRotation(dir.normalized, Vector3.up).eulerAngles - characterEntity.EntityTransform.eulerAngles;

            return ((angle.NormalizeAngle().y < 15 && angle.NormalizeAngle().y > -15));
        }

        protected virtual Vector3 targetArmAlignmentPosition
        {
            get
            {
                return characterEntity.AimPosition.position + ((Vector3)characterEntity.AimPosition.direction * 10f);
            }
        }

        protected virtual Vector3 targetArmAligmentDirection
        {
            get
            {
                return characterEntity.AimPosition.direction;
            }
        }

        protected virtual void RotateAimArm(bool isUsingLeftHand = false)
        {
            BaseEquipmentEntity equipmentEntity;
            if (isUsingLeftHand)
            {
                equipmentEntity = CacheLeftHandEquipmentEntity;
            }
            else
            {
                equipmentEntity = CacheRightHandEquipmentEntity;
            }

            armAlignmentWeight = isAiming && /*aimConditions &&*/ CanRotateAimArm() ? Mathf.Lerp(armAlignmentWeight, Mathf.Clamp(upperBodyInfo.normalizedTime, 0, 1f), smoothArmAlignWeight * Time.deltaTime) : 0;
            if (equipmentEntity && armAlignmentWeight > 0.01f && equipmentEntity.alignRightUpperArmToAim)
            {
                var aimPoint = targetArmAlignmentPosition;
                Vector3 v = aimPoint - equipmentEntity.missileDamageTransform.position;
                var orientation = equipmentEntity.missileDamageTransform.forward;

                var upperArm = isUsingLeftHand ? leftUpperArm : rightUpperArm;
                var rot = Quaternion.FromToRotation(upperArm.InverseTransformDirection(orientation), upperArm.InverseTransformDirection(v));

                if ((!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z)))
                    upperArmRotationAlignment = isShooting ? upperArmRotation : rot;

                var angle = Vector3.Angle(aimPoint - aimAngleReference.transform.position, aimAngleReference.transform.forward);
                upperArmRotation = Quaternion.Lerp(upperArmRotation, upperArmRotationAlignment, smoothArmIKRotation * (.001f + Time.deltaTime));

                if (!float.IsNaN(upperArmRotation.x) && !float.IsNaN(upperArmRotation.y) && !float.IsNaN(upperArmRotation.z))
                {
                    var armWeight = equipmentEntity.alignRightHandToAim ? Mathf.Clamp(armAlignmentWeight, 0, 0.5f) : armAlignmentWeight;
                    upperArm.localRotation *= Quaternion.Euler(upperArmRotation.eulerAngles.NormalizeAngle() * armWeight);
                }
            }
            else
            {
                upperArmRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        protected virtual void RotateAimHand(bool isUsingLeftHand = false)
        {
            BaseEquipmentEntity equipmentEntity;
            if (isUsingLeftHand)
            {
                equipmentEntity = CacheLeftHandEquipmentEntity;
            }
            else
            {
                equipmentEntity = CacheRightHandEquipmentEntity;
            }
            if (equipmentEntity && armAlignmentWeight > 0.01f && /*aimConditions &&*/ equipmentEntity.alignRightHandToAim)
            {
                var aimPoint = targetArmAlignmentPosition;
                Vector3 v = aimPoint - equipmentEntity.missileDamageTransform.position;
                var orientation = equipmentEntity.missileDamageTransform.forward;

                var hand = isUsingLeftHand ? leftHand : rightHand;
                var rot = Quaternion.FromToRotation(hand.InverseTransformDirection(orientation), hand.InverseTransformDirection(v));

                if ((!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z)))
                    handRotationAlignment = isShooting ? handRotation : rot;

                var angle = Vector3.Angle(aimPoint - aimAngleReference.transform.position, aimAngleReference.transform.forward);
                handRotation = Quaternion.Lerp(handRotation, handRotationAlignment, smoothArmIKRotation * (.001f + Time.deltaTime));

                if (!float.IsNaN(handRotation.x) && !float.IsNaN(handRotation.y) && !float.IsNaN(handRotation.z))
                {
                    var armWeight = armAlignmentWeight;
                    hand.localRotation *= Quaternion.Euler(handRotation.eulerAngles.NormalizeAngle() * armWeight);
                }
            }
            else
            {
                handRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        protected virtual void UpdateHeadTrack()
        {
            if (!headTrack)
                return;
            headTrack.ignoreSmooth = isAiming;
            if (characterEntity.AimPosition.type != AimPositionType.Direction)
            {
                headTrack.SetLookAtPosition(targetArmAlignmentPosition, 0, 0);
            }
            else
            {
                var dir = targetArmAligmentDirection;
                dir.Normalize();
                dir.y = 0;
                var angle = Quaternion.LookRotation(dir.normalized, Vector3.up).eulerAngles - characterEntity.EntityTransform.eulerAngles;
                float weight = (180f - Mathf.Abs(angle.NormalizeAngle().y)) / 180f;
                headTrack.SetLookAtPosition(targetArmAlignmentPosition, weight, weight);
            }
        }
    }
}
