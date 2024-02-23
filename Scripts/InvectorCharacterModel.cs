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

        public BaseCharacterEntity CharacterEntity => Entity as BaseCharacterEntity;
        public bool IsReloading
        {
            get
            {
                return IsAnimatorTag("IsReloading") || CharacterEntity.IsPlayingReloadAnimation();
            }
        }
        public bool IsShooting
        {
            get
            {
                return _shootTimming > 0f;
            }
        }
        public bool IsAiming
        {
            get
            {
                return _aimTimming > 0f;
            }
        }
        public bool IsAttacking
        {
            get
            {
                return IsAnimatorTag("Attack") || CharacterEntity.IsPlayingAttackOrUseSkillAnimation();
            }
        }
        public bool IsEquipping
        {
            get
            {
                return IsAnimatorTag("IsEquipping");
            }
        }
        /// <summary>
        /// Animator Hash for RandomAttack parameter 
        /// </summary>
        public readonly int RandomAttack = Animator.StringToHash("RandomAttack");
        /// <summary>
        /// Animator Hash for IsShoot parameter 
        /// </summary>
        public readonly int IsShoot = Animator.StringToHash("Shoot");
        /// <summary>
        /// Animator Hash for Reload parameter 
        /// </summary>
        public readonly int Reload = Animator.StringToHash("Reload");
        /// <summary>
        /// Animator Hash for ReloadID parameter 
        /// </summary>
        public readonly int ReloadID = Animator.StringToHash("ReloadID");
        /// <summary>
        /// Animator Hash for IsCrawling parameter
        /// </summary>
        public readonly int IsCrawling = Animator.StringToHash("IsCrawling");

        protected float _onlyArmsLayerWeight = 0f;
        protected float _supportIKWeight, _weaponIKWeight;
        protected float _shootTimming = 0f;
        protected float _aimTimming = 0f;
        protected bool _ignoreIK = false;
        protected int _onlyArmsLayer;
        protected int _currentAttackClipIndex = 0;
        protected bool _dirtyIsDead = true;
        protected bool _jumpFallen = true;
        protected bool _isJumping = false;
        protected float _movesetId = 0f;
        protected float _upperBodyId = 0f;
        protected bool _isStrafing = false;
        protected bool _isSprinting = false;
        // NOTE: Actually has no `isSliding` usage
        protected bool _isSliding = false;
        // NOTE: Actually has no `isRolling` usage
        protected bool _isRolling = false;
        protected bool _isCrouching = false;
        protected bool _isCrawling = false;
        protected bool _isGrounded = false;
        protected float _verticalVelocity = 0f;
        protected float _horizontalSpeed = 0f;
        protected float _verticalSpeed = 0f;
        protected float _inputMagnitude = 0f;

        /// <summary>
        /// sets the rotationMagnitude to update the animations in the animator controller
        /// </summary>
        protected float _rotationMagnitude;
        /// <summary>
        /// Last angle of the character used to calculate rotationMagnitude
        /// </summary>
        protected Vector3 _lastCharacterAngle;

        protected AnimatorStateInfo _baseLayerInfo, _underBodyInfo, _rightArmInfo, _leftArmInfo, _fullBodyInfo, _upperBodyInfo;
        protected Transform _leftHand, _rightHand, _rightLowerArm, _leftLowerArm, _rightUpperArm, _leftUpperArm;

        protected float _armAlignmentWeight;
        protected float _aimWeight;

        protected Quaternion _handRotation, _upperArmRotation;
        protected GameObject _aimAngleReference;

        protected Quaternion _upperArmRotationAlignment, _handRotationAlignment;

        public int BaseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        public int UnderBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        public int RightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        public int LeftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        public int UpperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        public int FullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }
        public vIKSolver LeftIK { get; set; }
        public vIKSolver RightIK { get; set; }
        private bool _ignoreIKFromAnimator;
        private bool IsIgnoreIK
        {
            get
            {
                return _ignoreIK || _ignoreIKFromAnimator;
            }
        }

        protected override void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (headTrack == null)
                headTrack = GetComponentInChildren<vHeadTrack>();
            _onlyArmsLayer = animator.GetLayerIndex("OnlyArms");

            _leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            _leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

            _aimAngleReference = new GameObject("aimAngleReference");
            _aimAngleReference.transform.rotation = transform.rotation;
            _aimAngleReference.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Head));
            _aimAngleReference.transform.localPosition = Vector3.zero;

            animatorStateInfos = new vAnimatorStateInfos(animator);
            base.Awake();
        }

        protected virtual void Start()
        {
            vHeadTrackSensor headTrackSensor = GetComponentInChildren<vHeadTrackSensor>();
            if (headTrackSensor != null)
            {
                headTrackSensor.tag = "Untagged";
                headTrackSensor.gameObject.AddComponent<UnHittable>();
            }
        }

        protected virtual void OnEnable()
        {
            animatorStateInfos.RegisterListener();
        }

        protected virtual void OnDisable()
        {
            animatorStateInfos.RemoveListener();
            animator.SetLayerWeight(_onlyArmsLayer, _upperBodyId > 0f ? 1f : 0f);
            animator.SetFloat(vAnimatorParameters.MoveSet_ID, _movesetId);
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, _upperBodyId);
        }

        public virtual bool IsAnimatorTag(string tag)
        {
            if (animator == null) return false;
            if (animatorStateInfos.HasTag(tag))
            {
                return true;
            }
            if (_baseLayerInfo.IsTag(tag)) return true;
            if (_underBodyInfo.IsTag(tag)) return true;
            if (_rightArmInfo.IsTag(tag)) return true;
            if (_leftArmInfo.IsTag(tag)) return true;
            if (_upperBodyInfo.IsTag(tag)) return true;
            if (_fullBodyInfo.IsTag(tag)) return true;
            return false;
        }

        protected void Update()
        {
            _baseLayerInfo = animator.GetCurrentAnimatorStateInfo(BaseLayer);
            _underBodyInfo = animator.GetCurrentAnimatorStateInfo(UnderBodyLayer);
            _rightArmInfo = animator.GetCurrentAnimatorStateInfo(RightArmLayer);
            _leftArmInfo = animator.GetCurrentAnimatorStateInfo(LeftArmLayer);
            _upperBodyInfo = animator.GetCurrentAnimatorStateInfo(UpperBodyLayer);
            _fullBodyInfo = animator.GetCurrentAnimatorStateInfo(FullbodyLayer);

            float deltaTime = Time.deltaTime;
            _onlyArmsLayerWeight = Mathf.Lerp(_onlyArmsLayerWeight, _upperBodyId > 0f ? 1f : 0f, onlyArmsSpeed * deltaTime);
            animator.SetLayerWeight(_onlyArmsLayer, _onlyArmsLayerWeight);
            animator.SetFloat(vAnimatorParameters.MoveSet_ID, _movesetId, .2f, deltaTime);
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, _upperBodyId);
            if (_shootTimming > 0)
            {
                _shootTimming -= deltaTime;
            }
            if (_aimTimming > 0)
            {
                _aimTimming -= deltaTime;
                if (_aimTimming <= 0f)
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
                animationIndex = _currentAttackClipIndex;
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

        public override void PlaySkillCastClip(int dataId, float duration, out bool skipMovementValidation, out bool shouldUseRootMotion)
        {
            // TODO: May implement this?
            throw new System.NotImplementedException();
        }

        public override void StopSkillCastAnimation()
        {
            // TODO: May implement this?
            throw new System.NotImplementedException();
        }

        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand, out bool skipMovementValidation, out bool shouldUseRootMotion)
        {
            // TODO: Implement this, for only throwing item and bow
            throw new System.NotImplementedException();
        }

        public override void StopWeaponChargeAnimation()
        {
            // TODO: Implement this, for only throwing item and bow
            throw new System.NotImplementedException();
        }

        public void PlayAttackAnimation(int dataId, int animationIndex)
        {
            GetAttackAnimation(dataId, animationIndex, out _, out _shootTimming);
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
                    _aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 2);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    _aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 3);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    _aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 4);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    _aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 5);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    _aimTimming = hipfireAimTime;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
                {
                    animator.SetBool(vAnimatorParameters.IsAiming, true);
                    animator.SetFloat(vAnimatorParameters.Shot_ID, 6);
                    animator.ResetTrigger(IsShoot);
                    animator.SetTrigger(IsShoot);
                    _aimTimming = hipfireAimTime;
                }
                else
                {
                    animator.ResetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(vAnimatorParameters.AttackID, 0);
                    animator.SetTrigger(vAnimatorParameters.WeakAttack);
                    animator.SetInteger(RandomAttack, animationIndex);
                }
            }
            _currentAttackClipIndex++;
            ClipLengthData[] tempClipLengths = GetAttackClipLengths(dataId);
            if (_currentAttackClipIndex >= tempClipLengths.Length)
                _currentAttackClipIndex = 0;
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

        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, out bool skipMovementValidation, out bool shouldUseRootMotion, float playSpeedMultiplier = 1)
        {
            skipMovementValidation = false;
            shouldUseRootMotion = false;
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

        public override void SetEquipItems(IList<CharacterItem> equipItems, IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
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
                _upperBodyId = 0;
                _movesetId = 0;
            }
            else
            {
                if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sword)
                {
                    _upperBodyId = 0;
                    _movesetId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.TwoHandSword)
                {
                    _upperBodyId = 0;
                    _movesetId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.DualSword)
                {
                    _upperBodyId = 0;
                    _movesetId = 2;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Pistol)
                {
                    _upperBodyId = 1;
                    _movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rifle)
                {
                    _upperBodyId = 2;
                    _movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Shotgun)
                {
                    _upperBodyId = 3;
                    _movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Sniper)
                {
                    _upperBodyId = 2;
                    _movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Rpg)
                {
                    _upperBodyId = 4;
                    _movesetId = 1;
                }
                else if (GameInstance.WeaponTypes[dataId].InvectorWeaponType == WeaponType.EInvectorWeaponType.Bow)
                {
                    _upperBodyId = 5;
                    _movesetId = 1;
                }
                else
                {
                    _upperBodyId = 0;
                    _movesetId = 0;
                }
            }
            if (!enabled)
            {
                animator.SetLayerWeight(_onlyArmsLayer, _upperBodyId > 0f ? 1f : 0f);
                animator.SetFloat(vAnimatorParameters.MoveSet_ID, _movesetId);
            }
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, _upperBodyId);
            _currentAttackClipIndex = 0;
            base.SetEquipItems(equipItems, selectableWeaponSets, equipWeaponSet, isWeaponsSheathed);
        }

        public override void PlayJumpAnimation()
        {
            _isJumping = true;
        }

        public override void PlayMoveAnimation()
        {
            if (_dirtyIsDead != IsDead)
            {
                _dirtyIsDead = IsDead;
                ResetAnimatorParameters();
                animator.SetBool(vAnimatorParameters.IsDead, IsDead);
            }

            if (IsDead)
            {
                _isJumping = false;
                return;
            }

            float deltaTime = Time.deltaTime;
            _isStrafing = MovementState.Has(MovementState.Left) || MovementState.Has(MovementState.Right) || MovementState.Has(MovementState.Backward);
            _isSprinting = ExtraMovementState == ExtraMovementState.IsSprinting;
            // NOTE: Actually has no `isSliding` usage
            bool isSliding = false;
            // NOTE: Actually has no `isRolling` usage
            bool isRolling = false;
            _isCrouching = ExtraMovementState == ExtraMovementState.IsCrouching;
            _isCrawling = ExtraMovementState == ExtraMovementState.IsCrawling;
            _isGrounded = MovementState.Has(MovementState.IsGrounded);
            // NOTE: Cannot find ground distance and ground angle from direction yet
            float groundDistance = 0f;
            float groundAngleFromDirection = 0f;
            _verticalVelocity = 0f;
            _horizontalSpeed = 0f;
            _verticalSpeed = 0f;
            _inputMagnitude = 0f;
            float stopMoveWeight = 0f;

            Vector3 eulerDifference = Entity.EntityTransform.eulerAngles - _lastCharacterAngle;
            float magnitude = eulerDifference.NormalizeAngle().y / (_isStrafing ? strafeRotationSpeed : freeRotationSpeed);
            _rotationMagnitude = magnitude;
            _lastCharacterAngle = Entity.EntityTransform.eulerAngles;

            if (MovementState.Has(MovementState.Forward))
            {
                _verticalSpeed = 1f;
                _inputMagnitude = 1f;
            }
            if (MovementState.Has(MovementState.Backward))
            {
                _verticalSpeed = -1f;
                _inputMagnitude = 1f;
            }
            if (MovementState.Has(MovementState.Right))
            {
                _horizontalSpeed = 1f;
                _inputMagnitude = 1f;
            }
            if (MovementState.Has(MovementState.Left))
            {
                _horizontalSpeed = -1f;
                _inputMagnitude = 1f;
            }

            if (_jumpFallen && _isJumping)
            {
                _isJumping = false;
                _jumpFallen = false;
                if (_inputMagnitude < 0.1f)
                {
                    animator.CrossFadeInFixedTime("Jump", 0.1f);
                }
                else
                {
                    animator.CrossFadeInFixedTime("JumpMove", .2f);
                }
            }

            if (!_isGrounded)
            {
                // TODO: use madeup vert velocity and ground distance (just to make it work)
                _verticalVelocity = 10f;
                groundDistance = 0.3f;
            }

            if (!_jumpFallen && MovementState.Has(MovementState.IsGrounded))
                _jumpFallen = true;

            if (MovementState.Has(MovementState.IsGrounded) && ExtraMovementState == ExtraMovementState.IsSprinting)
                _inputMagnitude *= 1.5f;

            animator.SetBool(vAnimatorParameters.IsStrafing, _isStrafing);
            animator.SetBool(vAnimatorParameters.IsSprinting, _isSprinting);
            animator.SetBool(vAnimatorParameters.IsSliding, isSliding && !isRolling);
            animator.SetBool(vAnimatorParameters.IsCrouching, _isCrouching);
            animator.SetBool(IsCrawling, _isCrawling);
            animator.SetBool(vAnimatorParameters.IsGrounded, _isGrounded);
            animator.SetFloat(vAnimatorParameters.GroundDistance, groundDistance);
            animator.SetFloat(vAnimatorParameters.GroundAngle, groundAngleFromDirection);
            animator.SetBool(vAnimatorParameters.CanAim, true);

            if (!_isGrounded)
            {
                animator.SetFloat(vAnimatorParameters.VerticalVelocity, _verticalVelocity);
            }

            if (_isStrafing)
            {
                animator.SetFloat(vAnimatorParameters.InputHorizontal, _horizontalSpeed, strafeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputVertical, _verticalSpeed, strafeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(_inputMagnitude, 0f, stopMoveWeight), strafeAnimationSmooth, deltaTime);
            }
            else
            {
                animator.SetFloat(vAnimatorParameters.InputVertical, _verticalSpeed, freeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputHorizontal, 0, freeAnimationSmooth, deltaTime);
                animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(_inputMagnitude, 0f, stopMoveWeight), freeAnimationSmooth, deltaTime);
            }

            if (useLeanMovementAnim && _inputMagnitude > 0.1f)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, _rotationMagnitude, leanSmooth, deltaTime);
            }
            else if (useTurnOnSpotAnim)
            {
                animator.SetFloat(vAnimatorParameters.RotationMagnitude, _rotationMagnitude, _rotationMagnitude == 0 ? 0.1f : 0.01f, deltaTime);
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

            if ((!equipmentEntity || !useLeftIK || IsIgnoreIK || IsEquipping) ||
                (IsAnimatorTag("Shot Fire") && equipmentEntity.disableIkOnShot))
            {
                if (_supportIKWeight > 0)
                {
                    _supportIKWeight = 0;
                    targetIK.SetIKWeight(0);
                }
                return;
            }

            bool useIkConditions = false;
            if (!IsAiming && !IsAttacking)
            {
                if (_inputMagnitude < 1f)
                    useIkConditions = equipmentEntity.useIkOnIdle;
                else if (_isStrafing)
                    useIkConditions = equipmentEntity.useIkOnStrafe;
                else
                    useIkConditions = equipmentEntity.useIkOnFree;
            }
            else if (IsAiming && !IsAttacking) useIkConditions = equipmentEntity.useIKOnAiming;
            else if (IsAttacking) useIkConditions = equipmentEntity.useIkAttacking;

            if (targetIK != null)
            {
                // control weight of ik
                if (equipmentEntity && equipmentEntity.handIKTarget && !IsReloading && (_isGrounded || IsAiming) && useIkConditions)
                    _supportIKWeight = Mathf.Lerp(_supportIKWeight, 1, armIKSmoothIn * Time.deltaTime);
                else
                    _supportIKWeight = Mathf.Lerp(_supportIKWeight, 0, armIKSmoothOut * Time.deltaTime);

                if (_supportIKWeight <= 0) return;

                // update IK
                targetIK.SetIKWeight(_supportIKWeight);
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
            if (CharacterEntity.AimPosition.type != AimPositionType.Direction) return false;
            var dir = targetArmAligmentDirection;
            dir.Normalize();
            dir.y = 0;
            var angle = Quaternion.LookRotation(dir.normalized, Vector3.up).eulerAngles - CharacterEntity.EntityTransform.eulerAngles;

            return angle.NormalizeAngle().y < 15 && angle.NormalizeAngle().y > -15;
        }

        protected virtual Vector3 targetArmAlignmentPosition
        {
            get
            {
                return CharacterEntity.AimPosition.position + ((Vector3)CharacterEntity.AimPosition.direction * 10f);
            }
        }

        protected virtual Vector3 targetArmAligmentDirection
        {
            get
            {
                return CharacterEntity.AimPosition.direction;
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

            _armAlignmentWeight = IsAiming && /*aimConditions &&*/ CanRotateAimArm() ? Mathf.Lerp(_armAlignmentWeight, Mathf.Clamp(_upperBodyInfo.normalizedTime, 0, 1f), smoothArmAlignWeight * Time.deltaTime) : 0;
            if (equipmentEntity && _armAlignmentWeight > 0.01f && equipmentEntity.alignRightUpperArmToAim)
            {
                var aimPoint = targetArmAlignmentPosition;
                Vector3 v = aimPoint - equipmentEntity.missileDamageTransform.position;
                var orientation = equipmentEntity.missileDamageTransform.forward;

                var upperArm = isUsingLeftHand ? _leftUpperArm : _rightUpperArm;
                var rot = Quaternion.FromToRotation(upperArm.InverseTransformDirection(orientation), upperArm.InverseTransformDirection(v));

                if ((!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z)))
                    _upperArmRotationAlignment = IsShooting ? _upperArmRotation : rot;

                var angle = Vector3.Angle(aimPoint - _aimAngleReference.transform.position, _aimAngleReference.transform.forward);
                _upperArmRotation = Quaternion.Lerp(_upperArmRotation, _upperArmRotationAlignment, smoothArmIKRotation * (.001f + Time.deltaTime));

                if (!float.IsNaN(_upperArmRotation.x) && !float.IsNaN(_upperArmRotation.y) && !float.IsNaN(_upperArmRotation.z))
                {
                    var armWeight = equipmentEntity.alignRightHandToAim ? Mathf.Clamp(_armAlignmentWeight, 0, 0.5f) : _armAlignmentWeight;
                    upperArm.localRotation *= Quaternion.Euler(_upperArmRotation.eulerAngles.NormalizeAngle() * armWeight);
                }
            }
            else
            {
                _upperArmRotation = Quaternion.Euler(0, 0, 0);
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
            if (equipmentEntity && _armAlignmentWeight > 0.01f && /*aimConditions &&*/ equipmentEntity.alignRightHandToAim)
            {
                var aimPoint = targetArmAlignmentPosition;
                Vector3 v = aimPoint - equipmentEntity.missileDamageTransform.position;
                var orientation = equipmentEntity.missileDamageTransform.forward;

                var hand = isUsingLeftHand ? _leftHand : _rightHand;
                var rot = Quaternion.FromToRotation(hand.InverseTransformDirection(orientation), hand.InverseTransformDirection(v));

                if ((!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z)))
                    _handRotationAlignment = IsShooting ? _handRotation : rot;

                var angle = Vector3.Angle(aimPoint - _aimAngleReference.transform.position, _aimAngleReference.transform.forward);
                _handRotation = Quaternion.Lerp(_handRotation, _handRotationAlignment, smoothArmIKRotation * (.001f + Time.deltaTime));

                if (!float.IsNaN(_handRotation.x) && !float.IsNaN(_handRotation.y) && !float.IsNaN(_handRotation.z))
                {
                    var armWeight = _armAlignmentWeight;
                    hand.localRotation *= Quaternion.Euler(_handRotation.eulerAngles.NormalizeAngle() * armWeight);
                }
            }
            else
            {
                _handRotation = Quaternion.Euler(0, 0, 0);
            }
        }

        protected virtual void UpdateHeadTrack()
        {
            if (!headTrack)
                return;
            headTrack.ignoreSmooth = IsAiming;
            if (CharacterEntity.AimPosition.type != AimPositionType.Direction)
            {
                headTrack.SetLookAtPosition(targetArmAlignmentPosition, 0, 0);
            }
            else
            {
                var dir = targetArmAligmentDirection;
                dir.Normalize();
                dir.y = 0;
                var angle = Quaternion.LookRotation(dir.normalized, Vector3.up).eulerAngles - CharacterEntity.EntityTransform.eulerAngles;
                float weight = (180f - Mathf.Abs(angle.NormalizeAngle().y)) / 180f;
                headTrack.SetLookAtPosition(targetArmAlignmentPosition, weight, weight);
            }
        }
    }
}
