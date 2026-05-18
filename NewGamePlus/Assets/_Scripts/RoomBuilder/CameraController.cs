using UnityEngine;
using UnityEngine.InputSystem;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// Pans the camera with arrow keys or WASD at a fixed speed.
    /// Attach to the Main Camera.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        private const float DefaultPanSpeed = 5f;

        [SerializeField] private float _panSpeed = DefaultPanSpeed;

        private void Update()
        {
            Vector2 direction = Vector2.zero;

            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)  direction.x -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) direction.x += 1f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed)    direction.y += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed)  direction.y -= 1f;

            if (direction != Vector2.zero)
                transform.Translate(direction.normalized * (_panSpeed * Time.deltaTime), Space.World);
        }
    }
}
