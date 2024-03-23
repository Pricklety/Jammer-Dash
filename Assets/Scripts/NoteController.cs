using UnityEngine;

public enum NoteType
{
    Harmonious,
    Dissonant
}

public class NoteController : MonoBehaviour
{
    public NoteType noteType;

    // You can add additional properties or methods as needed

    public void SetNoteType(NoteType type)
    {
        noteType = type;

        // Adjust appearance or behavior based on note type
        if (noteType == NoteType.Harmonious)
        {
            // Set harmonious note appearance or behavior
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            // Set dissonant note appearance or behavior
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    // You can add more methods or logic here as needed
}
