using UnityEngine;

namespace LAS.Config
{
    /// <summary>
    /// Configuration for procedural board generation
    /// </summary>
    [CreateAssetMenu(fileName = "BoardGeneratorConfig", menuName = "LAS/Board Generator Config")]
    public class BoardGeneratorConfig : ScriptableObject
    {
        [Header("Board Dimensions")]
        [Tooltip("Size of the board (e.g., 100 for 10x10)")]
        public int boardSize = 100;

        [Tooltip("Number of columns (e.g., 10 for standard board)")]
        public int columns = 10;

        [Header("Ladders Configuration")]
        [Tooltip("Minimum number of ladders to generate")]
        public int minLadders = 5;

        [Tooltip("Maximum number of ladders to generate")]
        public int maxLadders = 10;

        [Tooltip("Minimum length of a ladder (in tiles)")]
        public int minLadderLength = 5;

        [Tooltip("Maximum length of a ladder (in tiles)")]
        public int maxLadderLength = 40;

        [Header("Snakes Configuration")]
        [Tooltip("Minimum number of snakes to generate")]
        public int minSnakes = 5;

        [Tooltip("Maximum number of snakes to generate")]
        public int maxSnakes = 10;

        [Tooltip("Minimum length of a snake (in tiles)")]
        public int minSnakeLength = 5;

        [Tooltip("Maximum length of a snake (in tiles)")]
        public int maxSnakeLength = 40;

        [Header("Placement Rules")]
        [Tooltip("Minimum distance between jump start positions")]
        public int minDistanceBetweenJumps = 3;

        [Tooltip("Tiles where jumps cannot start (e.g., tile 1, 100)")]
        public int[] blockedTiles = new int[] { 1, 100 };

        [Tooltip("Minimum tile where jumps can start")]
        public int minStartTile = 2;

        [Tooltip("Maximum tile where jumps can start")]
        public int maxStartTile = 95;

        [Header("Balance Settings")]
        [Tooltip("Try to balance total ladder advancement vs snake setback")]
        public bool balanceJumps = true;

        [Tooltip("Allowed imbalance ratio (ladders/snakes total length)")]
        [Range(0.5f, 2.0f)]
        public float balanceRatio = 1.2f;

        [Header("Generation Settings")]
        [Tooltip("Random seed for reproducible generation (0 = random)")]
        public int seed = 0;

        [Tooltip("Maximum attempts to generate a valid board")]
        public int maxGenerationAttempts = 100;

        [Tooltip("Maximum attempts to place a single jump")]
        public int maxPlacementAttempts = 50;

        /// <summary>
        /// Gets the number of rows based on board size and columns
        /// </summary>
        public int Rows => boardSize / columns;

        /// <summary>
        /// Validates the configuration
        /// </summary>
        public bool IsValid(out string error)
        {
            if (boardSize <= 0 || columns <= 0)
            {
                error = "Board size and columns must be positive";
                return false;
            }

            if (boardSize % columns != 0)
            {
                error = "Board size must be divisible by columns";
                return false;
            }

            if (minLadders < 0 || maxLadders < minLadders)
            {
                error = "Invalid ladder count configuration";
                return false;
            }

            if (minSnakes < 0 || maxSnakes < minSnakes)
            {
                error = "Invalid snake count configuration";
                return false;
            }

            if (minLadderLength <= 0 || maxLadderLength < minLadderLength)
            {
                error = "Invalid ladder length configuration";
                return false;
            }

            if (minSnakeLength <= 0 || maxSnakeLength < minSnakeLength)
            {
                error = "Invalid snake length configuration";
                return false;
            }

            if (minStartTile < 1 || maxStartTile > boardSize)
            {
                error = "Invalid start tile range";
                return false;
            }

            error = null;
            return true;
        }
    }
}
