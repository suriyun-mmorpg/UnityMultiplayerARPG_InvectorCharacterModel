using UnityEngine;

namespace MultiplayerARPG
{
    public partial class WeaponType
    {
        public enum EInvectorWeaponType
        {
            Sword,
            TwoHandSword,
            DualSword,
            Pistol,
            Rifle,
            Shotgun,
            Sniper,
            Rpg,
            Bow,
            Unarmed = 254,
        }

        [Category("Weapon Type Settings")]
        [Header("Invector")]
        [SerializeField]
        private EInvectorWeaponType invectorWeaponType;
        public EInvectorWeaponType InvectorWeaponType
        {
            get { return invectorWeaponType; }
        }
    }
}
