using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace JammerDash {
public class JsonConverter : MonoBehaviour
{
    public GameObject warningPanel; 
    public Text warningText;        
    public Text progressText;       
    public Button yesButton;        
    public Button noButton;         

    private string directoryPath;
    private string[] jsonFiles;

    void Start()
{
    directoryPath = Main.gamePath;
    jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);

    if (jsonFiles.Length == 0)
    {
        Debug.Log("No JSON files found.");
        progressText.text = "No JSON files found.";
        return;
    }

    // Check if any file is in the old format
    bool needsConversion = false;
    foreach (string file in jsonFiles)
    {
        if (IsOldFormat(file))
        {
            needsConversion = true;
            break; 
        }
    }

    if (needsConversion)
    {
        warningText.text = $"WARNING: This will modify your level files in:\n{directoryPath}\nContinue?";
        warningPanel.SetActive(true);

        yesButton.onClick.AddListener(() => StartCoroutine(ProcessFiles()));
        noButton.onClick.AddListener(CancelOperation);
    }
    else
    {
        Debug.Log("No old-format JSON files detected.");
    }
}

bool IsOldFormat(string filePath)
{
    try
    {
        string content = File.ReadAllText(filePath);
        JObject json = JObject.Parse(content);

        
        bool isOldVersion = json.ContainsKey("version") && (int)json["version"] == 1;

        return isOldVersion;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error reading {filePath}: {ex.Message}");
        return false;
    }
}

    string[] bannedFileNames = { "settings.json", "keybindings.json"};
    IEnumerator ProcessFiles()
{
    progressText.text = "Starting conversion...";
    Debug.Log("Starting JSON conversion...");
    noButton.interactable = false;

    for (int i = 0; i < jsonFiles.Length; i++)
    {
        string file = jsonFiles[i];
        string fileName = Path.GetFileName(file);
        if (bannedFileNames.Contains(fileName))
            continue;
        progressText.text = $"Processing {i + 1}/{jsonFiles.Length}: {fileName}";
        Debug.Log($"Processing {fileName}");

        // Backup the file before modifying it
        string backupPath = Path.Combine(Path.GetDirectoryName(file), $"{fileName}.bak");
        try
        {
            // Ensure the backup is created
            File.Copy(file, backupPath, true);  // Creates a backup or overwrites it
            Debug.Log($"Backup created for: {fileName}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating backup for {fileName}: {ex.Message}");
            continue; // Skip to next file if backup fails
        }

        try
        {
            string oldJson = File.ReadAllText(file);  // Read the original JSON file
            Debug.Log($"Original JSON for {fileName}: {oldJson}");

            string newJson = ConvertJson(oldJson);    // Convert the old JSON to new format
            Debug.Log($"Converted JSON for {fileName}: {newJson}");

            // Ensure the converted JSON is not null or empty
            if (!string.IsNullOrEmpty(newJson))
            {
                File.WriteAllText(file, newJson);  // Write the converted JSON back to the file
                Debug.Log($"Converted: {fileName}");
            }
            else
            {
                Debug.LogError($"Conversion failed for {fileName}. Empty JSON returned.");
            }
        }
        catch (Exception ex)
        {
            // Log any errors that occur during conversion
            Debug.LogError($"Error processing {fileName}: {ex.Message}");

            // Restore the original file from the backup if conversion fails
            try
            {
                File.Copy(backupPath, file, true);  // Restore from backup
                Debug.Log($"Restored original file for {fileName}.");
            }
            catch (Exception restoreEx)
            {
                Debug.LogError($"Failed to restore backup for {fileName}: {restoreEx.Message}");
            }
        }

        yield return null; // Prevent Unity from freezing
    }

    progressText.text = "Conversion Complete!";
    warningPanel.SetActive(false);
    noButton.interactable = true;
    Debug.Log("JSON conversion complete.");
}

public string ConvertJson(string oldJson)
{
    // Log for debugging before converting the JSON
    Debug.Log("Converting JSON...");

    

    try
    {
        var oldData = JsonConvert.DeserializeObject<SceneDataV1>(oldJson);

        if (oldData.version ==  2) {
            return null;
        }
        if (oldData == null)
        {
            Debug.LogError("Failed to deserialize the old JSON data.");
            return null;
        }

        var newData = new SceneData
        {
            version = 2,
            name = oldData.sceneName,
            creator = oldData.creator,
            ID = oldData.ID,
            ground = oldData.ground,
            levelLength = oldData.levelLength,
            calculatedDifficulty = oldData.calculatedDifficulty,
            defBGColor = oldData.defBGColor,
            cubePositions = ConvertVector3ListToVector2List(oldData.cubePositions),
            sawPositions = ConvertVector3ListToVector2List(oldData.sawPositions),
            longCubePositions = ConvertVector3ListToVector2List(oldData.longCubePositions),
            longCubeWidth = oldData.longCubeWidth,
            cubeType = oldData.cubeType,
            playerHP = oldData.playerHP,
            boxSize = oldData.boxSize,
            bpm = oldData.bpm,
            artist = oldData.artist,
            romanizedArtist = oldData.romanizedArtist,
            songName = oldData.songName,
            romanizedName = oldData.romanizedName,
            songLength = oldData.songLength,
            offset = oldData.offset,
            gameVersion = oldData.gameVersion,
            saveTime = oldData.saveTime,
        };
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = { new UnityVector2Converter(), new UnityColorConverter() }
        };
        // Serialize the new data back to JSON
        string newJson = JsonConvert.SerializeObject(newData, settings);

        // Log the result of conversion for debugging
        Debug.Log($"New JSON data after conversion: {newJson}");

        return newJson;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error during conversion: {ex.Message}");
        return null;
    }
}
    void CancelOperation()
    {
        warningPanel.SetActive(false);
        progressText.text = "Operation cancelled.";
        Debug.Log("Operation cancelled.");
    }

    static List<Vector2> ConvertVector3ListToVector2List(List<Vector3> positions)
{
    List<Vector2> newPositions = new List<Vector2>();
    foreach (Vector3 pos in positions)
    {
        // Use x and y components, ignoring z
        newPositions.Add(new Vector2(pos.x, pos.y));
    }
    return newPositions;
}

}
}
public class UnityVector2Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2) || objectType == typeof(Vector3);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is Vector2 vector2)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vector2.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vector2.y);
            writer.WriteEndObject();
        }
        else if (value is Vector3 vector3)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vector3.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vector3.y);
            writer.WritePropertyName("z");
            writer.WriteValue(vector3.z);
            writer.WriteEndObject();
        }
    }

    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        if (objectType == typeof(Vector2))
        {
            return new Vector2((float)jo["x"], (float)jo["y"]);
        }
        else if (objectType == typeof(Vector3))
        {
            return new Vector3((float)jo["x"], (float)jo["y"], (float)jo["z"]);
        }
        return null;
    }
}

public class UnityColorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Color);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Color color = (Color)value;
        writer.WriteStartObject();
        writer.WritePropertyName("r"); writer.WriteValue(color.r);
        writer.WritePropertyName("g"); writer.WriteValue(color.g);
        writer.WritePropertyName("b"); writer.WriteValue(color.b);
        writer.WritePropertyName("a"); writer.WriteValue(color.a);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        return new Color(
            (float)jo["r"],
            (float)jo["g"],
            (float)jo["b"],
            (float)jo["a"]
        );
    }
}
