using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Quick setup menu for common development tasks
/// </summary>
public class QuickSetupMenu : MonoBehaviour
{
    [MenuItem("Tools/Snakes and Ladders/Quick Setup/Complete Setup (All-in-One)", false, 1)]
    public static void CompleteSetup()
    {
        if (EditorUtility.DisplayDialog(
            "Complete Game Setup",
            "This will perform a complete setup:\n\n" +
            "1. Create all directories\n" +
            "2. Create ScriptableObject configs\n" +
            "3. Create player prefabs\n" +
            "4. Build complete GameScene\n" +
            "5. Setup build settings\n\n" +
            "This is the recommended option for first-time setup.\n\n" +
            "Continue?",
            "Yes, Setup Everything!",
            "Cancel"))
        {
            SceneBuilder.BuildCompleteScene();
            SetupBuildSettings();
        }
    }

    [MenuItem("Tools/Snakes and Ladders/Quick Setup/1. Create Directories", false, 100)]
    public static void CreateDirectories()
    {
        string[] directories = {
            "Assets/Resources",
            "Assets/Prefabs",
            "Assets/Scenes",
            "Assets/ScriptableObjects",
            "Assets/Materials",
            "Assets/Animations"
        };

        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Debug.Log($"Created directory: {dir}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "All directories created successfully!", "OK");
    }

    [MenuItem("Tools/Snakes and Ladders/Quick Setup/2. Create ScriptableObjects Only", false, 101)]
    public static void CreateScriptableObjectsOnly()
    {
        CreateGameConfig();
        CreateDiceConfig();
        CreateBoardConfig();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "ScriptableObjects created successfully!", "OK");
    }

    [MenuItem("Tools/Snakes and Ladders/Quick Setup/3. Create Player Prefabs Only", false, 102)]
    public static void CreatePlayerPrefabsOnly()
    {
        CreateDirectories();
        CreatePlayerPrefabs();
        EditorUtility.DisplayDialog("Success", "Player prefabs created successfully!", "OK");
    }

    [MenuItem("Tools/Snakes and Ladders/Quick Setup/4. Setup Build Settings", false, 103)]
    public static void SetupBuildSettings()
    {
        // Get all scenes in the Scenes folder
        string[] sceneFiles = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.AllDirectories);

        if (sceneFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No Scenes Found", "No scenes found in Assets/Scenes folder.", "OK");
            return;
        }

        // Add scenes to build settings
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[sceneFiles.Length];
        for (int i = 0; i < sceneFiles.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(sceneFiles[i], true);
        }

        EditorBuildSettings.scenes = buildScenes;

        Debug.Log($"Added {sceneFiles.Length} scenes to build settings:");
        foreach (var scene in sceneFiles)
        {
            Debug.Log($"  - {scene}");
        }

        EditorUtility.DisplayDialog("Success", $"Added {sceneFiles.Length} scenes to build settings!", "OK");
    }

    [MenuItem("Tools/Snakes and Ladders/Open Scenes/Open GameScene", false, 200)]
    public static void OpenGameScene()
    {
        string scenePath = "Assets/Scenes/GameScene.unity";
        if (File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
        }
        else
        {
            EditorUtility.DisplayDialog("Scene Not Found", "GameScene.unity not found. Please build it first.", "OK");
        }
    }

    [MenuItem("Tools/Snakes and Ladders/Open Scenes/Open MainMenu", false, 201)]
    public static void OpenMainMenu()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
        }
        else
        {
            EditorUtility.DisplayDialog("Scene Not Found", "MainMenu.unity not found. Please create it first.", "OK");
        }
    }

    [MenuItem("Tools/Snakes and Ladders/Utilities/Clear All Generated Assets", false, 300)]
    public static void ClearAllAssets()
    {
        if (!EditorUtility.DisplayDialog(
            "Clear All Assets?",
            "This will DELETE:\n\n" +
            "• All ScriptableObjects\n" +
            "• All Prefabs\n" +
            "• All Scenes\n\n" +
            "This action CANNOT be undone!\n\n" +
            "Are you sure?",
            "Yes, Delete Everything",
            "Cancel"))
        {
            return;
        }

        // Double confirmation
        if (!EditorUtility.DisplayDialog(
            "Final Confirmation",
            "This is your last chance to cancel.\n\n" +
            "Delete all generated assets?",
            "DELETE",
            "Cancel"))
        {
            return;
        }

        int deletedCount = 0;

        // Delete ScriptableObjects
        if (Directory.Exists("Assets/ScriptableObjects"))
        {
            string[] files = Directory.GetFiles("Assets/ScriptableObjects", "*.asset");
            foreach (string file in files)
            {
                AssetDatabase.DeleteAsset(file);
                deletedCount++;
            }
        }

        // Delete Prefabs
        if (Directory.Exists("Assets/Prefabs"))
        {
            string[] files = Directory.GetFiles("Assets/Prefabs", "*.prefab");
            foreach (string file in files)
            {
                AssetDatabase.DeleteAsset(file);
                deletedCount++;
            }
        }

        // Delete Scenes
        if (Directory.Exists("Assets/Scenes"))
        {
            string[] files = Directory.GetFiles("Assets/Scenes", "*.unity");
            foreach (string file in files)
            {
                AssetDatabase.DeleteAsset(file);
                deletedCount++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Cleared", $"Deleted {deletedCount} assets.", "OK");
    }

    [MenuItem("Tools/Snakes and Ladders/Help/View Documentation", false, 400)]
    public static void ViewDocumentation()
    {
        string readmePath = "Assets/README.md";
        if (File.Exists(readmePath))
        {
            System.Diagnostics.Process.Start(readmePath);
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "README.md not found in Assets folder.", "OK");
        }
    }

    [MenuItem("Tools/Snakes and Ladders/Help/View Implementation Guide", false, 401)]
    public static void ViewImplementationGuide()
    {
        string guidePath = "Assets/IMPLEMENTATION_GUIDE.md";
        if (File.Exists(guidePath))
        {
            System.Diagnostics.Process.Start(guidePath);
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", "IMPLEMENTATION_GUIDE.md not found in Assets folder.", "OK");
        }
    }

    // Helper methods

    private static void CreateGameConfig()
    {
        string path = "Assets/ScriptableObjects/GameConfig.asset";
        if (!File.Exists(path))
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            config.boardSize = 100;
            config.moveSpeed = 4f;
            config.movementAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"Created {path}");
        }
    }

    private static void CreateDiceConfig()
    {
        string path = "Assets/ScriptableObjects/DiceConfig.asset";
        if (!File.Exists(path))
        {
            DiceConfig config = ScriptableObject.CreateInstance<DiceConfig>();
            config.diceSides = 6;
            config.rollDuration = 1.2f;
            config.spinTorque = 10f;
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"Created {path}");
        }
    }

    private static void CreateBoardConfig()
    {
        string path = "Assets/ScriptableObjects/BoardConfig.asset";
        if (!File.Exists(path))
        {
            BoardConfig config = ScriptableObject.CreateInstance<BoardConfig>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"Created {path}");
        }
    }

    private static void CreatePlayerPrefabs()
    {
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
        string[] names = { "Player1_Red", "Player2_Blue", "Player3_Green", "Player4_Yellow" };

        for (int i = 0; i < 4; i++)
        {
            string prefabPath = $"Assets/Prefabs/{names[i]}.prefab";

            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            piece.name = names[i];
            piece.transform.localScale = Vector3.one * 0.5f;

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = colors[i];
            piece.GetComponent<Renderer>().material = mat;

            piece.AddComponent<PlayerPiece>();
            piece.AddComponent<Animator>();

            PrefabUtility.SaveAsPrefabAsset(piece, prefabPath);
            DestroyImmediate(piece);

            Debug.Log($"Created {prefabPath}");
        }
    }
}
