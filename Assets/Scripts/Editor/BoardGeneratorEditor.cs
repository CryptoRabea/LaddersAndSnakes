using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for BoardGenerator with automated board creation button
/// </summary>
[CustomEditor(typeof(BoardGenerator))]
public class BoardGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardGenerator generator = (BoardGenerator)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Click the button below to automatically generate the game board with tiles, snakes, and ladders.",
            MessageType.Info);

        if (GUILayout.Button("Generate Board", GUILayout.Height(40)))
        {
            GenerateBoard(generator);
        }

        EditorGUILayout.Space();

        if (generator.transform.childCount > 0)
        {
            EditorGUILayout.HelpBox(
                $"Board has {generator.transform.childCount} child objects. " +
                "Clear them before regenerating if needed.",
                MessageType.Warning);

            if (GUILayout.Button("Clear Existing Board", GUILayout.Height(30)))
            {
                ClearBoard(generator);
            }
        }
    }

    private void GenerateBoard(BoardGenerator generator)
    {
        // Clear existing board first
        ClearBoard(generator);

        // Generate tiles
        CreateTiles(generator);

        // Generate snakes and ladders
        CreateSnakesAndLadders(generator);

        // Update BoardConfig
        UpdateBoardConfig(generator);

        EditorUtility.DisplayDialog(
            "Board Generated!",
            $"Successfully created:\n\n" +
            $"• {generator.rows * generator.columns} tiles\n" +
            $"• {generator.snakeCount} snakes\n" +
            $"• {generator.ladderCount} ladders\n\n" +
            "BoardConfig has been updated.",
            "OK");

        EditorUtility.SetDirty(generator);
    }

    private void ClearBoard(BoardGenerator generator)
    {
        while (generator.transform.childCount > 0)
        {
            DestroyImmediate(generator.transform.GetChild(0).gameObject);
        }
        Debug.Log("Cleared existing board");
    }

    private void CreateTiles(BoardGenerator generator)
    {
        GameObject tilesParent = new GameObject("Tiles");
        tilesParent.transform.SetParent(generator.transform);

        int tileIndex = 0;
        for (int row = 0; row < generator.rows; row++)
        {
            for (int col = 0; col < generator.columns; col++)
            {
                Vector3 position = CalculateTilePosition(row, col, generator);

                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile_{tileIndex}";
                tile.transform.SetParent(tilesParent.transform);
                tile.transform.position = position;
                tile.transform.localScale = new Vector3(generator.tileSize * 0.9f, 0.1f, generator.tileSize * 0.9f);

                // Add BoardSquare component
                BoardSquare square = tile.AddComponent<BoardSquare>();
                square.tileIndex = tileIndex;

                // Alternate colors for checkerboard pattern
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = (row + col) % 2 == 0 ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.7f, 0.7f, 0.7f);
                tile.GetComponent<Renderer>().material = mat;

                // Add tile number text (optional - creates TextMesh for debugging)
                CreateTileNumberLabel(tile, tileIndex, position);

                tileIndex++;
            }
        }

        Debug.Log($"Created {tileIndex} tiles");
    }

    private void CreateTileNumberLabel(GameObject tile, int number, Vector3 position)
    {
        GameObject label = new GameObject($"Label_{number}");
        label.transform.SetParent(tile.transform);
        label.transform.position = position + Vector3.up * 0.1f;
        label.transform.rotation = Quaternion.Euler(90, 0, 0);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = number.ToString();
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.black;
    }

    private void CreateSnakesAndLadders(BoardGenerator generator)
    {
        int totalTiles = generator.rows * generator.columns;
        System.Collections.Generic.HashSet<int> usedTiles = new System.Collections.Generic.HashSet<int>();

        // Create snakes (red cylinders going down)
        GameObject snakesParent = new GameObject("Snakes");
        snakesParent.transform.SetParent(generator.transform);

        for (int i = 0; i < generator.snakeCount; i++)
        {
            int head = Random.Range(20, totalTiles - 10); // Start from mid-board
            int tail = Random.Range(5, head - 10); // Go down

            // Ensure unique tiles
            while (usedTiles.Contains(head) || usedTiles.Contains(tail))
            {
                head = Random.Range(20, totalTiles - 10);
                tail = Random.Range(5, head - 10);
            }

            usedTiles.Add(head);
            usedTiles.Add(tail);

            CreateSnake(snakesParent.transform, head, tail, generator);
        }

        // Create ladders (green cylinders going up)
        GameObject laddersParent = new GameObject("Ladders");
        laddersParent.transform.SetParent(generator.transform);

        for (int i = 0; i < generator.ladderCount; i++)
        {
            int bottom = Random.Range(5, totalTiles - 20); // Start from lower area
            int top = Random.Range(bottom + 10, totalTiles - 5); // Go up

            // Ensure unique tiles
            while (usedTiles.Contains(bottom) || usedTiles.Contains(top))
            {
                bottom = Random.Range(5, totalTiles - 20);
                top = Random.Range(bottom + 10, totalTiles - 5);
            }

            usedTiles.Add(bottom);
            usedTiles.Add(top);

            CreateLadder(laddersParent.transform, bottom, top, generator);
        }

        Debug.Log($"Created {generator.snakeCount} snakes and {generator.ladderCount} ladders");
    }

    private void CreateSnake(Transform parent, int head, int tail, BoardGenerator generator)
    {
        Vector3 headPos = GetTilePosition(head, generator);
        Vector3 tailPos = GetTilePosition(tail, generator);

        GameObject snake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        snake.name = $"Snake_{head}_to_{tail}";
        snake.transform.SetParent(parent);

        // Position and scale cylinder to connect head and tail
        Vector3 midpoint = (headPos + tailPos) / 2f + Vector3.up * 0.5f;
        float distance = Vector3.Distance(headPos, tailPos);

        snake.transform.position = midpoint;
        snake.transform.localScale = new Vector3(0.2f, distance / 2f, 0.2f);
        snake.transform.LookAt(tailPos);
        snake.transform.Rotate(90, 0, 0);

        // Red material for snakes
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        snake.GetComponent<Renderer>().material = mat;

        // Add labels
        CreateEndpointLabel(headPos + Vector3.up * 0.6f, $"S{head}", Color.red, parent);
        CreateEndpointLabel(tailPos + Vector3.up * 0.6f, $"{tail}", Color.red, parent);
    }

    private void CreateLadder(Transform parent, int bottom, int top, BoardGenerator generator)
    {
        Vector3 bottomPos = GetTilePosition(bottom, generator);
        Vector3 topPos = GetTilePosition(top, generator);

        GameObject ladder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ladder.name = $"Ladder_{bottom}_to_{top}";
        ladder.transform.SetParent(parent);

        // Position and scale cylinder to connect bottom and top
        Vector3 midpoint = (bottomPos + topPos) / 2f + Vector3.up * 0.5f;
        float distance = Vector3.Distance(bottomPos, topPos);

        ladder.transform.position = midpoint;
        ladder.transform.localScale = new Vector3(0.2f, distance / 2f, 0.2f);
        ladder.transform.LookAt(topPos);
        ladder.transform.Rotate(90, 0, 0);

        // Green material for ladders
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        ladder.GetComponent<Renderer>().material = mat;

        // Add labels
        CreateEndpointLabel(bottomPos + Vector3.up * 0.6f, $"L{bottom}", Color.green, parent);
        CreateEndpointLabel(topPos + Vector3.up * 0.6f, $"{top}", Color.green, parent);
    }

    private void CreateEndpointLabel(Vector3 position, string text, Color color, Transform parent)
    {
        GameObject label = new GameObject($"Label_{text}");
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.transform.rotation = Quaternion.Euler(90, 0, 0);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 30;
        textMesh.characterSize = 0.08f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = color;
        textMesh.fontStyle = FontStyle.Bold;
    }

    private Vector3 CalculateTilePosition(int row, int col, BoardGenerator generator)
    {
        // Snake-like pattern (alternate row direction)
        float x = (row % 2 == 0) ? col * generator.tileSize : (generator.columns - 1 - col) * generator.tileSize;
        float z = row * generator.tileSize;
        return new Vector3(x, 0, z);
    }

    private Vector3 GetTilePosition(int tileIndex, BoardGenerator generator)
    {
        int row = tileIndex / generator.columns;
        int col = tileIndex % generator.columns;

        // Reverse column for odd rows (snake pattern)
        if (row % 2 == 1)
        {
            col = generator.columns - 1 - col;
        }

        return CalculateTilePosition(row, col, generator);
    }

    private void UpdateBoardConfig(BoardGenerator generator)
    {
        // Find or create BoardConfig
        string configPath = "Assets/ScriptableObjects/BoardConfig.asset";
        BoardConfig config = AssetDatabase.LoadAssetAtPath<BoardConfig>(configPath);

        if (config == null)
        {
            Debug.LogWarning("BoardConfig not found at expected path. Skipping config update.");
            return;
        }

        // Update config with snake and ladder data
        SerializedObject so = new SerializedObject(config);

        // Find all snakes and ladders in the board
        Transform snakesParent = generator.transform.Find("Snakes");
        Transform laddersParent = generator.transform.Find("Ladders");

        // Note: You would need to add fields to BoardConfig to store this data
        // For now, just mark it dirty
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();

        Debug.Log("BoardConfig updated");
    }
}
