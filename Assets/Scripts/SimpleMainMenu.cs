using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Simple main menu for starting the game
/// </summary>
public class SimpleMainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button play2PlayerButton;
    [SerializeField] private Button play3PlayerButton;
    [SerializeField] private Button play4PlayerButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    private int selectedPlayerCount = 2;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        if (titleText != null)
        {
            titleText.text = "Snakes & Ladders";
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(() => StartGame(2));
        }

        if (play2PlayerButton != null)
        {
            play2PlayerButton.onClick.AddListener(() => StartGame(2));
        }

        if (play3PlayerButton != null)
        {
            play3PlayerButton.onClick.AddListener(() => StartGame(3));
        }

        if (play4PlayerButton != null)
        {
            play4PlayerButton.onClick.AddListener(() => StartGame(4));
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void StartGame(int playerCount)
    {
        // Store player count for the game scene to use
        PlayerPrefs.SetInt("PlayerCount", playerCount);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }

    void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
        if (play2PlayerButton != null)
            play2PlayerButton.onClick.RemoveAllListeners();
        if (play3PlayerButton != null)
            play3PlayerButton.onClick.RemoveAllListeners();
        if (play4PlayerButton != null)
            play4PlayerButton.onClick.RemoveAllListeners();
        if (quitButton != null)
            quitButton.onClick.RemoveAllListeners();
    }
}
