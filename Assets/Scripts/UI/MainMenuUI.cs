using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace LAS.UI
{
    /// <summary>
    /// Main menu UI controller
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject playerSetupPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Player Setup")]
        [SerializeField] private TMP_Dropdown playerCountDropdown;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Button settingsBackButton;

        [Header("Game Scene")]
        [SerializeField] private string gameSceneName = "GameScene";

        private int selectedPlayerCount = 2;

        private void Start()
        {
            // Setup button listeners
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);

            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(OnSettingsBackClicked);

            if (playerCountDropdown != null)
                playerCountDropdown.onValueChanged.AddListener(OnPlayerCountChanged);

            // Show main panel
            ShowMainPanel();
        }

        private void OnPlayClicked()
        {
            ShowPlayerSetupPanel();
        }

        private void OnSettingsClicked()
        {
            ShowSettingsPanel();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnStartGameClicked()
        {
            // Save player count for game scene
            PlayerPrefs.SetInt("PlayerCount", selectedPlayerCount);
            PlayerPrefs.Save();

            // Load game scene
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnBackClicked()
        {
            ShowMainPanel();
        }

        private void OnSettingsBackClicked()
        {
            ShowMainPanel();
        }

        private void OnPlayerCountChanged(int value)
        {
            selectedPlayerCount = value + 2; // Dropdown values 0-2 = 2-4 players
        }

        private void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (playerSetupPanel != null) playerSetupPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void ShowPlayerSetupPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (playerSetupPanel != null) playerSetupPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void ShowSettingsPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (playerSetupPanel != null) playerSetupPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);

            // Load saved settings
            if (musicToggle != null)
                musicToggle.isOn = PlayerPrefs.GetInt("Music", 1) == 1;

            if (sfxToggle != null)
                sfxToggle.isOn = PlayerPrefs.GetInt("SFX", 1) == 1;
        }

        public void OnMusicToggled(bool value)
        {
            PlayerPrefs.SetInt("Music", value ? 1 : 0);
            // TODO: Implement audio manager
        }

        public void OnSFXToggled(bool value)
        {
            PlayerPrefs.SetInt("SFX", value ? 1 : 0);
            // TODO: Implement audio manager
        }
    }
}
