using Newtonsoft.Json;
using System;
using UnityEngine;

namespace JammerDash
{

    public class SceneDataConverter : JsonConverter<SceneData>
    {
        public override SceneData ReadJson(JsonReader reader, Type objectType, SceneData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string json = reader.Value.ToString();
            SceneData sceneData = JsonUtility.FromJson<SceneData>(json);
            return sceneData;
        }

        public override void WriteJson(JsonWriter writer, SceneData value, JsonSerializer serializer)
        {
            string json = JsonUtility.ToJson(value);
            writer.WriteRawValue(json);
        }
    }
}