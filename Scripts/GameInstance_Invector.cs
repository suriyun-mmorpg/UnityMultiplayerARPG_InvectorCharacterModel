using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Invector")]
        public WeaponType[] invectorSwordWeaponTypes;
        public WeaponType[] invectorTwoHandSwordWeaponTypes;
        public WeaponType[] invectorDualSwordWeaponTypes;
        public WeaponType[] invectorPistolWeaponTypes;
        public WeaponType[] invectorRifleWeaponTypes;
        public WeaponType[] invectorShotgunWeaponTypes;
        public WeaponType[] invectorSniperWeaponTypes;
        public WeaponType[] invectorRpgWeaponTypes;
        public WeaponType[] invectorBowWeaponTypes;

        /// <summary>
        /// Attack ID = 1
        /// </summary>
        public HashSet<int> InvectorSwordWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Attack ID = 4
        /// </summary>
        public HashSet<int> InvectorTwoHandSwordWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Attack ID = 5
        /// </summary>
        public HashSet<int> InvectorDualSwordWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Reload ID = 1
        /// Aiming ID = 1
        /// Shot ID = 1
        /// </summary>
        public HashSet<int> InvectorPistolWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Reload ID = 2
        /// Aiming ID = 2
        /// Shot ID = 2
        /// </summary>
        public HashSet<int> InvectorRifleWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Reload ID = 3
        /// Aiming ID = 3
        /// Shot ID = 3
        /// </summary>
        public HashSet<int> InvectorShotgunWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Reload ID = 2
        /// Aiming ID = 2
        /// Shot ID = 4
        /// </summary>
        public HashSet<int> InvectorSniperWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Reload ID = 4
        /// Aiming ID = 4
        /// Shot ID = 5
        /// </summary>
        public HashSet<int> InvectorRpgWeaponTypes { get; } = new HashSet<int>();
        /// <summary>
        /// Reload ID = 5
        /// Aiming ID = 5
        /// Shot ID = 6
        /// </summary>
        public HashSet<int> InvectorBowWeaponTypes { get; } = new HashSet<int>();

        [DevExtMethods("Awake")]
        protected void Awake_Invector()
        {
            // Fill data ID hash set
            foreach (WeaponType type in invectorSwordWeaponTypes)
            {
                InvectorSwordWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorTwoHandSwordWeaponTypes)
            {
                InvectorTwoHandSwordWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorDualSwordWeaponTypes)
            {
                InvectorDualSwordWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorPistolWeaponTypes)
            {
                InvectorPistolWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorRifleWeaponTypes)
            {
                InvectorRifleWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorShotgunWeaponTypes)
            {
                InvectorShotgunWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorSniperWeaponTypes)
            {
                InvectorSniperWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorRpgWeaponTypes)
            {
                InvectorRpgWeaponTypes.Add(type.DataId);
            }
            foreach (WeaponType type in invectorBowWeaponTypes)
            {
                InvectorBowWeaponTypes.Add(type.DataId);
            }
        }
    }
}
