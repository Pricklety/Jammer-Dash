using UnityEngine;
using UnityEngine.UI;

public class CursorTrail : MonoBehaviour
{
    public Color trailColor = new Color(1, 0.996f, 0.682f);
    public float distanceFromCamera = 5;

    Transform canvasTransform; // Reference to the canvas transform
    public Image trailImage; // Reference to the trail image component
    Camera mainCamera; // Reference to the main camera
    bool hasClicked = false;
    void Start()
    {

        // Find the canvas in the hierarchy
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvasTransform = canvas.transform;

            // Create the trail image GameObject
            trailImage = GameObject.Find("Mouse Trail").GetComponent<Image>();

            trailImage.color = trailColor;

        }
        if (Input.touches.Length < 1)
        {

            mainCamera = Camera.main; Vector3 screenPosition = Input.mousePosition;
            screenPosition.z = distanceFromCamera;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            trailImage.rectTransform.position = screenPosition;
        }
    }

    void Update()
    {
        SettingsData data = SettingsFileHandler.LoadSettingsFromFile();
        if (Input.GetKey(KeyCode.B) && !Input.GetKeyUp(KeyCode.B)) 
        {
            trailImage.GetComponentInChildren<ParticleSystem>().startLifetime = 5f;
            trailImage.GetComponentInChildren<ParticleSystem>().maxParticles = 10000;

            trailImage.GetComponentInChildren<ParticleSystem>().emissionRate = 360;
        }
        else
        {
            
            trailImage.GetComponentInChildren<ParticleSystem>().startLifetime = data.cursorFade;

            trailImage.GetComponentInChildren<ParticleSystem>().maxParticles = 2000; 

            trailImage.GetComponentInChildren<ParticleSystem>().emissionRate = data.mouseParticles;
        }
        mainCamera = Camera.main;
        // Move the trail object to the cursor position 
       
        if (Input.touches.Length > 0)
        {
            // Get the first touch
            Touch touch = Input.GetTouch(0);

            // Check if the touch phase is moved or stationary
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // Set the position of the cursor object
                trailImage.rectTransform.position = touch.position;
                trailImage.rectTransform.sizeDelta = new Vector2(20, 20);
                hasClicked = true; // Set the flag to true once a touch occurs
            }
        }
        else
        {
            // Check if the player has ever clicked
            if (!hasClicked)
            {
                if (canvasTransform != null && mainCamera != null)
                {
                    if (Input.touches.Length < 1)
                    {
                        Vector3 screenPosition = Input.mousePosition;
                        screenPosition.z = distanceFromCamera;
                        trailImage.rectTransform.position = screenPosition;
                        trailImage.rectTransform.sizeDelta = Vector2.zero;
                        trailImage.GetComponentInChildren<ParticleSystem>().transform.position = screenPosition;


                    }
                }
            }
            else
            {
                // Reset cursor when no touch
                trailImage.rectTransform.localEulerAngles = Vector3.zero;
            }
        }
        if (Input.touchCount == 0)
        {
            
            if ((Input.GetAxisRaw("Mouse X") == 0 && Input.GetAxisRaw("Mouse Y") == 0))
            {
                trailImage.GetComponentInChildren<ParticleSystem>().emissionRate = 0;
            }
            
        }
        else
        {
            trailImage.GetComponentInChildren<ParticleSystem>().emissionRate = 10000;
        }
        

    }
}
