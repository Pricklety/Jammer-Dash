using System;
using UnityEngine;
using UnityEngine.UI;
using JammerDash;
public class KeybindingUI : MonoBehaviour
{
    public KeybindingManager keybindingManager;
    public string keyName;

    private void Start()
    {
        KeybindingManager.instance.LoadKeybindingsFromJson();
        UpdateUI(this.GetComponent<Text>(), keyName);
        GetComponentInParent<Button>().onClick.RemoveAllListeners();
        GetComponentInParent<Button>().onClick.AddListener(Rebind);
    }

    public void Rebind()
    {
        StartCoroutine(WaitForKeyPress(keyName));
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
        UpdateUI(this.GetComponent<Text>(), keyName);
    }
    private void Update()
    {
        if (keybindingManager == null)
        {
            keybindingManager = KeybindingManager.instance;
            KeybindingManager.instance.LoadKeybindingsFromJson();
        }
    }

    public void UpdateUI(Text text, string name)
    {
        text.text = KeybindingManager.GetBindingName(name);
        Debug.Log("Updated UI with current bindings");
    }

    public void Save()
    {

        KeybindingManager.instance.SaveKeybindingsToJson();
    }
}
