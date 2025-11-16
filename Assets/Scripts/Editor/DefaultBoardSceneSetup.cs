using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LAS.Config;
using LAS.Entities;

namespace LaddersAndSnakes.Editor
{
    /// <summary>
    /// Editor utility to setup the game scene with a default board
    /// </summary>
    public class DefaultBoardSceneSetup
    {
        private const string CONFIG_PATH = "Assets/Config/DefaultBoardConfig.asset";

        [MenuItem("LAS/Setup Default Board in Scene")]
        public static void SetupDefaultBoard()
        {
            // First, ensure we have a BoardConfig asset
            BoardConfig config = AssetDatabase.LoadAssetAtPath<BoardConfig>(CONFIG_PATH);
            if (config == null)
            {
                Debug.Log("BoardConfig not found. Creating default config...");
                BoardConfigGenerator.CreateDefaultBoardConfig();
                config = AssetDatabase.LoadAssetAtPath<BoardConfig>(CONFIG_PATH);
            }

            // Find or create BoardGenerator
            BoardGenerator generator = Object.FindFirstObjectByType<BoardGenerator>();
            if (generator == null)
            {
                GameObject generatorObj = new GameObject("BoardGenerator");
                generator = generatorObj.AddComponent<BoardGenerator>();
                Undo.RegisterCreatedObjectUndo(generatorObj, "Create BoardGenerator");
                Debug.Log("Created BoardGenerator GameObject");
            }

            // Generate the board
            generator.GenerateBoard();
            Debug.Log("Board generated!");

            // Find or create BoardManager
            BoardManager boardManager = Object.FindFirstObjectByType<BoardManager>();
            if (boardManager == null)
            {
                GameObject managerObj = new GameObject("BoardManager");
                boardManager = managerObj.AddComponent<BoardManager>();
                Undo.RegisterCreatedObjectUndo(managerObj, "Create BoardManager");
                Debug.Log("Created BoardManager GameObject");
            }

            // Configure BoardManager
            Transform boardParent = generator.transform.Find("BoardParent");
            if (boardParent != null)
            {
                // Use reflection to set the private fields
                var boardParentField = typeof(BoardManager).GetField("boardParent",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var boardConfigField = typeof(BoardManager).GetField("boardConfig",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (boardParentField != null)
                {
                    boardParentField.SetValue(boardManager, boardParent);
                    Debug.Log("BoardManager.boardParent assigned");
                }

                if (boardConfigField != null)
                {
                    boardConfigField.SetValue(boardManager, config);
                    Debug.Log("BoardManager.boardConfig assigned");
                }

                EditorUtility.SetDirty(boardManager);
            }

            // Find or create BoardModel
            BoardModel boardModel = Object.FindFirstObjectByType<BoardModel>();
            if (boardModel == null)
            {
                GameObject modelObj = new GameObject("BoardModel");
                boardModel = modelObj.AddComponent<BoardModel>();
                Undo.RegisterCreatedObjectUndo(modelObj, "Create BoardModel");
                Debug.Log("Created BoardModel GameObject");
            }

            // Configure BoardModel
            var configField = typeof(BoardModel).GetField("config",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (configField != null)
            {
                configField.SetValue(boardModel, config);
                EditorUtility.SetDirty(boardModel);
                Debug.Log("BoardModel.config assigned");
            }

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("=== Board Setup Complete! ===\n" +
                     $"Board has 100 tiles, 8 snakes, and 7 ladders.\n" +
                     "BoardManager and BoardModel are configured with the default config.");

            // Select the generator for easy access
            Selection.activeGameObject = generator.gameObject;
        }

        [MenuItem("LAS/Clear Board from Scene")]
        public static void ClearBoard()
        {
            BoardGenerator generator = Object.FindFirstObjectByType<BoardGenerator>();
            if (generator != null)
            {
                if (EditorUtility.DisplayDialog("Clear Board",
                    "Are you sure you want to remove the BoardGenerator and all generated tiles?",
                    "Yes", "No"))
                {
                    Undo.DestroyObjectImmediate(generator.gameObject);
                    Debug.Log("BoardGenerator removed from scene");
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            else
            {
                Debug.Log("No BoardGenerator found in scene");
            }
        }
    }
}
