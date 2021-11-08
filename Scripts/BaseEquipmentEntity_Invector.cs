using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseEquipmentEntity
    {
        [Header("IK Options")]
        [Tooltip("IK will help the right hand to align where you actually is aiming")]
        public bool alignRightHandToAim = true;
        [Tooltip("IK will help the right hand to align where you actually is aiming")]
        public bool alignRightUpperArmToAim = true;
        public bool raycastAimTarget = true;
        [Tooltip("Left IK on Idle")]
        public bool useIkOnIdle = true;
        [Tooltip("Left IK on free locomotion")]
        public bool useIkOnFree = true;
        [Tooltip("Left IK on strafe locomotion")]
        public bool useIkOnStrafe = true;
        [Tooltip("Left IK while attacking")]
        public bool useIkAttacking = false;
        public bool disableIkOnShot = false;
        public bool useIKOnAiming = true;
        public Transform handIKTarget;
    }
}
