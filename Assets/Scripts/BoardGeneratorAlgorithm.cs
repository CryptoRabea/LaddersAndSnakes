using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LAS.Config;

namespace LaddersAndSnakes
{
    /// <summary>
    /// Core algorithm for generating random board configurations
    /// </summary>
    public class BoardGeneratorAlgorithm
    {
        private BoardGeneratorConfig config;
        private System.Random random;

        public BoardGeneratorAlgorithm(BoardGeneratorConfig config)
        {
            this.config = config;
            int seed = config.seed == 0 ? System.Environment.TickCount : config.seed;
            this.random = new System.Random(seed);
        }

        /// <summary>
        /// Generates a complete board configuration
        /// </summary>
        public List<BoardJump> GenerateBoard(out string error)
        {
            if (!config.IsValid(out error))
            {
                return null;
            }

            for (int attempt = 0; attempt < config.maxGenerationAttempts; attempt++)
            {
                var jumps = new List<BoardJump>();

                // Generate ladders
                int ladderCount = random.Next(config.minLadders, config.maxLadders + 1);
                if (!GenerateJumps(jumps, ladderCount, true, out error))
                {
                    continue; // Try again
                }

                // Generate snakes
                int snakeCount = random.Next(config.minSnakes, config.maxSnakes + 1);
                if (!GenerateJumps(jumps, snakeCount, false, out error))
                {
                    continue; // Try again
                }

                // Validate the complete board
                if (!BoardValidator.ValidateJumps(jumps, config.boardSize, out error))
                {
                    continue;
                }

                // Check balance if required
                if (config.balanceJumps)
                {
                    if (!BoardValidator.IsBalanced(jumps, config.balanceRatio))
                    {
                        error = "Board is not balanced";
                        continue;
                    }
                }

                // Success!
                error = null;
                return jumps;
            }

            error = $"Failed to generate valid board after {config.maxGenerationAttempts} attempts. Last error: {error}";
            return null;
        }

        /// <summary>
        /// Generates a specific number of jumps (ladders or snakes)
        /// </summary>
        private bool GenerateJumps(List<BoardJump> existingJumps, int count, bool isLadder, out string error)
        {
            int successfulPlacements = 0;

            for (int i = 0; i < count; i++)
            {
                BoardJump jump = GenerateSingleJump(existingJumps, isLadder, out error);
                if (jump != null)
                {
                    existingJumps.Add(jump);
                    successfulPlacements++;
                }
                else
                {
                    // If we can't place this jump, try to continue
                    // We'll fail if we didn't get minimum required
                    continue;
                }
            }

            // Consider it successful if we got at least the minimum
            int minRequired = isLadder ? config.minLadders : config.minSnakes;
            if (successfulPlacements < minRequired)
            {
                error = $"Only placed {successfulPlacements} {(isLadder ? "ladders" : "snakes")}, minimum is {minRequired}";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Generates a single jump (ladder or snake)
        /// </summary>
        private BoardJump GenerateSingleJump(List<BoardJump> existingJumps, bool isLadder, out string error)
        {
            for (int attempt = 0; attempt < config.maxPlacementAttempts; attempt++)
            {
                // Pick a random start position
                int startTile = random.Next(config.minStartTile, config.maxStartTile + 1);

                // Check if we can place a jump here
                if (!BoardValidator.CanPlaceJumpAt(startTile, existingJumps, config.blockedTiles, config.minDistanceBetweenJumps))
                {
                    continue;
                }

                // Calculate valid end position
                int endTile = CalculateEndTile(startTile, isLadder, existingJumps);
                if (endTile < 1 || endTile > config.boardSize)
                {
                    continue;
                }

                // Create the jump
                var jump = new BoardJump(startTile, endTile, isLadder);

                // Validate it
                if (!BoardValidator.ValidateJump(jump, config.boardSize, out error))
                {
                    continue;
                }

                // Check that destination is not blocked or occupied
                if (BoardValidator.IsTileBlocked(endTile, config.blockedTiles))
                {
                    continue;
                }

                if (existingJumps.Any(j => j.to == endTile || j.from == endTile))
                {
                    continue;
                }

                error = null;
                return jump;
            }

            error = $"Failed to place {(isLadder ? "ladder" : "snake")} after {config.maxPlacementAttempts} attempts";
            return null;
        }

        /// <summary>
        /// Calculates a valid end tile for a jump
        /// </summary>
        private int CalculateEndTile(int startTile, bool isLadder, List<BoardJump> existingJumps)
        {
            int minLength = isLadder ? config.minLadderLength : config.minSnakeLength;
            int maxLength = isLadder ? config.maxLadderLength : config.maxSnakeLength;

            // Pick a random length
            int length = random.Next(minLength, maxLength + 1);

            // Calculate end tile
            int endTile;
            if (isLadder)
            {
                endTile = startTile + length;
                // Make sure we don't go past the board
                if (endTile > config.boardSize)
                {
                    endTile = config.boardSize;
                }
            }
            else
            {
                endTile = startTile - length;
                // Make sure we don't go below 1
                if (endTile < 1)
                {
                    endTile = 1;
                }
            }

            return endTile;
        }

        /// <summary>
        /// Generates a board with a specific difficulty level
        /// </summary>
        public List<BoardJump> GenerateBoardWithDifficulty(DifficultyLevel difficulty, out string error)
        {
            // Apply difficulty preset to config
            ApplyDifficultyPreset(difficulty);

            // Generate the board
            return GenerateBoard(out error);
        }

        /// <summary>
        /// Applies a difficulty preset to the configuration
        /// </summary>
        private void ApplyDifficultyPreset(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Easy:
                    config.minLadders = 8;
                    config.maxLadders = 12;
                    config.minSnakes = 5;
                    config.maxSnakes = 8;
                    config.minLadderLength = 10;
                    config.maxLadderLength = 40;
                    config.minSnakeLength = 5;
                    config.maxSnakeLength = 25;
                    config.balanceRatio = 1.5f; // More ladders than snakes
                    break;

                case DifficultyLevel.Medium:
                    config.minLadders = 6;
                    config.maxLadders = 9;
                    config.minSnakes = 6;
                    config.maxSnakes = 9;
                    config.minLadderLength = 8;
                    config.maxLadderLength = 35;
                    config.minSnakeLength = 8;
                    config.maxSnakeLength = 35;
                    config.balanceRatio = 1.2f; // Slightly more ladders
                    break;

                case DifficultyLevel.Hard:
                    config.minLadders = 5;
                    config.maxLadders = 7;
                    config.minSnakes = 8;
                    config.maxSnakes = 12;
                    config.minLadderLength = 5;
                    config.maxLadderLength = 30;
                    config.minSnakeLength = 10;
                    config.maxSnakeLength = 45;
                    config.balanceRatio = 0.8f; // More snakes than ladders
                    break;

                case DifficultyLevel.Extreme:
                    config.minLadders = 3;
                    config.maxLadders = 5;
                    config.minSnakes = 12;
                    config.maxSnakes = 15;
                    config.minLadderLength = 5;
                    config.maxLadderLength = 20;
                    config.minSnakeLength = 15;
                    config.maxSnakeLength = 60;
                    config.balanceRatio = 0.5f; // Much more snakes
                    break;

                case DifficultyLevel.Custom:
                    // Don't modify config for custom
                    break;
            }
        }
    }

    /// <summary>
    /// Difficulty levels for board generation
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Extreme,
        Custom
    }
}
