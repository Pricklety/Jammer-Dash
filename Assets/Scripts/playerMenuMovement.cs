using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class playerMenuMovement : MonoBehaviour
{
    public float normalSpeed = 5f;
    public float slowSpeedMultiplier = 0.5f;
    public float fastSpeedMultiplier = 1.5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Animator animator;
    public Camera mainCamera;
    public float cameraFocusLerpSpeed = 5f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isFacingRight = true;
    private PostProcessVolume postProcessVolume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        postProcessVolume = mainCamera.GetComponent<PostProcessVolume>();
        postProcessVolume.profile.TryGetSettings(out vignette);
        postProcessVolume.profile.TryGetSettings(out chromaticAberration);
    }

    void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Player movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float currentSpeed = GetSpeed(horizontalInput);
        Move(horizontalInput, currentSpeed);

        // Player jump
        if (isGrounded && Input.GetButton("Jump"))
        {
            Jump();
        }

        // Adjust camera effects based on player speed
        AdjustCameraEffects(currentSpeed);
    }

    void Move(float horizontalInput, float currentSpeed)
    {
        Vector2 moveVelocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);
        rb.velocity = moveVelocity;

        // Flip the character if moving in the opposite direction
        if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
        {
            Flip();
        }

        if (horizontalInput != 0)
        {
            // Update animator parameters with Lerp for smooth transition
            float animationSpeed = Mathf.Lerp(animator.GetFloat("Speed"), Mathf.Abs(horizontalInput), Time.deltaTime * 5f);
            animator.SetFloat("Speed", animationSpeed);
        }
        else
        {
            // Player is not moving, stop the walk animation
            float animationSpeed = Mathf.Lerp(animator.GetFloat("Speed"), 0f, Time.deltaTime * 5f);
            animator.SetFloat("Speed", animationSpeed);
        }
    }

    float GetSpeed(float horizontalInput)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Slower walk when holding Shift
            return normalSpeed * slowSpeedMultiplier;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            // Faster walk when holding Control
            return normalSpeed * fastSpeedMultiplier;
        }
        else
        {
            return normalSpeed;
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void AdjustCameraEffects(float currentSpeed)
    {
        if (currentSpeed < normalSpeed)
        {
            // Zoom in the camera
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, 3f, Time.deltaTime * 5f);
            // Adjust vignette and chromatic aberration values
            vignette.intensity.value = 0.66f;
            chromaticAberration.intensity.value = 0.35f;
        }
        else
        {
            // Return to normal zoom
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, 5f, Time.deltaTime * 5f);
            // Return vignette and chromatic aberration to normal values
            vignette.intensity.value = 0.466f;
            chromaticAberration.intensity.value = 0f;
        }

        // Focus the camera on the player
        Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * cameraFocusLerpSpeed);
    }
}
