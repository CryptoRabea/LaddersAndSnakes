using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

    /// <summary>
    /// Main menu controller for game setup and multiplayer options
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject multiplayerPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button playLocalButton;
        [SerializeField] private Button playAIButton;
        [SerializeField] private Button playOnlineButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;


        [Header("Multiplayer Options")]
        [SerializeField] private Button hostGameButton;
        [SerializeField] private Button joinGameButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_InputField serverAddressInput;
        [SerializeField] private TMP_Dropdown playerCountDropdown;

        [Header("Settings")]
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private Button settingsBackButton;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            ShowMainPanel();
        }

        private void InitializeUI()
        {
            // Initialize dropdowns
            if (playerCountDropdown != null)
            {
                playerCountDropdown.ClearOptions();
                playerCountDropdown.AddOptions(new System.Collections.Generic.List<string> { "2 Players", "3 Players", "4 Players" });
                playerCountDropdown.value = 0;
            }

            if (serverAddressInput != null)
            {
                serverAddressInput.text = "127.0.0.1";
            }
        }

        private void SetupEventListeners()
        {
            if (playLocalButton != null)
                playLocalButton.onClick.AddListener(OnPlayLocal);

            if (playAIButton != null)
                playAIButton.onClick.AddListener(OnPlayAI);

            if (playOnlineButton != null)
                playOnlineButton.onClick.AddListener(OnPlayOnline);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);

            if (hostGameButton != null)
                hostGameButton.onClick.AddListener(OnHostGame);

            if (joinGameButton != null)
                joinGameButton.onClick.AddListener(OnJoinGame);

            if (backButton != null)
                backButton.onClick.AddListener(OnBack);

            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(OnBack);
        }

        private void ShowMainPanel()
        {
            SetPanelActive(mainPanel, true);
            SetPanelActive(multiplayerPanel, false);
            SetPanelActive(settingsPanel, false);
        }

        private void ShowMultiplayerPanel()
        {
            SetPanelActive(mainPanel, false);
            SetPanelActive(multiplayerPanel, true);
            SetPanelActive(settingsPanel, false);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        private void OnPlayLocal()
        {
            Debug.Log("[MainMenu] Starting local multiplayer");
            int playerCount = playerCountDropdown != null ? playerCountDropdown.value + 2 : 2;

           
            LoadGameScene();
        }

    private void OnPlayAI()
    {
        Debug.Log("[MainMenu] Starting single player vs AI");

    }

        private void OnPlayOnline()
        {
            Debug.Log("[MainMenu] Opening online multiplayer menu");
            ShowMultiplayerPanel();
        }

        private void OnHostGame()
        {
            Debug.Log("[MainMenu] Hosting game");

            // Ensure NetworkManager exists
           
            LoadGameScene();
        }

        private void OnJoinGame()
        {
            string serverAddress = serverAddressInput != null ? serverAddressInput.text : "127.0.0.1";
            Debug.Log($"[MainMenu] Joining game at {serverAddress}");

            // Ensure NetworkManager exists
            
            LoadGameScene();
        }

    public void ReturnToMainMenu()
    {
        Debug.Log($"[MainMenu] Loading scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Opening settings");
            SetPanelActive(mainPanel, false);
            SetPanelActive(multiplayerPanel, false);
            SetPanelActive(settingsPanel, true);
        }

        private void OnBack()
        {
            Debug.Log("[MainMenu] Back to main menu");
            ShowMainPanel();
        }

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quitting game");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadGameScene()
        {
            Debug.Log($"[MainMenu] Loading scene: {gameSceneName}");
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (playLocalButton != null)
                playLocalButton.onClick.RemoveListener(OnPlayLocal);

            if (playAIButton != null)
                playAIButton.onClick.RemoveListener(OnPlayAI);

            if (playOnlineButton != null)
                playOnlineButton.onClick.RemoveListener(OnPlayOnline);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettings);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuit);

            if (hostGameButton != null)
                hostGameButton.onClick.RemoveListener(OnHostGame);

            if (joinGameButton != null)
                joinGameButton.onClick.RemoveListener(OnJoinGame);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBack);

            if (settingsBackButton != null)
                settingsBackButton.onClick.RemoveListener(OnBack);
        }
    }

