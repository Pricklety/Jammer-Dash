using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JammerDash.Editor
{
    public class ItemController : MonoBehaviour
    {
        public int ID;
        public bool clicked = false;
        private EditorManager editor;
        // Start is called before the first frame update
        public void Start()
        {
            editor = GameObject.FindGameObjectWithTag("EditorManager").GetComponent<EditorManager>();
        }


        // Update is called once per frame
        public void ButtonClicked()
        {
            if (clicked)
            {
                GetComponent<Button>().OnDeselect(null);
            }
            clicked = true;
            editor.currentButtonPressed = ID;
        }
    }
}
