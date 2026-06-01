#if UNITY_EDITOR
using UnityEditor;

public static class KyUcSaiGonDeveloperMenu
{
    private const string MenuPath = "Ky Uc Sai Gon/Developer Mode";

    [MenuItem(MenuPath)]
    private static void ToggleDeveloperMode()
    {
        bool next = !DeveloperMode.IsEnabled;
        DeveloperMode.SetEnabled(next);
        Menu.SetChecked(MenuPath, next);
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateDeveloperMode()
    {
        Menu.SetChecked(MenuPath, DeveloperMode.IsEnabled);
        return true;
    }
}
#endif
