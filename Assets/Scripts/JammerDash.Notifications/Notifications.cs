using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JammerDash.Notifications
{
    public class Notifications : MonoBehaviour
    {
        public GameObject panel;
        public Text main;
        public Button action;


        public static Notifications instance;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        public void Notify(string message, UnityAction buttonEvent)
        {
            main.text = $"{message}";
            
            panel.GetComponent<Animation>().Stop();
            panel.GetComponent<Animation>().Play();                                                                                                                                                                                                                                                                                                                                                                                     
            action.onClick.AddListener(buttonEvent);
            Invoke("End", 6f);
        }

        public void End()
        {
            action.onClick.RemoveAllListeners();
        }
    }
}
