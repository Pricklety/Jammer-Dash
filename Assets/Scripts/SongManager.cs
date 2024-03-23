using UnityEngine;

public class SongManager : MonoBehaviour
{
    public GameObject songPanelPrefab;
    public Transform songPanelParent; // Assign this in the Inspector
    public SongData[] songs; // Ensure you populate this array in the Inspector

    [System.Serializable]
    public class SongData
    {
        public string songName;
        public string artist;
    }


    void LoadSongs()
    {
        foreach (SongData songData in songs)
        {
            GameObject panel = Instantiate(songPanelPrefab, songPanelParent);
            SongPanelScript panelScript = panel.GetComponent<SongPanelScript>();
            panelScript.SetSongData(songData.songName, songData.artist);
        }
    }
}
