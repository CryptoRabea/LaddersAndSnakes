using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using LAS.Config;
using LaddersAndSnakes;

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
            config.jumps.Add(new BoardJump(98, 78, false));
            config.jumps.Add(new BoardJump(95, 75, false));
            config.jumps.Add(new BoardJump(92, 73, false));
            config.jumps.Add(new BoardJump(87, 36, false));
            config.jumps.Add(new BoardJump(64, 60, false));
            config.jumps.Add(new BoardJump(62, 19, false));
            config.jumps.Add(new BoardJump(54, 34, false));
            config.jumps.Add(new BoardJump(17, 7, false));

            // Add default ladders (bottom to top)
            config.jumps.Add(new BoardJump(4, 14, true));
            config.jumps.Add(new BoardJump(9, 31, true));
            config.jumps.Add(new BoardJump(21, 42, true));
            config.jumps.Add(new BoardJump(28, 84, true));
            config.jumps.Add(new BoardJump(40, 63, true));
            config.jumps.Add(new BoardJump(51, 67, true));
            config.jumps.Add(new BoardJump(71, 91, true));

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

        [MenuItem("LAS/Generate Random Board (Medium)")]
        public static void GenerateRandomBoard()
        {
            GenerateRandomBoardWithDifficulty(DifficultyLevel.Medium);
        }

        [MenuItem("LAS/Generate Random Board/Easy")]
        public static void GenerateRandomBoardEasy()
        {
            GenerateRandomBoardWithDifficulty(DifficultyLevel.Easy);
        }

        [MenuItem("LAS/Generate Random Board/Medium")]
        public static void GenerateRandomBoardMedium()
        {
            GenerateRandomBoardWithDifficulty(DifficultyLevel.Medium);
        }

        [MenuItem("LAS/Generate Random Board/Hard")]
        public static void GenerateRandomBoardHard()
        {
            GenerateRandomBoardWithDifficulty(DifficultyLevel.Hard);
        }

        [MenuItem("LAS/Generate Random Board/Extreme")]
        public static void GenerateRandomBoardExtreme()
        {
            GenerateRandomBoardWithDifficulty(DifficultyLevel.Extreme);
        }

        private static void GenerateRandomBoardWithDifficulty(DifficultyLevel difficulty)
        {
            // Ensure the Config folder exists
            if (!AssetDatabase.IsValidFolder(CONFIG_FOLDER))
            {
                AssetDatabase.CreateFolder("Assets", "Config");
            }

            // Create generator config
            var generatorConfig = ScriptableObject.CreateInstance<BoardGeneratorConfig>();

            // Generate board
            var algorithm = new BoardGeneratorAlgorithm(generatorConfig);
            var jumps = algorithm.GenerateBoardWithDifficulty(difficulty, out string error);

            if (jumps == null)
            {
                Debug.LogError($"Failed to generate board: {error}");
                EditorUtility.DisplayDialog("Generation Failed",
                    $"Failed to generate random board:\n{error}",
                    "OK");
                return;
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

            // Set the generated jumps
            config.jumps = jumps;

            // Save the asset
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the asset in the project window
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            int ladderCount = jumps.Count(j => j.isLadder);
            int snakeCount = jumps.Count - ladderCount;
            float balanceRatio = BoardValidator.CalculateBalanceRatio(jumps);

            Debug.Log($"Generated random {difficulty} board with {jumps.Count} jumps " +
                     $"({ladderCount} ladders + {snakeCount} snakes). " +
                     $"Balance ratio: {balanceRatio:F2}");

            EditorUtility.DisplayDialog("Board Generated",
                $"Successfully generated {difficulty} board!\n\n" +
                $"Jumps: {jumps.Count}\n" +
                $"Ladders: {ladderCount}\n" +
                $"Snakes: {snakeCount}\n" +
                $"Balance Ratio: {balanceRatio:F2}",
                "OK");
        }
    }
}
