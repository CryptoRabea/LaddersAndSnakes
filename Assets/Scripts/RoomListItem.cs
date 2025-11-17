using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Component for individual room list item UI
/// This script helps identify the required UI elements for room listing
/// </summary>
public class RoomListItem : MonoBehaviour
{
    [Header("Required UI Elements")]
    [Tooltip("Text component to display room name")]
    public TextMeshProUGUI RoomNameText;

    [Tooltip("Text component to display player count (e.g., '2/4')")]
    public TextMeshProUGUI PlayerCountText;

    [Tooltip("Button to join this room")]
    public Button JoinButton;

    void Awake()
    {
        // Auto-find components if not assigned
        if (RoomNameText == null)
        {
            var nameTextObj = transform.Find("RoomNameText");
            if (nameTextObj != null)
            {
                RoomNameText = nameTextObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (PlayerCountText == null)
        {
            var countTextObj = transform.Find("PlayerCountText");
            if (countTextObj != null)
            {
                PlayerCountText = countTextObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (JoinButton == null)
        {
            var buttonObj = transform.Find("JoinButton");
            if (buttonObj != null)
            {
                JoinButton = buttonObj.GetComponent<Button>();
            }
        }
    }

    /// <summary>
    /// Initialize the room item with session data
    /// </summary>
    public void Initialize(string roomName, int currentPlayers, int maxPlayers, System.Action onJoinClicked)
    {
        if (RoomNameText != null)
        {
            RoomNameText.text = roomName;
        }

        if (PlayerCountText != null)
        {
            PlayerCountText.text = $"{currentPlayers}/{maxPlayers}";
        }

        if (JoinButton != null)
        {
            bool isFull = currentPlayers >= maxPlayers;
            JoinButton.interactable = !isFull;

            if (isFull)
            {
                var buttonText = JoinButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Full";
                }
            }
            else
            {
                JoinButton.onClick.RemoveAllListeners();
                JoinButton.onClick.AddListener(() => onJoinClicked?.Invoke());
            }
        }
    }

    void OnDestroy()
    {
        if (JoinButton != null)
        {
            JoinButton.onClick.RemoveAllListeners();
        }
    }
}
