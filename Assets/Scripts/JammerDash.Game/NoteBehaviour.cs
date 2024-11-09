using System.Collections;
using UnityEngine;

namespace JammerDash.Game.Note
{
    public class NoteBehaviour : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(Death());
        }

        IEnumerator Death()
        {
            yield return new WaitForSecondsRealtime(1f);
            Destroy(gameObject);
        }
    }

}