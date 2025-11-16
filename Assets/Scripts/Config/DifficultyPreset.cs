using UnityEngine;

namespace LaddersAndSnakes
{
    /// <summary>
    /// Stores a difficulty preset configuration for board generation
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyPreset", menuName = "LAS/Difficulty Preset")]
    public class DifficultyPreset : ScriptableObject
    {
        [Header("Preset Info")]
        public string presetName = "Custom";
        [TextArea(2, 4)]
        public string description = "Custom difficulty configuration";

        [Header("Ladders")]
        public int minLadders = 5;
        public int maxLadders = 10;
        public int minLadderLength = 5;
        public int maxLadderLength = 40;

        [Header("Snakes")]
        public int minSnakes = 5;
        public int maxSnakes = 10;
        public int minSnakeLength = 5;
        public int maxSnakeLength = 40;

        [Header("Balance")]
        public bool balanceJumps = true;
        [Range(0.5f, 2.0f)]
        public float balanceRatio = 1.2f;

        [Header("Placement")]
        public int minDistanceBetweenJumps = 3;

        /// <summary>
        /// Applies this preset to a generator config
        /// </summary>
        public void ApplyToConfig(BoardGeneratorConfig config)
        {
            config.minLadders = minLadders;
            config.maxLadders = maxLadders;
            config.minLadderLength = minLadderLength;
            config.maxLadderLength = maxLadderLength;

            config.minSnakes = minSnakes;
            config.maxSnakes = maxSnakes;
            config.minSnakeLength = minSnakeLength;
            config.maxSnakeLength = maxSnakeLength;

            config.balanceJumps = balanceJumps;
            config.balanceRatio = balanceRatio;

            config.minDistanceBetweenJumps = minDistanceBetweenJumps;
        }

        /// <summary>
        /// Creates a preset from a config
        /// </summary>
        public static DifficultyPreset FromConfig(BoardGeneratorConfig config, string name, string desc)
        {
            var preset = CreateInstance<DifficultyPreset>();
            preset.presetName = name;
            preset.description = desc;

            preset.minLadders = config.minLadders;
            preset.maxLadders = config.maxLadders;
            preset.minLadderLength = config.minLadderLength;
            preset.maxLadderLength = config.maxLadderLength;

            preset.minSnakes = config.minSnakes;
            preset.maxSnakes = config.maxSnakes;
            preset.minSnakeLength = config.minSnakeLength;
            preset.maxSnakeLength = config.maxSnakeLength;

            preset.balanceJumps = config.balanceJumps;
            preset.balanceRatio = config.balanceRatio;

            preset.minDistanceBetweenJumps = config.minDistanceBetweenJumps;

            return preset;
        }
    }
}
