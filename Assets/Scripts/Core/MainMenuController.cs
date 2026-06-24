using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Settings UI")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Main Menu Navigation")]
    public Button[] mainMenuButtons;

    private int selectedMenuIndex;
    private readonly Color normalMenuColor = new Color(0.78f, 0.8f, 0.82f, 0.16f);
    private readonly Color selectedMenuColor = new Color(0.82f, 0.84f, 0.84f, 0.54f);

    private void Start()
    {
        // Play Main Menu background music if available
        AudioManager.EnsureInstance()?.FadeToAmbience("AMB_NguyenHue_Gloomy", 1.5f);

        // Make sure panels are active/inactive correctly
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Initialize sliders
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
            bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Show cursor so player can click buttons
        CursorLockManager.UnlockForUI();
        SelectMenuButton(0, false);
    }

    private void Update()
    {
        if (mainPanel == null || !mainPanel.activeInHierarchy || mainMenuButtons == null || mainMenuButtons.Length == 0)
        {
            return;
        }

        if (MenuUpPressed())
        {
            MoveSelection(-1);
        }
        else if (MenuDownPressed())
        {
            MoveSelection(1);
        }

        if (MenuSubmitPressed())
        {
            Button selectedButton = GetSelectedButton();
            if (selectedButton != null && selectedButton.interactable)
            {
                selectedButton.onClick.Invoke();
            }
        }
    }

    public void StartGame()
    {
        AudioManager.EnsureInstance()?.PlaySfx("SFX_MapSelect", 1.0f);
        // Reset progress on new game start
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgressForNewGame();
        }
        SceneLoader.Load(SceneLoader.BusHub);
    }

    public void OpenSettings()
    {
        AudioManager.EnsureInstance()?.PlaySfx("SFX_PuzzleButton", 0.85f);
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        AudioManager.EnsureInstance()?.PlaySfx("SFX_PuzzleButton", 0.85f);
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
        SelectMenuButton(selectedMenuIndex, false);
        PlayerPrefs.Save();
    }

    public void OpenCredits()
    {
        AudioManager.EnsureInstance()?.PlaySfx("SFX_PuzzleButton", 0.85f);
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        AudioManager.EnsureInstance()?.PlaySfx("SFX_PuzzleButton", 0.85f);
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
        SelectMenuButton(selectedMenuIndex, false);
    }

    public void QuitGame()
    {
        AudioManager.EnsureInstance()?.PlaySfx("SFX_BusDepart", 1.0f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetBGMVolume(float val)
    {
        PlayerPrefs.SetFloat("BGMVolume", val);
    }

    private void SetSFXVolume(float val)
    {
        PlayerPrefs.SetFloat("SFXVolume", val);
    }

    private void MoveSelection(int direction)
    {
        if (mainMenuButtons == null || mainMenuButtons.Length == 0)
        {
            return;
        }

        int nextIndex = selectedMenuIndex;
        for (int i = 0; i < mainMenuButtons.Length; i++)
        {
            nextIndex = (nextIndex + direction + mainMenuButtons.Length) % mainMenuButtons.Length;
            if (mainMenuButtons[nextIndex] != null && mainMenuButtons[nextIndex].interactable)
            {
                SelectMenuButton(nextIndex, true);
                return;
            }
        }
    }

    private void SelectMenuButton(int index, bool playSound)
    {
        if (mainMenuButtons == null || mainMenuButtons.Length == 0)
        {
            return;
        }

        selectedMenuIndex = Mathf.Clamp(index, 0, mainMenuButtons.Length - 1);
        Button selectedButton = GetSelectedButton();
        if (selectedButton == null)
        {
            return;
        }

        UpdateMenuButtonVisuals();
        EventSystem.current?.SetSelectedGameObject(null);

        if (playSound)
        {
            AudioManager.EnsureInstance()?.PlaySfx("SFX_ItemCollect_Memory", 0.55f);
        }
    }

    private Button GetSelectedButton()
    {
        if (mainMenuButtons == null || selectedMenuIndex < 0 || selectedMenuIndex >= mainMenuButtons.Length)
        {
            return null;
        }

        return mainMenuButtons[selectedMenuIndex];
    }

    private void UpdateMenuButtonVisuals()
    {
        if (mainMenuButtons == null)
        {
            return;
        }

        for (int i = 0; i < mainMenuButtons.Length; i++)
        {
            if (mainMenuButtons[i] == null)
            {
                continue;
            }

            Image image = mainMenuButtons[i].targetGraphic as Image;
            if (image != null)
            {
                image.color = i == selectedMenuIndex ? selectedMenuColor : normalMenuColor;
            }
        }
    }

    private static bool MenuUpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#else
        return false;
#endif
    }

    private static bool MenuDownPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame))
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
#else
        return false;
#endif
    }

    private static bool MenuSubmitPressed()
    {
        if (GameInput.SubmitPressed || GameInput.InteractPressed)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space);
#else
        return false;
#endif
    }
}
