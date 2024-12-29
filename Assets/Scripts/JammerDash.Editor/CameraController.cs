using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JammerDash.Editor
{
    public class CameraController : MonoBehaviour
    {
        public float moveSpeed = 7;

        public Text text;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {

            if (transform.position.x < 7f)
            {
                transform.position = new Vector3(7.1f, transform.position.y, transform.position.z);
                moveSpeed = 0;
                text.text = "You can't go there!";
            }
            else
            {
                moveSpeed = Input.GetAxisRaw("Mouse X");
            }

            bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();
            bool isMovingWithMouse = false;
            if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeybindingManager.moveCam) && !pointerOverUI)
            {
                isMovingWithMouse = true;
            }

            if (Input.GetKeyUp(KeybindingManager.moveCam))
            {
                isMovingWithMouse = false;
            }

            if (isMovingWithMouse)
            {
                float horizontalInput = Input.GetAxisRaw("Mouse X") * 0.03025f; // Multiply by sensitivity and deltaTime
                Vector3 newPosition = transform.position + new Vector3(horizontalInput, 0, 0);
                transform.position = newPosition;

            }

            // Display mouse position
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            text.text = "X: " + mousePos.x.ToString("0.0") + ", Y:" + mousePos.y.ToString("0");
        }
    }
}