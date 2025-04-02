using JammerDash.Tech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            if (SceneManager.GetActiveScene().name == "LevelDefault")
            {

                if (gameObject.name.Contains("hitter"))
                {
                    transform.localScale = new Vector2(CustomLevelDataManager.Instance.data.boxSize, CustomLevelDataManager.Instance.data.boxSize);
                }
                
            }
        }

    }

}