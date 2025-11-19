#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Unity Editor helper to quickly add game scene UI elements
/// </summary>
public class GameSceneUISetupEditor : EditorWindow
{
    [MenuItem("Ladders & Snakes/Setup Game Scene UI")]
    public static void SetupGameSceneUI()
    {
        // Check if we're in the game scene
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "GameScene")
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Not in GameScene",
                $"Current scene is '{scene.name}'. This setup is designed for GameScene. Continue anyway?",
                "Yes", "No"
            );

            if (!proceed)
                return;
        }

        // Find or create setup object
        GameSceneUISetup setup = FindObjectOfType<GameSceneUISetup>();

        if (setup == null)
        {
            // Create new GameObject with the setup component
            GameObject setupObj = new GameObject("GameSceneUI");
            setup = setupObj.AddComponent<GameSceneUISetup>();

            Debug.Log("[Editor] Created GameSceneUI GameObject with setup component");
        }

        // Trigger the setup
        setup.SendMessage("SetupUIFromEditor", SendMessageOptions.DontRequireReceiver);

        // Mark scene as dirty so it can be saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);

        EditorUtility.DisplayDialog(
            "Setup Complete",
            "Game Scene UI has been set up successfully!\n\n" +
            "Added:\n" +
            "- Main Menu button (top-left)\n" +
            "- Settings button (top-right)\n" +
            "- Settings panel with Surrender button\n\n" +
            "The GameSceneUI GameObject has been created with the GameSceneUISetup component.\n\n" +
            "Don't forget to save your scene!",
            "OK"
        );
    }

    [MenuItem("Ladders & Snakes/About Game Scene UI")]
    public static void ShowAbout()
    {
        EditorUtility.DisplayDialog(
            "Game Scene UI Features",
            "Main Menu Button:\n" +
            "- Returns to main menu from game scene\n" +
            "- Located in top-left corner\n\n" +
            "Settings Button:\n" +
            "- Opens settings panel\n" +
            "- Located in top-right corner\n\n" +
            "Settings Panel:\n" +
            "- Surrender button to quit current game\n" +
            "- For local/AI games: returns to main menu\n" +
            "- For online games: leaves room or closes if last player\n" +
            "- Close button to return to game\n\n" +
            "Scripts:\n" +
            "- GameSceneUISetup.cs: Creates UI elements\n" +
            "- SettingsPanelController.cs: Handles all functionality",
            "OK"
        );
    }
}
#endif
