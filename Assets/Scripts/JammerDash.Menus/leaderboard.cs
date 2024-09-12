using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using JammerDash.Menus.Play.Score;

namespace JammerDash.Menus.Play
{
    public class leaderboard : MonoBehaviour
    {
        public GameObject panelPrefab;
        public Transform panelContainer;

        void Start()
        {
            panelContainer = GameObject.Find("lb content").transform;
        }

    }
}
