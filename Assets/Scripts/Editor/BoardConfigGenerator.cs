using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LAS.Config;

namespace LaddersAndSnakes.Editor
{
    /// <summary>
    /// Editor utility to create and configure default BoardConfig asset
    /// </summary>
    public class BoardConfigGenerator
    {
        private const string CONFIG_PATH = "Assets/Config/DefaultBoardConfig.asset";
        private const string CONFIG_FOLDER = "Assets/Config";

        [MenuItem("LAS/Create Default Board Config")]
        public static void CreateDefaultBoardConfig()
        {
            // Ensure the Config folder exists
            if (!AssetDatabase.IsValidFolder(CONFIG_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets", "Config");
            }

            // Create or load existing config
            BoardConfig config = AssetDatabase.LoadAssetAtPath<BoardConfig>(CONFIG_PATH);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BoardConfig>();
                AssetDatabase.CreateAsset(config, CONFIG_PATH);
                Debug.Log("Created new BoardConfig at: " + CONFIG_PATH);
            }
            else
            {
                Debug.Log("Updating existing BoardConfig at: " + CONFIG_PATH);
            }

            // Clear existing jumps
            config.jumps.Clear();

            // Add default snakes (head to tail)
            config.jumps.Add(new BoardJump { from = 98, to = 78 });
            config.jumps.Add(new BoardJump { from = 95, to = 75 });
            config.jumps.Add(new BoardJump { from = 92, to = 73 });
            config.jumps.Add(new BoardJump { from = 87, to = 36 });
            config.jumps.Add(new BoardJump { from = 64, to = 60 });
            config.jumps.Add(new BoardJump { from = 62, to = 19 });
            config.jumps.Add(new BoardJump { from = 54, to = 34 });
            config.jumps.Add(new BoardJump { from = 17, to = 7 });

            // Add default ladders (bottom to top)
            config.jumps.Add(new BoardJump { from = 4, to = 14 });
            config.jumps.Add(new BoardJump { from = 9, to = 31 });
            config.jumps.Add(new BoardJump { from = 21, to = 42 });
            config.jumps.Add(new BoardJump { from = 28, to = 84 });
            config.jumps.Add(new BoardJump { from = 40, to = 63 });
            config.jumps.Add(new BoardJump { from = 51, to = 67 });
            config.jumps.Add(new BoardJump { from = 71, to = 91 });

            // Save the asset
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the asset in the project window
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log($"BoardConfig configured with {config.jumps.Count} jumps " +
                     "(8 snakes + 7 ladders)");
        }

        [MenuItem("LAS/Select Board Config")]
        public static void SelectBoardConfig()
        {
            BoardConfig config = AssetDatabase.LoadAssetAtPath<BoardConfig>(CONFIG_PATH);
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
            else
            {
                Debug.LogWarning("BoardConfig not found at: " + CONFIG_PATH +
                               ". Use 'LAS/Create Default Board Config' to create it.");
            }
        }
    }
}
