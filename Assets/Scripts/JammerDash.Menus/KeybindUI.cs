using System;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingUI : MonoBehaviour
{
    public KeybindingManager keybindingManager;

    public Text upBindingText;
    public Text downBindingText;
    public Text groundBindingText;
    public Text boostBindingText;
    public Text key1BindingText;
    public Text key2BindingText;

    private void Start()
    {
        KeybindingManager.instance.LoadKeybindingsFromJson();
        UpdateUI();

    }

    public void RebindUp()
    {
        StartCoroutine(WaitForKeyPress("up"));
    }

    public void RebindDown()
    {
        StartCoroutine(WaitForKeyPress("down"));
    }

    public void RebindGround()
    {
        StartCoroutine(WaitForKeyPress("ground"));
    }

    public void RebindBoost()
    {
        StartCoroutine(WaitForKeyPress("boost"));
    }

    public void RebindKey1()
    {
        StartCoroutine(WaitForKeyPress("key1"));
    }

    public void RebindKey2()
    {
        StartCoroutine(WaitForKeyPress("key2"));
    }

    private System.Collections.IEnumerator WaitForKeyPress(string actionName)
    {
        yield return new WaitUntil(() => Input.anyKeyDown);
        foreach (KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keycode))
            {
                KeybindingManager.RebindKey(actionName, keycode);
                break;
            }
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        upBindingText.text = KeybindingManager.GetBindingName("up");
        downBindingText.text = KeybindingManager.GetBindingName("down");
        groundBindingText.text = KeybindingManager.GetBindingName("ground");
        boostBindingText.text = KeybindingManager.GetBindingName("boost");
        key1BindingText.text = KeybindingManager.GetBindingName("key1");
        key2BindingText.text = KeybindingManager.GetBindingName("key2");
        Debug.Log("Updated UI with current bindings");
    }

    public void Save()
    {

        KeybindingManager.instance.SaveKeybindingsToJson();
    }
}
