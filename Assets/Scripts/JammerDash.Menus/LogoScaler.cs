using UnityEngine;
using UnityEngine.EventSystems;

namespace JammerDash.Menus.Main
{
    public class LogoScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Vector3 targetScale;
        private Vector3 originalScale;
        private float lerpSpeed = 5f;
        public bool isOverLogo;

        void Start()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
        }

        void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isOverLogo = true;
            targetScale = originalScale * 1.2f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isOverLogo = false;
            targetScale = originalScale;
        }
    }
}
