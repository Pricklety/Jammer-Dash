using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

public class IpService : MonoBehaviour
{
    private const string IpifyApiUrl = "https://api.ipify.org?format=json";

    public static IEnumerator GetPublicIpAddress(System.Action<string> callback)
    {
        UnityWebRequest request = UnityWebRequest.Get(IpifyApiUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            IpInfo ipInfo = JsonUtility.FromJson<IpInfo>(jsonResponse);

            if (ipInfo != null)
            {
                string ipAddress = ipInfo.ip;
                callback?.Invoke(ipAddress);
            }
            else
            {
                Debug.LogError("Failed to parse IP information.");
                callback?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError($"Failed to get public IP address. Error: {request.error}");
            callback?.Invoke(null);
        }
    }

    [System.Serializable]
    public class IpInfo
    {
        public string ip;
        // You can add other fields from the ipify response if needed
    }
}
