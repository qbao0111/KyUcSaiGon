using UnityEngine;

public abstract class RestorableEffect : MonoBehaviour
{
    public abstract void SetRestoredInstant(bool restored);
    public abstract void PlayRestore();
}
