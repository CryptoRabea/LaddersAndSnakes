using LAS.Config;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all board logic – tile positions, ladders/snakes mapping.
/// Fully event-driven, no Update() usage.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Board Data")]
    [SerializeField] private BoardConfig boardConfig; // ScriptableObject reference
    [SerializeField] private Transform boardParent;   // Parent containing tile transforms (optional)

    private Dictionary<int, Transform> tileLookup = new Dictionary<int, Transform>();
    private Dictionary<int, int> jumpLookup = new Dictionary<int, int>(); // from -> to mapping

    private void Awake()
    {
        InitializeBoard();
    }

    /// <summary>
    /// Prepares the tile lookup table and validates board data.
    /// </summary>
    private void InitializeBoard()
    {
        if (boardParent == null)
        {
            Debug.LogWarning("BoardManager: No boardParent assigned. Generating logical positions only.");
        }

        // Build tile lookup based on child transforms (if a visual board exists)
        tileLookup.Clear();
        if (boardParent != null)
        {
            for (int i = 0; i < boardParent.childCount; i++)
            {
                tileLookup[i + 1] = boardParent.GetChild(i);
            }
        }

        // Build jump lookup from BoardConfig jumps
        jumpLookup.Clear();
        if (boardConfig != null && boardConfig.jumps != null)
        {
            foreach (var jump in boardConfig.jumps)
            {
                jumpLookup[jump.from] = jump.to;
            }
        }
    }

    /// <summary>
    /// Returns world position of a tile index.
    /// If no Transform assigned, generates logical grid coordinates.
    /// </summary>
    public Vector3 GetTilePosition(int index)
    {
        if (tileLookup.TryGetValue(index, out Transform tile))
            return tile.position;

        // Logical fallback: generate grid position (10 tiles per row)
        int row = (index - 1) / 10;
        int col = (index - 1) % 10;
        bool reverse = row % 2 == 1; // Snaking rows
        if (reverse) col = 9 - col;

        return new Vector3(col * 1.5f, 0, row * 1.5f);
    }

    /// <summary>
    /// Finds the final position if the tile leads to a ladder or snake.
    /// </summary>
    public int GetTargetTileIndex(int currentTile)
    {
        if (jumpLookup.TryGetValue(currentTile, out int targetTile))
            return targetTile;

        return currentTile;
    }

    /// <summary>
    /// Gets the BoardConfig asset being used
    /// </summary>
    public BoardConfig GetBoardConfig()
    {
        return boardConfig;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (boardConfig == null || boardConfig.jumps == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var jump in boardConfig.jumps)
        {
            Vector3 fromPos = GetTilePosition(jump.from);
            Vector3 toPos = GetTilePosition(jump.to);

            Gizmos.color = jump.isLadder ? Color.green : Color.red;
            Gizmos.DrawLine(fromPos + Vector3.up * 0.2f, toPos + Vector3.up * 0.2f);
            Gizmos.DrawSphere(fromPos + Vector3.up * 0.2f, 0.1f);
        }
    }
#endif
}
