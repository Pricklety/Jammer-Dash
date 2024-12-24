using JammerDash.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JammerDash.Menus
{
    public class KeybindPanel
    {
        public static void ToggleFunction(string func, string key)
        {
            if (EventSystem.current.currentSelectedGameObject == null ||
                    EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() == null)
            {
                AudioManager.Instance.toggleAnim.Rebind();
                AudioManager.Instance.toggleAnim.Play("keybindFunc", 0, 0);
                AudioManager.Instance.functionName.text = func;
                AudioManager.Instance.functionKeybind.text = key;
            }

        }
    }
}


