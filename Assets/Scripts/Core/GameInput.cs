using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class GameInput
{
    public static Vector2 Move
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Vector2 value = Vector2.zero;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) value.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) value.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) value.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) value.y += 1f;
                return Vector2.ClampMagnitude(value, 1f);
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
#else
            return Vector2.zero;
#endif
        }
    }

    public static Vector2 Look
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.delta.ReadValue();
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 8f;
#else
            return Vector2.zero;
#endif
        }
    }

    public static bool InteractPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.E);
#else
            return false;
#endif
        }
    }

    public static bool CancelPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
    }

    public static bool SubmitPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#else
            return false;
#endif
        }
    }

    public static bool BackspacePressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Backspace);
#else
            return false;
#endif
        }
    }

    public static string PressedPuzzleToken()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            Key[] digitKeys = { Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };
            Key[] numpadKeys = { Key.Numpad0, Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9 };

            for (int i = 0; i < digitKeys.Length; i++)
            {
                if (Keyboard.current[digitKeys[i]].wasPressedThisFrame || Keyboard.current[numpadKeys[i]].wasPressedThisFrame)
                {
                    return i.ToString();
                }
            }
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha0 + i)) || Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad0 + i)))
            {
                return i.ToString();
            }
        }
#endif
        return string.Empty;
    }
}

public static class CursorLockManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureRuntimeEnforcer();

        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == "Scene_MainMenu" || activeScene == "Scene_Loading")
        {
            UnlockForUI();
        }
        else
        {
            LockForGameplay();
        }
    }

    public static void LockForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void UnlockForUI()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void EnsureGameplayLock(bool inputBlocked)
    {
        if (inputBlocked)
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
        {
            LockForGameplay();
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureRuntimeEnforcer();

        if (scene.name == "Scene_MainMenu" || scene.name == "Scene_Loading")
        {
            UnlockForUI();
        }
        else
        {
            LockForGameplay();
        }
    }

    private static void EnsureRuntimeEnforcer()
    {
        if (Object.FindFirstObjectByType<CursorLockRuntimeEnforcer>() != null)
        {
            return;
        }

        GameObject enforcer = new GameObject("CursorLockRuntimeEnforcer");
        Object.DontDestroyOnLoad(enforcer);
        enforcer.AddComponent<CursorLockRuntimeEnforcer>();
    }
}

public class CursorLockRuntimeEnforcer : MonoBehaviour
{
    private void Update()
    {
        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == "Scene_MainMenu" || activeScene == "Scene_Loading")
        {
            return;
        }

        bool inputBlocked = UIManager.Instance != null && UIManager.Instance.IsBlockingPlayerInput;
        CursorLockManager.EnsureGameplayLock(inputBlocked);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            return;
        }

        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == "Scene_MainMenu" || activeScene == "Scene_Loading")
        {
            return;
        }

        bool inputBlocked = UIManager.Instance != null && UIManager.Instance.IsBlockingPlayerInput;
        CursorLockManager.EnsureGameplayLock(inputBlocked);
    }
}
