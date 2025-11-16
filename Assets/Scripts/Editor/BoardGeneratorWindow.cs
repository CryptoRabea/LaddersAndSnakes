using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LAS.Config;

namespace LAS.Editor
{
    /// <summary>
    /// Editor window for generating board configurations
    /// </summary>
    public class BoardGeneratorWindow : EditorWindow
    {
        private BoardGeneratorConfig config;
        private DifficultyPreset selectedPreset;
        private DifficultyLevel selectedDifficulty = DifficultyLevel.Medium;
        private bool usePreset = false;
        private bool showAdvanced = false;

        private List<BoardJump> generatedJumps;
        private string lastError;
        private string lastSuccess;

        private Vector2 scrollPosition;
        private Vector2 previewScrollPosition;

        [MenuItem("LAS/Board Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BoardGeneratorWindow>("Board Generator");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            // Load or create default config
            config = AssetDatabase.LoadAssetAtPath<BoardGeneratorConfig>("Assets/Config/BoardGeneratorConfig.asset");
            if (config == null)
            {
                config = CreateInstance<BoardGeneratorConfig>();
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawDifficultySelection();
            EditorGUILayout.Space(10);

            if (!usePreset)
            {
                DrawConfigEditor();
                EditorGUILayout.Space(10);
            }

            DrawGenerationControls();
            EditorGUILayout.Space(10);

            DrawMessages();
            EditorGUILayout.Space(10);

            if (generatedJumps != null && generatedJumps.Count > 0)
            {
                DrawPreview();
                EditorGUILayout.Space(10);
                DrawSaveControls();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Board Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Generate random board configurations with customizable ladders and snakes. " +
                "Choose a difficulty preset or configure manually for complete control.",
                MessageType.Info
            );
        }

        private void DrawDifficultySelection()
        {
            EditorGUILayout.LabelField("Generation Mode", EditorStyles.boldLabel);

            usePreset = EditorGUILayout.Toggle("Use Preset", usePreset);

            if (usePreset)
            {
                selectedPreset = (DifficultyPreset)EditorGUILayout.ObjectField(
                    "Difficulty Preset",
                    selectedPreset,
                    typeof(DifficultyPreset),
                    false
                );

                if (selectedPreset != null)
                {
                    EditorGUILayout.HelpBox(selectedPreset.description, MessageType.None);
                }
            }
            else
            {
                selectedDifficulty = (DifficultyLevel)EditorGUILayout.EnumPopup("Difficulty", selectedDifficulty);

                if (selectedDifficulty != DifficultyLevel.Custom)
                {
                    EditorGUILayout.HelpBox(GetDifficultyDescription(selectedDifficulty), MessageType.None);
                }
            }
        }

        private void DrawConfigEditor()
        {
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced Settings");

            if (showAdvanced)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Board", EditorStyles.boldLabel);
                config.boardSize = EditorGUILayout.IntField("Board Size", config.boardSize);
                config.columns = EditorGUILayout.IntField("Columns", config.columns);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Ladders", EditorStyles.boldLabel);
                config.minLadders = EditorGUILayout.IntField("Min Ladders", config.minLadders);
                config.maxLadders = EditorGUILayout.IntField("Max Ladders", config.maxLadders);
                config.minLadderLength = EditorGUILayout.IntField("Min Length", config.minLadderLength);
                config.maxLadderLength = EditorGUILayout.IntField("Max Length", config.maxLadderLength);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Snakes", EditorStyles.boldLabel);
                config.minSnakes = EditorGUILayout.IntField("Min Snakes", config.minSnakes);
                config.maxSnakes = EditorGUILayout.IntField("Max Snakes", config.maxSnakes);
                config.minSnakeLength = EditorGUILayout.IntField("Min Length", config.minSnakeLength);
                config.maxSnakeLength = EditorGUILayout.IntField("Max Length", config.maxSnakeLength);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Balance", EditorStyles.boldLabel);
                config.balanceJumps = EditorGUILayout.Toggle("Balance Jumps", config.balanceJumps);
                if (config.balanceJumps)
                {
                    config.balanceRatio = EditorGUILayout.Slider("Balance Ratio", config.balanceRatio, 0.5f, 2.0f);
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
                config.minDistanceBetweenJumps = EditorGUILayout.IntField("Min Distance", config.minDistanceBetweenJumps);
                config.minStartTile = EditorGUILayout.IntField("Min Start Tile", config.minStartTile);
                config.maxStartTile = EditorGUILayout.IntField("Max Start Tile", config.maxStartTile);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
                config.seed = EditorGUILayout.IntField("Random Seed (0=random)", config.seed);
                config.maxGenerationAttempts = EditorGUILayout.IntField("Max Attempts", config.maxGenerationAttempts);

                EditorGUI.indentLevel--;
            }
        }

        private void DrawGenerationControls()
        {
            EditorGUILayout.LabelField("Generate Board", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                GenerateBoard();
            }

            if (GUILayout.Button("Regenerate (New Seed)", GUILayout.Height(30)))
            {
                config.seed = 0; // Force new random seed
                GenerateBoard();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMessages()
        {
            if (!string.IsNullOrEmpty(lastError))
            {
                EditorGUILayout.HelpBox(lastError, MessageType.Error);
            }

            if (!string.IsNullOrEmpty(lastSuccess))
            {
                EditorGUILayout.HelpBox(lastSuccess, MessageType.Info);
            }
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Generated Board Preview", EditorStyles.boldLabel);

            // Statistics
            int ladderCount = 0, snakeCount = 0;
            int totalLadderLength = 0, totalSnakeLength = 0;

            foreach (var jump in generatedJumps)
            {
                if (jump.isLadder)
                {
                    ladderCount++;
                    totalLadderLength += (jump.to - jump.from);
                }
                else
                {
                    snakeCount++;
                    totalSnakeLength += (jump.from - jump.to);
                }
            }

            EditorGUILayout.LabelField($"Total Jumps: {generatedJumps.Count} (Ladders: {ladderCount}, Snakes: {snakeCount})");
            EditorGUILayout.LabelField($"Balance Ratio: {BoardValidator.CalculateBalanceRatio(generatedJumps):F2}");
            EditorGUILayout.LabelField($"Avg Ladder Length: {(ladderCount > 0 ? totalLadderLength / ladderCount : 0)}");
            EditorGUILayout.LabelField($"Avg Snake Length: {(snakeCount > 0 ? totalSnakeLength / snakeCount : 0)}");

            EditorGUILayout.Space(5);

            // Jump list
            previewScrollPosition = EditorGUILayout.BeginScrollView(previewScrollPosition, GUILayout.Height(200));

            foreach (var jump in generatedJumps)
            {
                string jumpType = jump.isLadder ? "Ladder" : "Snake";
                string icon = jump.isLadder ? "↑" : "↓";
                int length = Mathf.Abs(jump.to - jump.from);

                EditorGUILayout.LabelField($"{icon} {jumpType}: {jump.from} → {jump.to} (length: {length})");
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSaveControls()
        {
            EditorGUILayout.LabelField("Save Board", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save as New BoardConfig", GUILayout.Height(30)))
            {
                SaveAsBoardConfig();
            }

            if (GUILayout.Button("Update DefaultBoardConfig", GUILayout.Height(30)))
            {
                UpdateDefaultBoardConfig();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save Config as Preset", GUILayout.Height(25)))
            {
                SaveAsPreset();
            }
        }

        private void GenerateBoard()
        {
            lastError = null;
            lastSuccess = null;

            // Apply preset or difficulty if selected
            if (usePreset && selectedPreset != null)
            {
                selectedPreset.ApplyToConfig(config);
            }

            // Validate config
            if (!config.IsValid(out string error))
            {
                lastError = $"Invalid configuration: {error}";
                return;
            }

            // Generate
            var algorithm = new BoardGeneratorAlgorithm(config);

            if (selectedDifficulty != DifficultyLevel.Custom && !usePreset)
            {
                generatedJumps = algorithm.GenerateBoardWithDifficulty(selectedDifficulty, out error);
            }
            else
            {
                generatedJumps = algorithm.GenerateBoard(out error);
            }

            if (generatedJumps == null)
            {
                lastError = $"Generation failed: {error}";
            }
            else
            {
                lastSuccess = $"Successfully generated board with {generatedJumps.Count} jumps!";
            }

            Repaint();
        }

        private void SaveAsBoardConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Board Config",
                "NewBoardConfig",
                "asset",
                "Save the generated board configuration"
            );

            if (string.IsNullOrEmpty(path))
                return;

            var boardConfig = CreateInstance<BoardConfig>();
            boardConfig.jumps = generatedJumps;

            AssetDatabase.CreateAsset(boardConfig, path);
            AssetDatabase.SaveAssets();

            lastSuccess = $"Saved BoardConfig to {path}";
            EditorGUIUtility.PingObject(boardConfig);
        }

        private void UpdateDefaultBoardConfig()
        {
            var defaultConfig = AssetDatabase.LoadAssetAtPath<BoardConfig>("Assets/Config/DefaultBoardConfig.asset");

            if (defaultConfig == null)
            {
                lastError = "DefaultBoardConfig not found at Assets/Config/DefaultBoardConfig.asset";
                return;
            }

            defaultConfig.jumps = generatedJumps;
            EditorUtility.SetDirty(defaultConfig);
            AssetDatabase.SaveAssets();

            lastSuccess = "Updated DefaultBoardConfig successfully!";
        }

        private void SaveAsPreset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Difficulty Preset",
                "NewPreset",
                "asset",
                "Save the current configuration as a preset"
            );

            if (string.IsNullOrEmpty(path))
                return;

            var preset = DifficultyPreset.FromConfig(config, "Custom Preset", "User-created preset");

            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();

            lastSuccess = $"Saved preset to {path}";
            EditorGUIUtility.PingObject(preset);
        }

        private string GetDifficultyDescription(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    return "Easy: More ladders (8-12), fewer snakes (5-8). Good for beginners.";
                case DifficultyLevel.Medium:
                    return "Medium: Balanced ladders (6-9) and snakes (6-9). Standard gameplay.";
                case DifficultyLevel.Hard:
                    return "Hard: Fewer ladders (5-7), more snakes (8-12). Challenging gameplay.";
                case DifficultyLevel.Extreme:
                    return "Extreme: Very few ladders (3-5), many snakes (12-15). Very difficult!";
                default:
                    return "Custom: Use manual settings below.";
            }
        }
    }
}
