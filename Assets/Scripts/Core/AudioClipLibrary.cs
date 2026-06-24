using UnityEngine;

[System.Serializable]
public class AudioClipLibraryEntry
{
    public string name;
    public AudioClip clip;
}

public class AudioClipLibrary : ScriptableObject
{
    public AudioClipLibraryEntry[] clips;

    public AudioClip GetClip(string clipName)
    {
        if (clips == null)
        {
            return null;
        }

        foreach (AudioClipLibraryEntry entry in clips)
        {
            if (entry != null && entry.name == clipName)
            {
                return entry.clip;
            }
        }

        return null;
    }
}
