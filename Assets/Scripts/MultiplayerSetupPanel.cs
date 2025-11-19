using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles multiplayer setup UI
/// Allows player to host or join a game
/// Now includes room listing functionality
/// Optimized for mobile devices with responsive layout
/// </summary>
public class MultiplayerSetupPanel : MonoBehaviour
{
    [Header("Mobile Optimization")]
    [SerializeField] private bool enableMobileOptimizations = true;
    [SerializeField] private float mobileButtonScaleMultiplier = 1.2f;
    [Header("UI References")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;

    [Header("Room Listing")]
    [SerializeField] private GameObject roomListingPanel;
    [SerializeField] private GameObject manualJoinPanel;
    [SerializeField] private Button showRoomListButton;
    [SerializeField] private Button showManualJoinButton;
    [SerializeField] private RoomListingManager roomListingManager;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private int defaultMaxPlayers = 4;

    private string roomName = "LaddersAndSnakes";
    private int maxPlayers = 4;

    void OnEnable()
    {
        // Apply mobile optimizations if on mobile device
        if (enableMobileOptimizations && Application.isMobilePlatform)
        {
            ApplyMobileOptimizations();
        }

        ResetToDefaults();
        SetupListeners();
        UpdateUI();
    }

    /// <summary>
    /// Apply mobile-specific optimizations to multiplayer setup UI
    /// </summary>
    void ApplyMobileOptimizations()
    {
        Debug.Log("Applying mobile optimizations to multiplayer setup panel...");

        // Enlarge input fields for easier touch interaction
        if (roomNameInput != null)
        {
            RectTransform inputRect = roomNameInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            roomNameInput.textComponent.fontSize = Mathf.Max(roomNameInput.textComponent.fontSize, 36f);
        }

        if (maxPlayersInput != null)
        {
            RectTransform inputRect = maxPlayersInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            maxPlayersInput.textComponent.fontSize = Mathf.Max(maxPlayersInput.textComponent.fontSize, 36f);
        }

        // Enlarge buttons for better touch targets
        Button[] buttons = new Button[]
        {
            hostButton, joinButton, backButton,
            showRoomListButton, showManualJoinButton
        };

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

        // Enlarge status text
        if (statusText != null)
        {
            statusText.fontSize *= 1.2f;
        }

        Debug.Log("Mobile optimizations applied to multiplayer setup panel");
    }

    void ResetToDefaults()
    {
        roomName = "LaddersAndSnakes";
        maxPlayers = defaultMaxPlayers;

        if (roomNameInput != null)
        {
            roomNameInput.text = roomName;
        }

        if (maxPlayersInput != null)
        {
            maxPlayersInput.text = maxPlayers.ToString();
        }

        if (statusText != null)
        {
            statusText.text = "Choose to Host or Join a game";
        }
    }

    void SetupListeners()
    {
        if (roomNameInput != null)
        {
            roomNameInput.onValueChanged.AddListener(OnRoomNameChanged);
        }

        if (maxPlayersInput != null)
        {
            maxPlayersInput.onValueChanged.AddListener(OnMaxPlayersChanged);
        }

        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostClicked);
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (showRoomListButton != null)
        {
            showRoomListButton.onClick.AddListener(ShowRoomList);
        }

        if (showManualJoinButton != null)
        {
            showManualJoinButton.onClick.AddListener(ShowManualJoin);
        }

        // Show room list by default
        ShowRoomList();
    }

    void OnRoomNameChanged(string value)
    {
        roomName = string.IsNullOrWhiteSpace(value) ? "LaddersAndSnakes" : value.Trim();
        UpdateUI();
    }

    void OnMaxPlayersChanged(string value)
    {
        if (int.TryParse(value, out int players))
        {
            maxPlayers = Mathf.Clamp(players, 2, 8);
        }
        else
        {
            maxPlayers = defaultMaxPlayers;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // Validate room name
        bool isValid = !string.IsNullOrWhiteSpace(roomName) && maxPlayers >= 2;

        if (hostButton != null)
        {
            hostButton.interactable = isValid;
        }

        if (joinButton != null)
        {
            joinButton.interactable = isValid;
        }

        if (statusText != null && !isValid)
        {
            statusText.text = "Please enter a valid room name and max players (2-8)";
        }
    }

    void OnHostClicked()
    {
        Debug.Log($"Hosting game: Room={roomName}, MaxPlayers={maxPlayers}");

        if (statusText != null)
        {
            statusText.text = "Starting as Host...";
        }

        // Configure game for multiplayer
        GameConfiguration.Instance.SetMultiplayerMode(true, roomName, maxPlayers);

        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    void OnJoinClicked()
    {
        Debug.Log($"Joining game: Room={roomName}");

        if (statusText != null)
        {
            statusText.text = "Joining as Client...";
        }

        // Configure game for multiplayer
        GameConfiguration.Instance.SetMultiplayerMode(false, roomName, maxPlayers);

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

    void ShowRoomList()
    {
        if (roomListingPanel != null)
        {
            roomListingPanel.SetActive(true);
        }

        if (manualJoinPanel != null)
        {
            manualJoinPanel.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text = "Browse available rooms or create your own";
        }

        Debug.Log("Showing room list view");
    }

    void ShowManualJoin()
    {
        if (roomListingPanel != null)
        {
            roomListingPanel.SetActive(false);
        }

        if (manualJoinPanel != null)
        {
            manualJoinPanel.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.text = "Enter room name to join";
        }

        Debug.Log("Showing manual join view");
    }

    void OnDisable()
    {
        // Clean up listeners
        if (roomNameInput != null)
        {
            roomNameInput.onValueChanged.RemoveAllListeners();
        }

        if (maxPlayersInput != null)
        {
            maxPlayersInput.onValueChanged.RemoveAllListeners();
        }

        if (hostButton != null)
        {
            hostButton.onClick.RemoveAllListeners();
        }

        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }

        if (showRoomListButton != null)
        {
            showRoomListButton.onClick.RemoveAllListeners();
        }

        if (showManualJoinButton != null)
        {
            showManualJoinButton.onClick.RemoveAllListeners();
        }
    }
}
