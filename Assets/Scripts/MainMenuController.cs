using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main menu controller for game setup and multiplayer options
/// Optimized for mobile devices with responsive UI and touch-friendly controls
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Mobile Optimization")]
    [SerializeField] private bool enableMobileOptimizations = true;
    [SerializeField] private float mobileButtonScaleMultiplier = 1.2f;
    [SerializeField] private SafeAreaHandler safeAreaHandler;
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
        // Apply mobile optimizations if on mobile device
        if (enableMobileOptimizations && Application.isMobilePlatform)
        {
            ApplyMobileOptimizations();
        }

        InitializeUI();
        SetupEventListeners();
        ShowMainPanel();
    }

    /// <summary>
    /// Apply mobile-specific optimizations to menu buttons and UI
    /// </summary>
    private void ApplyMobileOptimizations()
    {
        Debug.Log("Applying mobile optimizations to main menu...");

        // Enlarge all buttons for touch-friendly interaction
        Button[] allButtons = new Button[]
        {
            playLocalButton, playAIButton, playOnlineButton, settingsButton, quitButton,
            hostGameButton, joinGameButton, backButton, settingsBackButton
        };

        foreach (var button in allButtons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.localScale *= mobileButtonScaleMultiplier;
                }

                // Increase button minimum size for better touch targets
                LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.minHeight = 100f; // Minimum 100 pixels for touch
            }
        }

        // Enlarge input fields for easier typing on mobile
        if (serverAddressInput != null)
        {
            RectTransform inputRect = serverAddressInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            serverAddressInput.textComponent.fontSize = Mathf.Max(serverAddressInput.textComponent.fontSize, 32f);
        }

        // Enlarge dropdown for easier selection
        if (playerCountDropdown != null)
        {
            RectTransform dropdownRect = playerCountDropdown.GetComponent<RectTransform>();
            if (dropdownRect != null)
            {
                dropdownRect.sizeDelta = new Vector2(dropdownRect.sizeDelta.x, Mathf.Max(dropdownRect.sizeDelta.y, 80f));
            }
        }

        Debug.Log("Mobile optimizations applied to main menu");
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

            // Configure for local multiplayer (all human players)
            GameConfiguration.Instance.SetSinglePlayerMode(playerCount, 0);
            LoadGameScene();
        }

        private void OnPlayAI()
        {
            Debug.Log("[MainMenu] Starting single player vs AI");

            // Configure for single player with 1 human and 1 AI
            GameConfiguration.Instance.SetSinglePlayerMode(1, 1);
            LoadGameScene();
        }

        private void OnPlayOnline()
        {
            Debug.Log("[MainMenu] Opening online multiplayer menu");
            ShowMultiplayerPanel();
        }

        private void OnHostGame()
        {
            Debug.Log("[MainMenu] Hosting game");

            // Configure for multiplayer as host
            int maxPlayers = playerCountDropdown != null ? playerCountDropdown.value + 2 : 2;
            GameConfiguration.Instance.SetMultiplayerMode(isHost: true, maxPlayers: maxPlayers);

            LoadGameScene();
        }

        private void OnJoinGame()
        {
            string serverAddress = serverAddressInput != null ? serverAddressInput.text : "127.0.0.1";
            Debug.Log($"[MainMenu] Joining game at {serverAddress}");

            // Configure for multiplayer as client
            int maxPlayers = playerCountDropdown != null ? playerCountDropdown.value + 2 : 2;
            GameConfiguration.Instance.SetMultiplayerMode(isHost: false, maxPlayers: maxPlayers);

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

