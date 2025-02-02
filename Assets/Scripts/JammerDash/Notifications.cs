using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JammerDash
{
    public class Notifications : MonoBehaviour
    {
        public GameObject panelPrefab;
        public Transform parentTransform;
        public Transform parentTransform2;

        public static Notifications instance;

        private float panelHeightOffset = 70f; 

        public GameObject notificationsList;

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

        public void Update() {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab)) {
                NotificationsList();
            }
        }
        public void NotificationsList()
        {
            notificationsList.SetActive(!notificationsList.activeSelf);
        }
        public void Notify(string message, UnityAction buttonEvent)
        {
            // Calculate the new position for the panel
            float highestYPosition = -70f;
            foreach (Transform child in parentTransform)
            {
                highestYPosition = Mathf.Max(highestYPosition, child.localPosition.y);
            }
            Vector3 spawnPosition = new Vector3(0, highestYPosition + panelHeightOffset, 0);

            // Instantiate a new panel
            GameObject panelInstance = Instantiate(panelPrefab, parentTransform);
            GameObject panelInstancePerm = Instantiate(panelPrefab, parentTransform2);
            panelPrefab.GetComponent<Animation>().Play();
            panelInstance.transform.localPosition = spawnPosition; // Set the target position

            // Set the message text
            Text main = panelInstance.GetComponentInChildren<Text>();
            Text main2 = panelInstancePerm.GetComponentInChildren<Text>();
            if (main != null)
            {
                main.text = message;
            }
            if (main2 != null) {
                main2.text = message;
            }

            // Add the button event
            Button actionButton = panelInstance.GetComponentInChildren<Button>();
            Button actionButton2 = panelInstancePerm.GetComponentInChildren<Button>();
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(buttonEvent);
            }
            if (actionButton2 != null) {
                actionButton2.onClick.AddListener(buttonEvent);
            }

                StartCoroutine(WaitForAnimationToFinishAndDestroy(panelInstance));
            
        }

        private System.Collections.IEnumerator WaitForAnimationToFinishAndDestroy(GameObject panelInstance)
        {
            yield return new WaitForSeconds(5f);

            Button actionButton = panelInstance.GetComponentInChildren<Button>();
            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
            }

            Destroy(panelInstance);
        }
    }
}
