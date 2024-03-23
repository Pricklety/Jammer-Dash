using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    public GameObject cubePrefab; // Prefab of the cube to spawn
    private Transform playerTransform; // Reference to the player's transform

    public Transform cam;
    public float moveSpeed = 7f;

    private void Start()
    {
        playerTransform = transform; // Assuming the script is attached to the player object
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 spawnPosition = playerTransform.position;
            SpawnCube(spawnPosition);
        }

        // Move player right
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        cam.transform.position = new Vector3(transform.position.x + 6, 0.7f, -10);

        if (Input.GetKeyDown(KeyCode.W) && transform.position.y < 3f)
        {
            transform.position += new Vector3(0f, 1f, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && transform.position.y < 3f - 1f)
        {
            transform.position += new Vector3(0f, 1f * 2f, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.S) && transform.position.y > -1)
        {
            transform.position -= new Vector3(0f, 1f, 0f);
        }
    }

    void SpawnCube(Vector3 spawnPosition)
    {
        Instantiate(cubePrefab, spawnPosition, Quaternion.identity);
    }
}
