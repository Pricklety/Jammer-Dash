using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class KeybindingManager : MonoBehaviour
{
    public static KeyCode up = KeyCode.W;
    public static KeyCode down = KeyCode.S;
    public static KeyCode boost = KeyCode.Space;
    public static KeyCode ground = KeyCode.A;
    public static KeyCode hit1 = KeyCode.K;
    public static KeyCode hit2 = KeyCode.L;
    [SerializeField] public static KeybindingManager instance;

    private static string savePath;


    private void Awake() 
    {
        LoadKeybindingsFromJson();
        savePath = Application.persistentDataPath + "/keybindings.json";
        instance = this;
    }

    public void SaveKeybindingsToJson()
    {
        KeybindingsData data = new KeybindingsData
        {
            up = up,
            down = down,
            boost = boost,
            ground = ground,
            hit1 = hit1,
            hit2 = hit2
        };

        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, jsonData);
        Debug.Log("Saved keybindings to " + savePath);
    }

    public void LoadKeybindingsFromJson()
    {
        if (File.Exists(savePath))
        {
            string jsonData = File.ReadAllText(savePath);
            Debug.Log("Loaded JSON data: " + jsonData); // Check the loaded JSON data

            KeybindingsData data = JsonUtility.FromJson<KeybindingsData>(jsonData);

            up = data.up;
            down = data.down;
            boost = data.boost;
            ground = data.ground;
            hit1 = data.hit1;
            hit2 = data.hit2;

            Debug.Log("Loaded keybindings: up=" + up + ", down=" + down + ", boost=" + boost +
                      ", ground=" + ground + ", hit1=" + hit1 + ", hit2=" + hit2);
        }
        else
        {
            Debug.LogWarning("No keybindings file found at " + savePath);
        }
    }



    public static string GetBindingName(string actionName)
    {
        switch (actionName)
        {
            case "up":
                return up.ToString();
            case "down":
                return down.ToString();
            case "boost":
                return boost.ToString();
            case "ground":
                return ground.ToString();
            case "key1":
                return hit1.ToString();
            case "key2":
                return hit2.ToString();
            default:
                return "Undefined";
        }
    }
    private void Update()
    {
        instance = this;
    }
    public static void RebindKey(string actionName, KeyCode newKey)
    {
        switch (actionName)
        {
            case "up":
                up = newKey;
                break;
            case "down":
                down = newKey;
                break;
            case "boost":
                boost = newKey;
                break;
            case "ground":
                ground = newKey;
                break;
            case "key1":
                hit1 = newKey;
                break;
            case "key2":
                hit2 = newKey;
                break;
            default:
                Debug.LogError($"Action '{actionName}' not found for rebinding.");
                return;
        }

    }

    [System.Serializable]
    public class KeybindingsData
    {
        public KeyCode up;
        public KeyCode down;
        public KeyCode boost;
        public KeyCode ground;
        public KeyCode hit1;
        public KeyCode hit2;
    }
}
