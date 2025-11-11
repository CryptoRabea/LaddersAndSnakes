using LAS.Config;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all board logic — tile data, world positions, ladders/snakes mapping.
/// Fully event-driven, no Update() usage.
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Board Data")]
    [SerializeField] private BoardConfig boardConfig; // ScriptableObject reference
    [SerializeField] private Transform boardParent;   // Parent containing tile transforms (optional)

    private Dictionary<int, Transform> tileLookup = new Dictionary<int, Transform>();

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

        // Build lookup based on child transforms (if a visual board exists)
        tileLookup.Clear();
        if (boardParent != null)
        {
            for (int i = 0; i < boardParent.childCount; i++)
            {
                tileLookup[i + 1] = boardParent.GetChild(i);
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
    /// Returns the ScriptableObject tile data (for ladders/snakes).
    /// </summary>
    public TileData GetTile(int index)
    {
        if (boardConfig == null || boardConfig.tiles == null)
            return null;

        if (index >= 0 && index < boardConfig.tiles.Length)
            return boardConfig.tiles[index];

        return null;
    }

    /// <summary>
    /// Finds the final position if the tile leads to a ladder or snake.
    /// </summary>
    public int GetTargetTileIndex(int currentTile)
    {
        TileData tile = GetTile(currentTile);
        if (tile == null) return currentTile;

        if (tile.tileType == TileType.LadderStart || tile.tileType == TileType.SnakeHead)
            return tile.targetTileIndex;

        return currentTile;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (boardConfig == null || boardConfig.tiles == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var tile in boardConfig.tiles)
        {
            if (tile == null) continue;
            Vector3 pos = GetTilePosition(tile.tileIndex);
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
#endif
}
