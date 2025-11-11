using UnityEngine;
using System.Collections.Generic;
using LAS.Config;

/// <summary>
/// Generates a 3D board with tiles, snakes, and ladders
/// </summary>
public class BoardGenerator : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int rows = 10;
    [SerializeField] private int columns = 10;
    [SerializeField] private float tileSize = 1.5f;
    [SerializeField] private float tileHeight = 0.1f;

    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject snakePrefab;
    [SerializeField] private GameObject ladderPrefab;

    [Header("Materials")]
    [SerializeField] private Material lightTileMaterial;
    [SerializeField] private Material darkTileMaterial;
    [SerializeField] private Material startTileMaterial;
    [SerializeField] private Material endTileMaterial;

    [Header("Snake & Ladder Configuration")]
    [SerializeField] private int numberOfSnakes = 5;
    [SerializeField] private int numberOfLadders = 5;
    [SerializeField] private BoardConfig boardConfig;

    [Header("Labels")]
    [SerializeField] private bool showTileNumbers = true;
    [SerializeField] private GameObject tileNumberPrefab;

    private Transform boardParent;
    private Dictionary<int, Transform> tiles = new Dictionary<int, Transform>();
    private List<SnakeLadderData> snakesAndLadders = new List<SnakeLadderData>();

    [System.Serializable]
    public class SnakeLadderData
    {
        public int from;
        public int to;
        public bool isLadder;
    }

    private void Start()
    {
        GenerateBoard();
    }

    /// <summary>
    /// Generate the complete board
    /// </summary>
    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        ClearBoard();
        CreateBoardParent();
        GenerateTiles();
        GenerateSnakesAndLadders();
        UpdateBoardConfig();
    }

    /// <summary>
    /// Clear existing board
    /// </summary>
    private void ClearBoard()
    {
        if (boardParent != null)
        {
            DestroyImmediate(boardParent.gameObject);
        }

        tiles.Clear();
        snakesAndLadders.Clear();
    }

    /// <summary>
    /// Create parent container for board
    /// </summary>
    private void CreateBoardParent()
    {
        GameObject parentObj = new GameObject("Board");
        boardParent = parentObj.transform;
        boardParent.SetParent(transform);
        boardParent.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Generate all tiles
    /// </summary>
    private void GenerateTiles()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int tileNumber = GetTileNumber(row, col);
                Vector3 position = GetTilePosition(row, col);

                CreateTile(tileNumber, position, row, col);
            }
        }
    }

    /// <summary>
    /// Create a single tile
    /// </summary>
    private void CreateTile(int tileNumber, Vector3 position, int row, int col)
    {
        GameObject tile;

        if (tilePrefab != null)
        {
            tile = Instantiate(tilePrefab, position, Quaternion.identity, boardParent);
        }
        else
        {
            // Create default cube tile
            tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.transform.position = position;
            tile.transform.localScale = new Vector3(tileSize * 0.9f, tileHeight, tileSize * 0.9f);
            tile.transform.SetParent(boardParent);
        }

        tile.name = $"Tile_{tileNumber}";

        // Set tile material based on position
        Renderer renderer = tile.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (tileNumber == 1 && startTileMaterial != null)
                renderer.material = startTileMaterial;
            else if (tileNumber == rows * columns && endTileMaterial != null)
                renderer.material = endTileMaterial;
            else
                renderer.material = (row + col) % 2 == 0 ? lightTileMaterial : darkTileMaterial;
        }

        // Add tile number label
        if (showTileNumbers)
        {
            CreateTileLabel(tile.transform, tileNumber);
        }

        tiles[tileNumber] = tile.transform;
    }

    /// <summary>
    /// Create tile number label
    /// </summary>
    private void CreateTileLabel(Transform parent, int number)
    {
        GameObject label = new GameObject($"Label_{number}");
        label.transform.SetParent(parent);
        label.transform.localPosition = new Vector3(0, tileHeight + 0.01f, 0);
        label.transform.localRotation = Quaternion.Euler(90, 0, 0);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = number.ToString();
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.black;
    }

    /// <summary>
    /// Generate snakes and ladders
    /// </summary>
    private void GenerateSnakesAndLadders()
    {
        // Generate ladders
        for (int i = 0; i < numberOfLadders; i++)
        {
            CreateRandomLadder();
        }

        // Generate snakes
        for (int i = 0; i < numberOfSnakes; i++)
        {
            CreateRandomSnake();
        }
    }

    /// <summary>
    /// Create a random ladder
    /// </summary>
    private void CreateRandomLadder()
    {
        int maxTile = rows * columns - 1;
        int attempts = 0;

        while (attempts < 100)
        {
            int from = Random.Range(2, maxTile / 2);
            int to = Random.Range(from + 10, maxTile);

            // Check if positions are already used
            if (!IsPositionUsed(from) && !IsPositionUsed(to))
            {
                CreateLadder(from, to);
                break;
            }

            attempts++;
        }
    }

    /// <summary>
    /// Create a random snake
    /// </summary>
    private void CreateRandomSnake()
    {
        int maxTile = rows * columns - 1;
        int attempts = 0;

        while (attempts < 100)
        {
            int from = Random.Range(maxTile / 2, maxTile);
            int to = Random.Range(2, from - 10);

            // Check if positions are already used
            if (!IsPositionUsed(from) && !IsPositionUsed(to))
            {
                CreateSnake(from, to);
                break;
            }

            attempts++;
        }
    }

    /// <summary>
    /// Create a ladder between two tiles
    /// </summary>
    private void CreateLadder(int from, int to)
    {
        if (!tiles.ContainsKey(from) || !tiles.ContainsKey(to))
            return;

        Vector3 startPos = tiles[from].position + Vector3.up * tileHeight;
        Vector3 endPos = tiles[to].position + Vector3.up * tileHeight;

        GameObject ladder;

        if (ladderPrefab != null)
        {
            ladder = Instantiate(ladderPrefab, startPos, Quaternion.identity, boardParent);
            // Orient ladder toward end position
            ladder.transform.LookAt(endPos);
        }
        else
        {
            // Create default ladder (cylinder)
            ladder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ladder.transform.position = (startPos + endPos) / 2f;
            ladder.transform.SetParent(boardParent);

            // Scale and rotate to connect tiles
            float distance = Vector3.Distance(startPos, endPos);
            ladder.transform.localScale = new Vector3(0.1f, distance / 2f, 0.1f);
            ladder.transform.LookAt(endPos);
            ladder.transform.Rotate(90, 0, 0);

            Renderer renderer = ladder.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = Color.green;
        }

        ladder.name = $"Ladder_{from}_to_{to}";

        snakesAndLadders.Add(new SnakeLadderData { from = from, to = to, isLadder = true });
    }

    /// <summary>
    /// Create a snake between two tiles
    /// </summary>
    private void CreateSnake(int from, int to)
    {
        if (!tiles.ContainsKey(from) || !tiles.ContainsKey(to))
            return;

        Vector3 startPos = tiles[from].position + Vector3.up * tileHeight;
        Vector3 endPos = tiles[to].position + Vector3.up * tileHeight;

        GameObject snake;

        if (snakePrefab != null)
        {
            snake = Instantiate(snakePrefab, startPos, Quaternion.identity, boardParent);
            snake.transform.LookAt(endPos);
        }
        else
        {
            // Create default snake (cylinder)
            snake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            snake.transform.position = (startPos + endPos) / 2f;
            snake.transform.SetParent(boardParent);

            // Scale and rotate to connect tiles
            float distance = Vector3.Distance(startPos, endPos);
            snake.transform.localScale = new Vector3(0.15f, distance / 2f, 0.15f);
            snake.transform.LookAt(endPos);
            snake.transform.Rotate(90, 0, 0);

            Renderer renderer = snake.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = Color.red;
        }

        snake.name = $"Snake_{from}_to_{to}";

        snakesAndLadders.Add(new SnakeLadderData { from = from, to = to, isLadder = false });
    }

    /// <summary>
    /// Check if a position is already used by a snake or ladder
    /// </summary>
    private bool IsPositionUsed(int position)
    {
        foreach (var item in snakesAndLadders)
        {
            if (item.from == position || item.to == position)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get tile number from row and column (snaking pattern)
    /// </summary>
    private int GetTileNumber(int row, int col)
    {
        int number = row * columns;

        // Snake pattern: reverse even rows
        if (row % 2 == 0)
            number += col + 1;
        else
            number += columns - col;

        return number;
    }

    /// <summary>
    /// Get world position for tile
    /// </summary>
    private Vector3 GetTilePosition(int row, int col)
    {
        float x = col * tileSize;
        float z = row * tileSize;
        return new Vector3(x, 0, z);
    }

    /// <summary>
    /// Update BoardConfig ScriptableObject with generated data
    /// </summary>
    private void UpdateBoardConfig()
    {
        if (boardConfig == null)
        {
            Debug.LogWarning("BoardConfig not assigned!");
            return;
        }

        boardConfig.jumps.Clear();

        foreach (var item in snakesAndLadders)
        {
            boardConfig.jumps.Add(new BoardJump { from = item.from, to = item.to });
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(boardConfig);
#endif

        Debug.Log($"BoardConfig updated with {snakesAndLadders.Count} snakes and ladders");
    }

    /// <summary>
    /// Get tile transform by number
    /// </summary>
    public Transform GetTile(int number)
    {
        return tiles.ContainsKey(number) ? tiles[number] : null;
    }
}
