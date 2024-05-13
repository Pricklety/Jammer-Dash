using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JammerDash.Unused
{
    public class smallButtonPanels : MonoBehaviour
    {
        public GameObject panel;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ShowButton()
        {
            panel.SetActive(true);
        }

        public void HideButton()
        {
            panel.SetActive(false);
        }

    }


}