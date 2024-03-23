using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class storyBegin : MonoBehaviour
{
    public Text no;

    public Text yes;


    public void Yes()
    {
        yes.text = "Good. Get ready for level one";
        StartCoroutine(Load());
    }

    public void No()
    {
        no.text = "No is not an option.";
        StartCoroutine(Repeat());
    }

    IEnumerator Repeat()
    {
        yield return new WaitForSecondsRealtime(2);
        no.text = "Do you accept?";
    }

    IEnumerator Load()
    {
        yield return new WaitForSecondsRealtime(3);
        SceneManager.LoadScene("storySelector");
        PlayerPrefs.SetString("Beginning", "itbegan");
    }
}
