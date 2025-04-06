using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace JammerDash
{
    public class KeybindingManager : MonoBehaviour
    {
        [Header("Gameplay")]
        public static KeyCode up = KeyCode.W;
        public static KeyCode down = KeyCode.S;
        public static KeyCode boost = KeyCode.E;
        public static KeyCode lowboost = KeyCode.Q;
        public static KeyCode top = KeyCode.D;
        public static KeyCode ground = KeyCode.A;
        public static KeyCode hit1 = KeyCode.K;
        public static KeyCode hit2 = KeyCode.L;

        [Header("Editor")]
        public static KeyCode place = KeyCode.Mouse0;
        public static KeyCode moveCam = KeyCode.Mouse2;
        public static KeyCode moveObjectLeft = KeyCode.A;
        public static KeyCode moveObjectRight = KeyCode.D;
        public static KeyCode moveObjectUp = KeyCode.W;
        public static KeyCode moveObjectDown = KeyCode.S;
        // These 2 next keycodes contain a suffix with moveObjectLeft/Right (e.g moveObjectFast + moveObjectLeft is a control to quickly move an object to the left). //
        public static KeyCode moveObjectFast = KeyCode.LeftControl;
        public static KeyCode moveObjectSlow = KeyCode.LeftShift;

        public static KeyCode selectObject = KeyCode.Mouse1;
        public static KeyCode delete = KeyCode.Delete;
        // This keycode is a prefix to the "delete" keycode. This means this button has to be held then with the click of the "Delete" keybind it deletes all BPM Markers. //
        public static KeyCode deleteBPM = KeyCode.LeftControl;

        public static KeyCode playMode = KeyCode.Return;
        // This keycode is a prefix to the "playMode" keycode. This means this button has to be held then with the click of the "Play Mode" keybind it starts the song playback (on the time the user is in). //
        public static KeyCode songMode = KeyCode.LeftShift;

        // This keycode is a prefix to the "place" keycode. This means this button has to be help ON TOP of a long cube as well as the place keybind to change the size of a long cube.
        public static KeyCode changeLongCubeSize = KeyCode.LeftShift;

        // These 2 use a LeftControl prefix (You may only be able to change the suffix)
        public static KeyCode options = KeyCode.O;
        public static KeyCode menu = KeyCode.M;

        [Header("Main Menu")]
        public static KeyCode nextSong = KeyCode.V;
        public static KeyCode prevSong = KeyCode.Y;
        public static KeyCode pause = KeyCode.C;
        public static KeyCode play = KeyCode.X;

        [Header("Gameplay")]
        // This keybind uses a LeftShift prefix (You may only be able to change the suffix)
        public static KeyCode toggleUI = KeyCode.F1;

        [Header("Function keys - Global")]
        public static KeyCode screenshot = KeyCode.F12;
        public static KeyCode reloadPlaylist = KeyCode.F9;

        [Header("Function keys - Menu")]
        public static KeyCode debug = KeyCode.F2;
        public static KeyCode goToSelectedLevel = KeyCode.F4;
        public static KeyCode reloadData = KeyCode.F5;

        [SerializeField] public static KeybindingManager instance;

        private static string savePath;


        private void Awake()
        {
            savePath = Main.gamePath + "/keybindings.json";
            LoadKeybindingsFromJson();
            instance = this;
        }

        public void SaveKeybindingsToJson()
        {
            KeybindingsData data = new KeybindingsData
            {
                up = up,
                down = down,
                boost = boost,
                lowboost = lowboost,
                top = top,
                ground = ground,
                hit1 = hit1,
                hit2 = hit2,
                place = place,
                moveCam = moveCam,
                moveObjectLeft = moveObjectLeft,
                moveObjectRight = moveObjectRight,
                moveObjectUp = moveObjectUp,
                moveObjectDown = moveObjectDown,
                moveObjectFast = moveObjectFast,
                moveObjectSlow = moveObjectSlow,
                selectObject = selectObject,
                delete = delete,
                deleteBPM = deleteBPM,
                playMode = playMode,
                songMode = songMode,
                changeLongCubeSize = changeLongCubeSize,
                options = options,
                menu = menu,
                nextSong = nextSong,
                prevSong = prevSong,
                pause = pause,
                play = play,
                toggleUI = toggleUI,
                screenshot = screenshot,
                reloadPlaylist = reloadPlaylist,
                debug = debug,
                goToSelectedLevel = goToSelectedLevel,
                reloadData = reloadData
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

                up = data.up != KeyCode.None ? data.up : KeyCode.W;
                down = data.down != KeyCode.None ? data.down : KeyCode.S;
                boost = data.boost != KeyCode.None ? data.boost : KeyCode.E;
                lowboost = data.lowboost != KeyCode.None ? data.lowboost : KeyCode.Q;
                top = data.top != KeyCode.None ? data.top : KeyCode.D;
                ground = data.ground != KeyCode.None ? data.ground : KeyCode.A;
                hit1 = data.hit1 != KeyCode.None ? data.hit1 : KeyCode.K;
                hit2 = data.hit2 != KeyCode.None ? data.hit2 : KeyCode.L;
                place = data.place != KeyCode.None ? data.place : KeyCode.Mouse0;
                moveCam = data.moveCam != KeyCode.None ? data.moveCam : KeyCode.Mouse0;
                moveObjectLeft = data.moveObjectLeft != KeyCode.None ? data.moveObjectLeft : KeyCode.A;
                moveObjectRight = data.moveObjectRight != KeyCode.None ? data.moveObjectRight : KeyCode.D;
                moveObjectUp = data.moveObjectUp != KeyCode.None ? data.moveObjectUp : KeyCode.W;
                moveObjectDown = data.moveObjectDown != KeyCode.None ? data.moveObjectDown : KeyCode.S;
                moveObjectFast = data.moveObjectFast != KeyCode.None ? data.moveObjectFast : KeyCode.LeftControl;
                moveObjectSlow = data.moveObjectSlow != KeyCode.None ? data.moveObjectSlow : KeyCode.LeftShift;
                selectObject = data.selectObject != KeyCode.None ? data.selectObject : KeyCode.Mouse1;
                delete = data.delete != KeyCode.None ? data.delete : KeyCode.Delete;
                deleteBPM = data.deleteBPM != KeyCode.None ? data.deleteBPM : KeyCode.LeftControl;
                playMode = data.playMode != KeyCode.None ? data.playMode : KeyCode.Return;
                songMode = data.songMode != KeyCode.None ? data.songMode : KeyCode.LeftShift;
                changeLongCubeSize = data.changeLongCubeSize != KeyCode.None ? data.changeLongCubeSize : KeyCode.LeftShift;
                options = data.options != KeyCode.None ? data.options : KeyCode.O;
                menu = data.menu != KeyCode.None ? data.menu : KeyCode.M;
                nextSong = data.nextSong != KeyCode.None ? data.nextSong : KeyCode.V;
                prevSong = data.prevSong != KeyCode.None ? data.prevSong : KeyCode.Y;
                pause = data.pause != KeyCode.None ? data.pause : KeyCode.C;
                play = data.play != KeyCode.None ? data.play : KeyCode.X;
                toggleUI = data.toggleUI != KeyCode.None ? data.toggleUI : KeyCode.F1;
                screenshot = data.screenshot != KeyCode.None ? data.screenshot : KeyCode.F12;
                reloadPlaylist = data.reloadPlaylist != KeyCode.None ? data.reloadPlaylist : KeyCode.F9;
                debug = data.debug != KeyCode.None ? data.debug : KeyCode.F2;
                goToSelectedLevel = data.goToSelectedLevel != KeyCode.None ? data.goToSelectedLevel : KeyCode.F4;
                reloadData = data.reloadData != KeyCode.None ? data.reloadData : KeyCode.F5;

                
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
                case "lowboost":
                    return lowboost.ToString();
                case "top":
                    return top.ToString();
                case "ground":
                    return ground.ToString();
                case "hit1":
                    return hit1.ToString();
                case "hit2":
                    return hit2.ToString();
                case "place":
                    return place.ToString();
                case "moveCam":
                    return moveCam.ToString();
                case "moveObjectLeft":
                    return moveObjectLeft.ToString();
                case "moveObjectRight":
                    return moveObjectRight.ToString();
                case "moveObjectUp":
                    return moveObjectUp.ToString();
                case "moveObjectDown":
                    return moveObjectDown.ToString();
                case "moveObjectFast":
                    return moveObjectFast.ToString();
                case "moveObjectSlow":
                    return moveObjectSlow.ToString();
                case "selectObject":
                    return selectObject.ToString();
                case "delete":
                    return delete.ToString();
                case "deleteBPM":
                    return deleteBPM.ToString();
                case "playMode":
                    return playMode.ToString();
                case "songMode":
                    return songMode.ToString();
                case "changeLongCubeSize":
                    return changeLongCubeSize.ToString();
                case "options":
                    return options.ToString();
                case "menu":
                    return menu.ToString();
                case "nextSong":
                    return nextSong.ToString();
                case "prevSong":
                    return prevSong.ToString();
                case "pause":
                    return pause.ToString();
                case "play":
                    return play.ToString();
                case "toggleUI":
                    return toggleUI.ToString();
                case "screenshot":
                    return screenshot.ToString();
                case "reloadPlaylist":
                    return reloadPlaylist.ToString();
                case "debug":
                    return debug.ToString();
                case "goToSelectedLevel":
                    return goToSelectedLevel.ToString();
                case "reloadData":
                    return reloadData.ToString();
                default:
                    return "Undefined";
            }
        }
        private void Update()
        {
            instance = this;
        }

        private static HashSet<KeyCode> usedGameplayKeys = new HashSet<KeyCode>();

        public static void RebindKey(string actionName, KeyCode newKey)
        {
            // Check if the new key is already used
            if (usedGameplayKeys.Contains(newKey))
            {
                Debug.LogError($"Key '{newKey}' is already in use. Please choose a different key.");
                return;
            }

            // Remove the old key from the set if applicable
            KeyCode oldKey = GetCurrentKey(actionName);
            if (usedGameplayKeys.Contains(oldKey))
            {
                usedGameplayKeys.Remove(oldKey);
            }

            // Assign the new key
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
                case "lowboost":
                    lowboost = newKey;
                    break;
                case "ground":
                    ground = newKey;
                    break;
                case "top":
                    top = newKey;
                    break;
                case "hit1":
                    hit1 = newKey;
                    break;
                case "hit2":
                    hit2 = newKey;
                    break;
                case "place":
                    place = newKey;
                    break;
                case "moveCam":
                    moveCam = newKey;
                    break;
                case "moveObjectLeft":
                    moveObjectLeft = newKey;
                    break;
                case "moveObjectRight":
                    moveObjectRight = newKey;
                    break;
                case "moveObjectUp":
                    moveObjectUp = newKey;
                    break;
                case "moveObjectDown":
                    moveObjectDown = newKey;
                    break;
                case "moveObjectFast":
                    moveObjectFast = newKey;
                    break;
                case "moveObjectSlow":
                    moveObjectSlow = newKey;
                    break;
                case "selectObject":
                    selectObject = newKey;
                    break;
                case "delete":
                    delete = newKey;
                    break;
                case "deleteBPM":
                    deleteBPM = newKey;
                    break;
                case "playMode":
                    playMode = newKey;
                    break;
                case "songMode":
                    songMode = newKey;
                    break;
                case "changeLongCubeSize":
                    changeLongCubeSize = newKey;
                    break;
                case "options":
                    options = newKey;
                    break;
                case "menu":
                    menu = newKey;
                    break;
                case "nextSong":
                    nextSong = newKey;
                    break;
                case "prevSong":
                    prevSong = newKey;
                    break;
                case "pause":
                    pause = newKey;
                    break;
                case "play":
                    play = newKey;
                    break;
                case "toggleUI":
                    toggleUI = newKey;
                    break;
                case "screenshot":
                    screenshot = newKey;
                    break;
                case "reloadPlaylist":
                    reloadPlaylist = newKey;
                    break;
                case "debug":
                    debug = newKey;
                    break;
                case "goToSelectedLevel":
                    goToSelectedLevel = newKey;
                    break;
                case "reloadData":
                    reloadData = newKey;
                    break;
                default:
                    Debug.LogError($"Action '{actionName}' not found for rebinding.");
                    return;
            }
        }
        private void InitializeUsedKeys()
        {
            usedGameplayKeys.Add(up);
            usedGameplayKeys.Add(down);
            usedGameplayKeys.Add(boost);
            usedGameplayKeys.Add(lowboost);
            usedGameplayKeys.Add(top);
            usedGameplayKeys.Add(ground);
            usedGameplayKeys.Add(hit1);
            usedGameplayKeys.Add(hit2);
        }
        private static KeyCode GetCurrentKey(string actionName)
        {
            return actionName switch
            {
                "up" => up,
                "down" => down,
                "boost" => boost,
                "lowboost" => lowboost,
                "top" => top,
                "ground" => ground,
                "hit1" => hit1,
                "hit2" => hit2,
                "place" => place,
                "moveCam" => moveCam,
                "moveObjectLeft" => moveObjectLeft,
                "moveObjectRight" => moveObjectRight,
                "moveObjectUp" => moveObjectUp,
                "moveObjectDown" => moveObjectDown,
                "moveObjectFast" => moveObjectFast,
                "moveObjectSlow" => moveObjectSlow,
                "selectObject" => selectObject,
                "delete" => delete,
                "deleteBPM" => deleteBPM,
                "playMode" => playMode,
                "songMode" => songMode,
                "changeLongCubeSize" => changeLongCubeSize,
                "options" => options,
                "menu" => menu,
                "nextSong" => nextSong,
                "prevSong" => prevSong,
                "pause" => pause,
                "play" => play,
                "toggleUI" => toggleUI,
                "screenshot" => screenshot,
                "reloadPlaylist" => reloadPlaylist,
                "debug" => debug,
                "goToSelectedLevel" => goToSelectedLevel,
                "reloadData" => reloadData,
                _ => KeyCode.None,
            };
        }
    }
        [System.Serializable]
    public class KeybindingsData
    {
        public KeyCode up;
        public KeyCode down;
        public KeyCode boost;
        public KeyCode lowboost;
        public KeyCode top;
        public KeyCode ground;
        public KeyCode hit1;
        public KeyCode hit2;
        public KeyCode place;
        public KeyCode moveCam;
        public KeyCode moveObjectLeft;
        public KeyCode moveObjectRight;
        public KeyCode moveObjectUp;
        public KeyCode moveObjectDown;
        public KeyCode moveObjectFast;
        public KeyCode moveObjectSlow;
        public KeyCode selectObject;
        public KeyCode delete;
        public KeyCode deleteBPM;
        public KeyCode playMode;
        public KeyCode songMode;
        public KeyCode changeLongCubeSize;
        public KeyCode options;
        public KeyCode menu;
        public KeyCode nextSong;
        public KeyCode prevSong;
        public KeyCode pause;
        public KeyCode play;
        public KeyCode toggleUI;
        public KeyCode screenshot;
        public KeyCode reloadPlaylist;
        public KeyCode debug;
        public KeyCode goToSelectedLevel;
        public KeyCode reloadData;
    }
}