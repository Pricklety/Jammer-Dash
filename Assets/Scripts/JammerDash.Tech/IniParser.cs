using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JammerDash.Tech {
   public class IniParser
{
    public Dictionary<string, Dictionary<string, string>> ParseIni(string filePath)
    {
        var data = new Dictionary<string, Dictionary<string, string>>();
        string currentSection = "";

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]")) // Section
                {
                    currentSection = line.Trim('[', ']');
                    if (!data.ContainsKey(currentSection))
                    {
                        data[currentSection] = new Dictionary<string, string>();
                    }
                }
                else if (line.Contains("=") && !string.IsNullOrEmpty(currentSection)) // Key-Value pair
                {
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        data[currentSection][keyValue[0].Trim()] = keyValue[1].Trim();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("INI file not found: " + filePath);
        }

        return data;
    }
}
}

