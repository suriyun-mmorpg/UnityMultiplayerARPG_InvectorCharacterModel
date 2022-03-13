using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InvectorInput : vInput
    {
        private void OnGUI()
        {
            // Override default one to do nothing
        }

        private bool isJoystickInput()
        {
            return false;
        }
    }
}
