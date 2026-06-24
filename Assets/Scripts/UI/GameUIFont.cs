using UnityEngine;

public static class GameUIFont
{
    private const string RegularPath = "UI/Fonts/BeVietnamPro-SemiBold";
    private const string BoldPath = "UI/Fonts/BeVietnamPro-Bold";

    private static Font regular;
    private static Font bold;

    public static Font Regular
    {
        get
        {
            regular ??= Resources.Load<Font>(RegularPath);
            return regular != null ? regular : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    public static Font Bold
    {
        get
        {
            bold ??= Resources.Load<Font>(BoldPath);
            return bold != null ? bold : Regular;
        }
    }
}
