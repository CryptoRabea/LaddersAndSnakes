using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LAS.Core;

namespace LAS.Examples
{
    /// <summary>
    /// Example demonstrating different ways to use the UI Reference Tool
    /// This shows various attribute configurations and best practices
    /// </summary>
    public class ExampleUIManager : MonoBehaviour
    {
        // ========== BASIC USAGE ==========

        // Simple reference - searches for GameObject named "PlayButton"
        [UIReference]
        private Button playButton;

        // Simple reference - searches for GameObject named "TitleText"
        [UIReference]
        private TextMeshProUGUI titleText;

        // ========== CUSTOM NAMES ==========

        // Field name doesn't match GameObject name
        // Searches for GameObject named "BtnStart"
        [UIReference(Name = "BtnStart")]
        private Button startButton;

        // ========== PATH-BASED REFERENCES ==========

        // Direct hierarchy path
        [UIReference(Path = "Canvas/Header/Logo")]
        private Image logoImage;

        // Nested UI elements
        [UIReference(Path = "Canvas/MainPanel/Content/Description")]
        private TextMeshProUGUI descriptionText;

        // ========== OPTIONAL REFERENCES ==========

        // Won't error if not found
        [UIReference(Required = false)]
        private GameObject debugPanel;

        // Optional with custom path
        [UIReference(Path = "Canvas/OptionsPanel", Required = false)]
        private GameObject optionsPanel;

        // ========== SCENE-WIDE SEARCH ==========

        // Search entire scene instead of just children
        [UIReference(SearchInChildren = false)]
        private Camera mainCamera;

        // ========== TAG-BASED SEARCH ==========

        // Only bind to GameObjects with specific tag
        [UIReference(Tag = "Player")]
        private GameObject player;

        // ========== UI ROOT ==========

        // Define a custom search root for all references below
        [UIRoot]
        [SerializeField] private Transform canvasTransform;

        // These will search starting from canvasTransform
        [UIReference]
        private Button settingsButton;

        [UIReference]
        private Button quitButton;

        // ========== MIXED WITH SERIALIZED FIELD ==========

        // Keep SerializeField for Inspector visibility and manual assignment option
        [UIReference]
        [SerializeField] private Button confirmButton;

        // ========== LIFECYCLE ==========

        private void Awake()
        {
            // OPTION 1: Manual binding at runtime
            // Uncomment if you want to bind at runtime instead of using Editor
            // UIReferenceBinder.BindUIReferences(this);

            // OPTION 2: Add UIReferenceAutoBinding component
            // This component will automatically bind references
            // No code needed here!

            // OPTION 3: Bind in Editor using the UI Reference Tool
            // Window > Ladders & Snakes > UI Reference Tool > Bind All
            // This is the recommended approach for best performance
        }

        private void Start()
        {
            // Setup UI listeners
            SetupUIListeners();
        }

        private void SetupUIListeners()
        {
            // All references should be bound by now
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Update text
            if (titleText != null)
                titleText.text = "Example UI Manager";

            if (descriptionText != null)
                descriptionText.text = "This is an example of the UI Reference Tool";

            // Optional references - check if they exist
            if (debugPanel != null)
                debugPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);

            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        // ========== UI CALLBACKS ==========

        private void OnPlayClicked()
        {
            Debug.Log("Play button clicked!");
        }

        private void OnStartClicked()
        {
            Debug.Log("Start button clicked!");
        }

        private void OnConfirmClicked()
        {
            Debug.Log("Confirm button clicked!");
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked!");
            if (optionsPanel != null)
                optionsPanel.SetActive(!optionsPanel.activeSelf);
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit button clicked!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ========== VALIDATION HELPER ==========

#if UNITY_EDITOR
        [ContextMenu("Validate References")]
        private void ValidateReferences()
        {
            UIReferenceBinder.ValidationResult result = new UIReferenceBinder.ValidationResult();
            UIReferenceBinder.ValidateComponent(this, result);

            Debug.Log($"Validation: {result.Success.Count} OK, {result.Warnings.Count} warnings, {result.Errors.Count} errors");

            if (result.HasErrors)
            {
                foreach (var error in result.Errors)
                {
                    Debug.LogError($"Missing: {error.FieldName}");
                }
            }
        }

        [ContextMenu("Bind References")]
        private void BindReferences()
        {
            int count = UIReferenceBinder.BindUIReferences(this, true);
            Debug.Log($"Bound {count} references");
        }
#endif
    }
}
