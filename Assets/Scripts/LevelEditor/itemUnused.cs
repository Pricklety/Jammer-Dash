using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JammerDash.Editor.Basics
{

    public class itemUnused : MonoBehaviour
    {
        public bool IsLongCube;
        public float longCubeLength;
        void Start()
        {
            if (gameObject.name.Contains("hitter02"))
            {
                IsLongCube = true;
                longCubeLength = GetComponent<SpriteRenderer>().size.x;
            }
            else
            {
                IsLongCube = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 2);
        }
    }

}