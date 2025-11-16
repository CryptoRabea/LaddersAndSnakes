using System.Collections.Generic;
using System.Linq;
using LAS.Config;

namespace LAS
{
    /// <summary>
    /// Validates board configurations to ensure they follow game rules
    /// </summary>
    public static class BoardValidator
    {
        /// <summary>
        /// Validates a list of board jumps
        /// </summary>
        public static bool ValidateJumps(List<BoardJump> jumps, int boardSize, out string error)
        {
            if (jumps == null)
            {
                error = "Jumps list is null";
                return false;
            }

            // Check for duplicate start positions
            var startPositions = jumps.Select(j => j.from).ToList();
            if (startPositions.Count != startPositions.Distinct().Count())
            {
                error = "Duplicate jump start positions detected";
                return false;
            }

            // Validate each jump
            foreach (var jump in jumps)
            {
                if (!ValidateJump(jump, boardSize, out error))
                {
                    return false;
                }
            }

            // Check for chained jumps (jump lands on another jump start)
            var endPositions = jumps.Select(j => j.to).ToHashSet();
            foreach (var jump in jumps)
            {
                if (startPositions.Contains(jump.to))
                {
                    error = $"Jump at {jump.from} lands on another jump start at {jump.to}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Validates a single jump
        /// </summary>
        public static bool ValidateJump(BoardJump jump, int boardSize, out string error)
        {
            // Check bounds
            if (jump.from < 1 || jump.from > boardSize)
            {
                error = $"Jump start {jump.from} is out of bounds (1-{boardSize})";
                return false;
            }

            if (jump.to < 1 || jump.to > boardSize)
            {
                error = $"Jump end {jump.to} is out of bounds (1-{boardSize})";
                return false;
            }

            // Check that jump is not to the same tile
            if (jump.from == jump.to)
            {
                error = $"Jump at {jump.from} goes to itself";
                return false;
            }

            // Ladders must go up, snakes must go down
            if (jump.isLadder && jump.to <= jump.from)
            {
                error = $"Ladder at {jump.from} goes down or stays same (to {jump.to})";
                return false;
            }

            if (!jump.isLadder && jump.to >= jump.from)
            {
                error = $"Snake at {jump.from} goes up or stays same (to {jump.to})";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Checks if a tile is blocked (usually first and last tile)
        /// </summary>
        public static bool IsTileBlocked(int tile, int[] blockedTiles)
        {
            if (blockedTiles == null || blockedTiles.Length == 0)
                return false;

            return blockedTiles.Contains(tile);
        }

        /// <summary>
        /// Checks if two jumps are too close to each other
        /// </summary>
        public static bool AreJumpsTooClose(int tile1, int tile2, int minDistance)
        {
            return System.Math.Abs(tile1 - tile2) < minDistance;
        }

        /// <summary>
        /// Checks if a tile can be used for a jump start
        /// </summary>
        public static bool CanPlaceJumpAt(int tile, List<BoardJump> existingJumps, int[] blockedTiles, int minDistance)
        {
            // Check if tile is blocked
            if (IsTileBlocked(tile, blockedTiles))
                return false;

            // Check if tile already has a jump
            if (existingJumps.Any(j => j.from == tile))
                return false;

            // Check if tile is a destination of another jump
            if (existingJumps.Any(j => j.to == tile))
                return false;

            // Check minimum distance from other jumps
            foreach (var jump in existingJumps)
            {
                if (AreJumpsTooClose(tile, jump.from, minDistance))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates the balance ratio of ladders vs snakes
        /// </summary>
        public static float CalculateBalanceRatio(List<BoardJump> jumps)
        {
            int totalLadderAdvancement = 0;
            int totalSnakeSetback = 0;

            foreach (var jump in jumps)
            {
                int distance = System.Math.Abs(jump.to - jump.from);
                if (jump.isLadder)
                    totalLadderAdvancement += distance;
                else
                    totalSnakeSetback += distance;
            }

            if (totalSnakeSetback == 0)
                return totalLadderAdvancement > 0 ? float.MaxValue : 1.0f;

            return (float)totalLadderAdvancement / totalSnakeSetback;
        }

        /// <summary>
        /// Checks if the board is balanced according to the specified ratio
        /// </summary>
        public static bool IsBalanced(List<BoardJump> jumps, float targetRatio, float tolerance = 0.3f)
        {
            float actualRatio = CalculateBalanceRatio(jumps);
            return System.Math.Abs(actualRatio - targetRatio) <= tolerance;
        }
    }
}
