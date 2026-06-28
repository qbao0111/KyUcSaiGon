using UnityEngine;
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
#endif

public class FullscreenHotkeyHandler : MonoBehaviour
{
    [SerializeField] private bool makeFullscreenAtStart = true;
	
    void Start() 
    { 
#if UNITY_EDITOR
        if (makeFullscreenAtStart) 
        { 
            // Ensure the Game View is fullscreen without toggling it off if it's already open
            FullscreenGameView.EnsureFullscreen(true); 
        } 
#endif
    }

    void Update() 
    {
#if UNITY_EDITOR
        // Toggle fullscreen when hotkey pressed (Backslash '\' key)
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            FullscreenGameView.Toggle();
        }
#endif
    }
}

#if UNITY_EDITOR
// Below code from: https://gist.github.com/fnuecke/d4275087cc7969257eae0f939fac3d2f
// Improvements for Unity 6, Multi-monitor support and Taskbar hiding:
// 1. Closes the fullscreen window automatically when exiting Play Mode.
// 2. Positions the popup on the correct monitor relative to the main Editor window.
// 3. Robust null checks and cleanup to avoid stuck/ghost editor windows.
// 4. Force Window to be Topmost on Windows to cover the Taskbar.
// 5. Closes the window synchronously during EnteredEditMode to prevent internal Unity NullReferenceException.
// 6. Seamless scene transitions by keeping the window open across scene loads.
// 7. Forces true full screen resolution via Win32 API to prevent Windows/Unity from clipping the bottom bar.
public static class FullscreenGameView
{
    static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
    static readonly PropertyInfo ShowToolbarProperty = GameViewType.GetProperty("showToolbar", BindingFlags.Instance | BindingFlags.NonPublic);
    static readonly object False = false; // Only box once.

    static EditorWindow instance;

    // Win32 API to force the window to be topmost (covers the Windows Taskbar) and resize it
#if UNITY_EDITOR_WIN
    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    private static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_SHOWWINDOW = 0x0040;
#endif

    static FullscreenGameView() 
    { 
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload; 
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnBeforeAssemblyReload() 
    { 
        CloseInstance(); 
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Close the fullscreen Game View ONLY when we have fully entered Edit Mode.
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            CloseInstance();
        }
    }

    private static void CloseInstance()
    {
        if (instance != null) 
        { 
            instance.Close(); 
            instance = null; 
        } 
    }

    public static void EnsureFullscreen(bool fullscreen)
    {
        if (fullscreen)
        {
            if (instance == null)
            {
                OpenInstance();
            }
        }
        else
        {
            CloseInstance();
        }
    }

    private static void OpenInstance()
    {
        if (GameViewType == null)
        {
            Debug.LogError("GameView type not found.");
            return;
        }

        if (ShowToolbarProperty == null)
        {
            Debug.LogWarning("GameView.showToolbar property not found.");
        }

        instance = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);
        instance.titleContent = new GUIContent("Fullscreen Game View");

        ShowToolbarProperty?.SetValue(instance, False);

        // Determine correct screen coordinates/offset for multi-monitor setups
        Rect mainWindowRect = EditorGUIUtility.GetMainWindowPosition();
        var resolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        
        // Estimate monitor position offset
        float xOffset = Mathf.Round(mainWindowRect.x / resolution.x) * resolution.x;
        float yOffset = Mathf.Round(mainWindowRect.y / resolution.y) * resolution.y;
        
        var fullscreenRect = new Rect(xOffset, yOffset, resolution.x, resolution.y);
        
        // Set size constraints in Unity to match full resolution
        instance.minSize = resolution;
        instance.maxSize = resolution;
        instance.position = fullscreenRect;

        instance.ShowPopup();
        instance.Focus();

#if UNITY_EDITOR_WIN
        // Find the window handle and set it to HWND_TOPMOST, forcing the exact coordinates/resolution
        IntPtr hWnd = FindWindowByCaption(IntPtr.Zero, "Fullscreen Game View");
        if (hWnd != IntPtr.Zero)
        {
            int x = Mathf.RoundToInt(fullscreenRect.x);
            int y = Mathf.RoundToInt(fullscreenRect.y);
            int w = Mathf.RoundToInt(fullscreenRect.width);
            int h = Mathf.RoundToInt(fullscreenRect.height);
            
            // Set size and position synchronously at the OS level to override Unity clamping
            SetWindowPos(hWnd, HWND_TOPMOST, x, y, w, h, SWP_SHOWWINDOW);
        }
#endif
    }
    
    [MenuItem("Window/General/Game (Fullscreen) %#&2", priority = 2)]
    public static void Toggle()
    {
        if (instance != null)
        {
            CloseInstance();
        }
        else
        {
            OpenInstance();
        }
    }
}
#endif
