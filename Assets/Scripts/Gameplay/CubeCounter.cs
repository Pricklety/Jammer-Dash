using UnityEngine;

public class CubeCounter : MonoBehaviour
{
    public int destroyedCubes;
    public int maxScore;
    public int score;
    public int accCount;
    public double destructionPercentage;
    public GameObject[] cubes;

    void Start()
    {
        cubes = GameObject.FindGameObjectsWithTag("Cubes");
        maxScore = cubes.Length * 50;
    }

    void FixedUpdate()
    {
        float destruction = ((float)accCount / FindObjectOfType<PlayerMovement>().Total) * 100;
        // Call GetTier and print the returned tier
        GetTier(destruction);

        destructionPercentage = destruction;
    }

    public string GetTier(float destructionPercentage)
    {
        float maxScore = 100;
        if (destructionPercentage > maxScore)
        {
            return "Invalid";
        }
        if (destructionPercentage == maxScore)
        {
            return "S+";
        }
        else if (destructionPercentage >= maxScore * 0.95f)
        {
            return "S";
        }
        else if (destructionPercentage >= maxScore * 0.92f)
        {
            return "A";
        }
        else if (destructionPercentage >= maxScore * 0.88f)
        {
            return "B";
        }
        else if (destructionPercentage >= maxScore * 0.84f)
        {
            return "C";
        }
        else if (destructionPercentage >= maxScore * 0.8f)
        {
            return "D";
        }
        else if (destructionPercentage >= maxScore * 0.20f)
        {
            return "F";
        }
        else
        {
            return "F-";
        }
    }
}