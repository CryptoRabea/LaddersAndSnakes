using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles player setup UI for single player mode
/// Allows configuring number of human players and AI players
/// Optimized for mobile devices with larger touch targets and responsive layout
/// </summary>
public class PlayerSetupPanel : MonoBehaviour
{
    [Header("Mobile Optimization")]
    [SerializeField] private bool enableMobileOptimizations = true;
    [SerializeField] private float mobileButtonScaleMultiplier = 1.2f;
    [Header("UI References")]
    [SerializeField] private TMP_InputField humanPlayersInput;
    [SerializeField] private TMP_InputField aiPlayersInput;
    [SerializeField] private TextMeshProUGUI totalPlayersText;
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;

    [Header("Settings")]
    [SerializeField] private int maxTotalPlayers = 4;
    [SerializeField] private int minTotalPlayers = 1;
    [SerializeField] private string gameSceneName = "GameScene";

    private int humanPlayers = 1;
    private int aiPlayers = 1;

    void OnEnable()
    {
        // Apply mobile optimizations if on mobile device
        if (enableMobileOptimizations && Application.isMobilePlatform)
        {
            ApplyMobileOptimizations();
        }

        // Reset to default values when panel is shown
        ResetToDefaults();
        SetupListeners();
        UpdateUI();
    }

    /// <summary>
    /// Apply mobile-specific optimizations to player setup UI
    /// </summary>
    void ApplyMobileOptimizations()
    {
        Debug.Log("Applying mobile optimizations to player setup panel...");

        // Enlarge input fields for easier touch interaction
        if (humanPlayersInput != null)
        {
            RectTransform inputRect = humanPlayersInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            humanPlayersInput.textComponent.fontSize = Mathf.Max(humanPlayersInput.textComponent.fontSize, 36f);
        }

        if (aiPlayersInput != null)
        {
            RectTransform inputRect = aiPlayersInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            aiPlayersInput.textComponent.fontSize = Mathf.Max(aiPlayersInput.textComponent.fontSize, 36f);
        }

        // Enlarge buttons for better touch targets
        Button[] buttons = new Button[] { startGameButton, backButton };
        foreach (var button in buttons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.localScale *= mobileButtonScaleMultiplier;
                }

                LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.minHeight = 100f;
            }
        }

        // Enlarge text for better readability
        if (totalPlayersText != null)
        {
            totalPlayersText.fontSize *= 1.2f;
        }

        if (errorText != null)
        {
            errorText.fontSize *= 1.1f;
        }

        Debug.Log("Mobile optimizations applied to player setup panel");
    }

    void ResetToDefaults()
    {
        humanPlayers = 1;
        aiPlayers = 1;

        if (humanPlayersInput != null)
        {
            humanPlayersInput.text = humanPlayers.ToString();
        }

        if (aiPlayersInput != null)
        {
            aiPlayersInput.text = aiPlayers.ToString();
        }
    }

    void SetupListeners()
    {
        if (humanPlayersInput != null)
        {
            humanPlayersInput.onValueChanged.AddListener(OnHumanPlayersChanged);
        }

        if (aiPlayersInput != null)
        {
            aiPlayersInput.onValueChanged.AddListener(OnAIPlayersChanged);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    void OnHumanPlayersChanged(string value)
    {
        if (int.TryParse(value, out int players))
        {
            humanPlayers = Mathf.Max(0, players);
        }
        else
        {
            humanPlayers = 0;
        }

        UpdateUI();
    }

    void OnAIPlayersChanged(string value)
    {
        if (int.TryParse(value, out int players))
        {
            aiPlayers = Mathf.Max(0, players);
        }
        else
        {
            aiPlayers = 0;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        int totalPlayers = humanPlayers + aiPlayers;

        // Update total players text
        if (totalPlayersText != null)
        {
            totalPlayersText.text = $"Total Players: {totalPlayers}";
        }

        // Validate and show errors
        bool isValid = ValidatePlayerCount(totalPlayers);

        if (startGameButton != null)
        {
            startGameButton.interactable = isValid;
        }
    }

    bool ValidatePlayerCount(int totalPlayers)
    {
        // Clear previous error
        if (errorText != null)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }

        // Check if total is within valid range
        if (totalPlayers < minTotalPlayers)
        {
            ShowError($"Need at least {minTotalPlayers} player total!");
            return false;
        }

        if (totalPlayers > maxTotalPlayers)
        {
            ShowError($"Maximum {maxTotalPlayers} players allowed!");
            return false;
        }

        // Check if at least one player type is present
        if (humanPlayers == 0 && aiPlayers == 0)
        {
            ShowError("Need at least one human or AI player!");
            return false;
        }

        return true;
    }

    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }

    void OnStartGameClicked()
    {
        int totalPlayers = humanPlayers + aiPlayers;

        if (!ValidatePlayerCount(totalPlayers))
        {
            return;
        }

        Debug.Log($"Starting game with {humanPlayers} human players and {aiPlayers} AI players");

        // Store game configuration for the game scene
        GameConfiguration.Instance.SetSinglePlayerMode(humanPlayers, aiPlayers);

        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    void OnBackClicked()
    {
        // Return to main menu
        MainMenuController mainMenu = FindFirstObjectByType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.ReturnToMainMenu();
        }
    }

    void OnDisable()
    {
        // Clean up listeners
        if (humanPlayersInput != null)
        {
            humanPlayersInput.onValueChanged.RemoveAllListeners();
        }

        if (aiPlayersInput != null)
        {
            aiPlayersInput.onValueChanged.RemoveAllListeners();
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }
    }
}
