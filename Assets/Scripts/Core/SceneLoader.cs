using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string BusHub = "Scene_00_BusHub";
    public const string NguyenHue = "Scene_01_NguyenHue_Tutorial";
    public const string BenThanh = "Scene_02_BenThanh";
    public const string DinhDocLap = "Scene_03_DinhDocLap";
    public const string NhaThoDucBa = "Scene_04_NhaThoDucBa";
    public const string Bitexco = "Scene_05_Bitexco";
    public const string BachDang = "Scene_06_BachDang";
    public const string Ending = "Scene_07_Ending";

    public static void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
