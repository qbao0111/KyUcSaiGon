using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string MainMenu = "Scene_MainMenu";
    public const string Loading = "Scene_Loading";
    public const string BusHub = "Scene_00_BusHub";
    public const string NguyenHue = "Scene_01_NguyenHue_Tutorial";
    public const string BenThanh = "Scene_02_BenThanh";
    public const string DinhDocLap = "Scene_03_DinhDocLap";
    public const string NhaThoDucBa = "Scene_04_NhaThoDucBa";
    public const string Bitexco = "Scene_05_Bitexco";
    public const string BachDang = "Scene_06_BachDang";
    public const string Ending = "Scene_07_Ending";

    public static string TargetScene { get; private set; }

    public static void Load(string sceneName)
    {
        sceneName = ResolveSceneRedirect(sceneName);
        PrototypeLogger.Info("Loading scene: " + sceneName);

        if (sceneName == MainMenu || sceneName == Loading)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        TargetScene = sceneName;
        SceneManager.LoadScene(Loading);
    }

    private static string ResolveSceneRedirect(string sceneName)
    {
        if (sceneName != BusHub || GameProgressManager.Instance == null || !GameProgressManager.Instance.AreAllMemoriesRestored())
        {
            return sceneName;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        bool returningFromRequiredMemoryScene = currentScene == NguyenHue || currentScene == NhaThoDucBa;
        return returningFromRequiredMemoryScene ? Ending : sceneName;
    }
}
