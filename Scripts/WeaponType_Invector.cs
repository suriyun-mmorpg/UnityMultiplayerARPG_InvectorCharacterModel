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
        }

        [Header("Invector")]
        private EInvectorWeaponType invectorWeaponType;
        public EInvectorWeaponType InvectorWeaponType
        {
            get { return invectorWeaponType; }
        }
    }
}
