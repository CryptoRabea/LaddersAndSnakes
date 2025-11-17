using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu UI and navigation
/// Handles switching between single player and multiplayer modes
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playerSetupPanel;
    [SerializeField] private GameObject multiplayerPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        SetupMainMenu();
    }

    void SetupMainMenu()
    {
        // Show main menu, hide other panels
        ShowMainMenu();

        // Setup button listeners
        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
        }

        if (multiplayerButton != null)
        {
            multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (playerSetupPanel != null) playerSetupPanel.SetActive(false);
        if (multiplayerPanel != null) multiplayerPanel.SetActive(false);
    }

    void OnSinglePlayerClicked()
    {
        Debug.Log("Single Player mode selected");

        // Hide main menu, show player setup panel
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (playerSetupPanel != null) playerSetupPanel.SetActive(true);
    }

    void OnMultiplayerClicked()
    {
        Debug.Log("Multiplayer mode selected");

        // Hide main menu, show multiplayer panel
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (multiplayerPanel != null) multiplayerPanel.SetActive(true);
    }

    void OnQuitClicked()
    {
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Called by other panels to return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        ShowMainMenu();
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (singlePlayerButton != null) singlePlayerButton.onClick.RemoveAllListeners();
        if (multiplayerButton != null) multiplayerButton.onClick.RemoveAllListeners();
        if (quitButton != null) quitButton.onClick.RemoveAllListeners();
    }
}
