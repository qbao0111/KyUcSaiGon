using UnityEngine;

public static class PrototypeLogger
{
    private const string Prefix = "[KyUcSaiGon]";

    public static void Info(string message)
    {
        Debug.Log(Prefix + " " + message);
    }

    public static void Warning(string message)
    {
        Debug.LogWarning(Prefix + " " + message);
    }

    public static void Error(string message, Object context = null)
    {
        Debug.LogError(Prefix + " " + message, context);
    }
}
