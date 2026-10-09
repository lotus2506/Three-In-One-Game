using UnityEngine.InputSystem;

namespace FinalGame
{
    // Keeps the course scripts working with this project's new Input System.
    public static class GameInput
    {
        public static bool EscapePressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        public static bool SpacePressed => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        public static float GetAxis(string axis)
        {
            var k = Keyboard.current;
            if (k == null) return 0;
            if (axis == "Horizontal") return (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            if (axis == "Vertical") return (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            return 0;
        }
    }
}
