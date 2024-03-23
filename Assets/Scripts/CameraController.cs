using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        if (transform.position.x > 20000)
        {
            transform.position = new Vector3(19999.9f, transform.position.y, transform.position.z);
            moveSpeed = 0;
        }
        bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();
        bool isMovingWithMouse = false;
        if (Input.GetMouseButton(2) && !pointerOverUI)
        {
            isMovingWithMouse = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isMovingWithMouse = false;
        }

        if (isMovingWithMouse)
        {
            float horizontalInput = Input.GetAxisRaw("Mouse X") * 0.03025f; // Multiply by sensitivity and deltaTime
            Vector3 newPosition = transform.position + new Vector3(horizontalInput, 0, 0);
            transform.position = newPosition;

            // Debugging: Print out raw mouse input
            Debug.Log("Horizontal Input: " + horizontalInput);
        }

        // Display mouse position
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        text.text = "X: " + mousePos.x.ToString("0.0") + ", Y:" + mousePos.y.ToString("0");
    }
}
