using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using Unity.Profiling;
using System.Security.Cryptography;

public class DebugMode : MonoBehaviour
{
    public GameObject panel;

    private void Update()
    {
        
        if (Input.GetKey(KeyCode.F4))
        {
            panel.SetActive(true);
        }
        else
        {
            panel.SetActive(false);
        }
    }
}
