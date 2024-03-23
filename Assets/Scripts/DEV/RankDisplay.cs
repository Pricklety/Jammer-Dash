using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable disable
public class RankDisplay : MonoBehaviour
{
    public Text rankText;
    public int sceneIndex;
    public string keyName;

    private void Start() => DisplayRank(sceneIndex, this.keyName);

    private void DisplayRank(int index, string playerkey)
    {
        SceneManager.GetSceneByBuildIndex(index);
        if (!PlayerPrefs.HasKey(playerkey))
            return;
        rankText.text = PlayerPrefs.GetString(playerkey + "Tier");
    }
}
