using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class storyHandler : MonoBehaviour
{
    [Header("Items")]
    public GameObject player;
    public GameObject portal;

    [Header("Levels")]
    public GameObject Oneup;
    public GameObject FlowerBazooka;

    [Header("Walls")]
    public GameObject l1wall;
    public GameObject l2wall;
    public GameObject l3wall;
    public GameObject l4wall;
    public GameObject l5wall;
    public GameObject l6wall;
    public GameObject l7wall;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("1up")) {
            PlayerPrefs.GetString("1up", "Lvk&(g};rUw'$+GL26/ySM6[3[B=JH");
            player.transform.position = Oneup.transform.position;
            Destroy(l1wall);
        }

        if (PlayerPrefs.HasKey("FlowerBazooka"))
        {
            PlayerPrefs.GetString("FlowerBazooka", "wXDd>3gJ&BJH<x+@`ps|F?2`u+>3rX");
            player.transform.position = FlowerBazooka.transform.position;
            Destroy(l2wall);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ladder")
        {
            if (player.transform.position.x == collision.transform.position.x)
            {
                Vector2 point = new Vector2(collision.transform.position.x + 0.0065f, collision.transform.position.y + 0.634f);
                player.transform.position = Vector2.Lerp(player.transform.position, point, 1);
            }
            
        }
    }
}
