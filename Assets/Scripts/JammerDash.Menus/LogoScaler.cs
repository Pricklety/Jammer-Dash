using UnityEngine;
using UnityEngine.EventSystems;

public class LogoScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 targetScale;
    private Vector3 originalScale;
    private float lerpSpeed = 5f;

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
        targetScale = originalScale * 1.2f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
