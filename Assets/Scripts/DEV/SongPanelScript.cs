using UnityEngine;
using UnityEngine.UI;

public class SongPanelScript : MonoBehaviour
{
    public Text songNameText;
    public Text artistText;

    public void SetSongData(string songName, string artist)
    {
        songNameText.text = songName;
        artistText.text = artist;
    }

    public void PlaySong()
    {
        // Add your logic to play the song here
        Debug.Log("Playing song: " + songNameText.text);
    }

    public void UseSong()
    {
        // Add your logic to set the song as the main audio source here
        Debug.Log("Using song: " + songNameText.text);
    }
}
