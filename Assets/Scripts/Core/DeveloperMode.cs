using UnityEngine;

public static class DeveloperMode
{
    private const string PlayerPrefsKey = "KyUcSaiGon_DeveloperMode";

    public static bool IsEnabled => PlayerPrefs.GetInt(PlayerPrefsKey, 0) == 1;

    public static void SetEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        GameProgressManager.Instance?.RefreshDeveloperMode();
        PrototypeLogger.Info("Developer mode: " + (enabled ? "ON" : "OFF"));
    }
}
