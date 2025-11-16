#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using LAS.UI;

namespace LAS.Editor
{
    /// <summary>
    /// Quick one-click UI setup tool for adding RuntimeUIBuilder to GameCanvas
    /// </summary>
    public class QuickUISetup
    {
        [MenuItem("LAS/Quick UI Setup (Add RuntimeUIBuilder)")]
        public static void SetupRuntimeUIBuilder()
        {
            // Find GameCanvas in the scene
            var canvas = GameObject.Find("GameCanvas");

            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "GameCanvas not found in the scene. Please create a Canvas named 'GameCanvas' first.",
                    "OK");
                return;
            }

            // Add RuntimeUIBuilder component if it doesn't exist
            var builder = canvas.GetComponent<RuntimeUIBuilder>();
            if (builder == null)
            {
                builder = canvas.AddComponent<RuntimeUIBuilder>();
                Debug.Log("[QuickUISetup] Added RuntimeUIBuilder to GameCanvas");
            }
            else
            {
                Debug.Log("[QuickUISetup] RuntimeUIBuilder already exists on GameCanvas");
            }

            // Mark scene as dirty to save changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Setup Complete",
                "RuntimeUIBuilder has been added to GameCanvas!\n\n" +
                "The UI will now be automatically created when you run the game.\n" +
                "Press Play to test it out!",
                "OK");
        }

        [MenuItem("LAS/Force Build UI Now (Editor)")]
        public static void ForceBuildUI()
        {
            var canvas = GameObject.Find("GameCanvas");

            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "GameCanvas not found in the scene.",
                    "OK");
                return;
            }

            var builder = canvas.GetComponent<RuntimeUIBuilder>();
            if (builder == null)
            {
                builder = canvas.AddComponent<RuntimeUIBuilder>();
            }

            // Force build UI immediately in editor
            builder.BuildUI();

            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("[QuickUISetup] UI built successfully!");

            EditorUtility.DisplayDialog("Success",
                "UI has been built!\n\n" +
                "You should now see:\n" +
                "- RollDiceButton\n" +
                "- TurnIndicator\n" +
                "- DiceResultText\n" +
                "- GameOverPanel\n\n" +
                "Check the GameCanvas hierarchy in the Scene view.",
                "OK");
        }
    }
}
#endif
