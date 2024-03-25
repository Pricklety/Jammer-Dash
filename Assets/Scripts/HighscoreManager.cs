using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

public class HighscoreManager : MonoBehaviour
{
    private const string filePath = "/highscores.dat";
    private const string encryptionKey = "PRKEJOI20UHUH/z#(/SIAOQHM654!!"; // Change this to your own encryption key

    [System.Serializable]
    public class HighscoreData
    {
        public int level;
        public int score;
    }

    private void SaveHighscores(HighscoreData[] highscores)
    {
        string jsonData = JsonUtility.ToJson(highscores);
        string encryptedData = EncryptString(jsonData, encryptionKey);

        File.WriteAllText(Application.persistentDataPath + filePath, encryptedData);
    }

    private HighscoreData[] LoadHighscores()
    {
        string encryptedData = File.ReadAllText(Application.persistentDataPath + filePath);
        string decryptedData = DecryptString(encryptedData, encryptionKey);

        return JsonUtility.FromJson<HighscoreData[]>(decryptedData);
    }

    private string EncryptString(string input, string key)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        using (AesManaged aes = new AesManaged())
        {
            Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(key, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            aes.Key = keyDerivation.GetBytes(32);
            aes.IV = keyDerivation.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.Close();
                }
                input = Convert.ToBase64String(ms.ToArray());
            }
        }
        return input;
    }

    private string DecryptString(string input, string key)
    {
        byte[] inputBytes = Convert.FromBase64String(input);
        using (AesManaged aes = new AesManaged())
        {
            Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(key, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            aes.Key = keyDerivation.GetBytes(32);
            aes.IV = keyDerivation.GetBytes(16);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputBytes, 0, inputBytes.Length);
                    cs.Close();
                }
                input = Encoding.UTF8.GetString(ms.ToArray());
            }
        }
        return input;
    }
}
