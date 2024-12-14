using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace JammerDash.EasterEggs
{
    public class MainMenu : MonoBehaviour
    {
        public Sprite main;
        public Sprite fools;
        public Sprite valentines;
        public Sprite easter;
        public Sprite christmas;
        public Sprite halloween;
        public Sprite jp;

        public Image logo;

        public void FixedUpdate()
        {
            if (LocalizationSettings.SelectedLocale.Identifier == "ja-JP")
            {
                logo.sprite = jp;
            }
            else
            {
                if (DateTime.Now.Month == 12)
                {
                    logo.sprite = christmas;
                }
                else if (DateTime.Now.Month == 10)
                {
                    logo.sprite = halloween;
                }
                else if (DateTime.Now.Month == 3 && DateTime.Now.Day > 21 || DateTime.Now.Month == 4 && DateTime.Now.Day < 26 && DateTime.Now.Day != 1)
                {
                    logo.sprite = easter;
                }
                else if (DateTime.Now.Month == 2 && DateTime.Now.Day == 14)
                {
                    logo.sprite = valentines;
                }
                else if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
                {
                    logo.sprite = fools;
                }
                else
                {
                    logo.sprite = main;
                }
            }
           
        }
    }
}