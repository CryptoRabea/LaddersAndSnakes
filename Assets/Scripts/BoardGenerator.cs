using UnityEngine;
using System.Collections.Generic;

namespace LaddersAndSnakes
{
    /// <summary>
    /// Generates a default 10x10 Snakes and Ladders board with visual tiles
    /// </summary>
    public class BoardGenerator : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int rows = 10;
        [SerializeField] private int columns = 10;
        [SerializeField] private float tileSize = 1.5f;
        [SerializeField] private float tileSpacing = 0.1f;

        [Header("Visual Settings")]
        [SerializeField] private Material tileMaterial;
        [SerializeField] private bool generateOnStart = true;

        [Header("Board Colors")]
        private Color[] tileColors = new Color[]
        {
            new Color(1f, 0f, 0f),      // Red
            new Color(1f, 0.5f, 0f),    // Orange
            new Color(1f, 1f, 0f),      // Yellow
            new Color(0f, 1f, 0f),      // Green
            new Color(0f, 0.5f, 1f),    // Blue
            new Color(1f, 0.4f, 0.7f),  // Pink
            new Color(0.6f, 0f, 1f),    // Purple
            new Color(1f, 1f, 1f)       // White
        };

        [Header("Snake Positions (Head to Tail)")]
        private int[,] snakes = new int[,]
        {
            {98, 78},
            {95, 75},
            {92, 73},
            {87, 36},
            {64, 60},
            {62, 19},
            {54, 34},
            {17, 7}
        };

        [Header("Ladder Positions (Bottom to Top)")]
        private int[,] ladders = new int[,]
        {
            {4, 14},
            {9, 31},
            {21, 42},
            {28, 84},
            {40, 63},
            {51, 67},
            {71, 91}
        };

        private Transform boardParent;
        private Dictionary<int, GameObject> tiles = new Dictionary<int, GameObject>();

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateBoard();
            }
        }

        /// <summary>
        /// Generates the complete board with tiles, snakes, and ladders
        /// </summary>
        public void GenerateBoard()
        {
            ClearExistingBoard();
            CreateBoardParent();
            CreateTiles();
            CreateSnakes();
            CreateLadders();
            CreateBoardBorder();

            Debug.Log("Board generated successfully with " + tiles.Count + " tiles, " +
                      snakes.GetLength(0) + " snakes, and " + ladders.GetLength(0) + " ladders.");
        }

        private void ClearExistingBoard()
        {
            // Clear existing tiles
            tiles.Clear();

            // Find and destroy existing board parent if it exists
            Transform existingBoard = transform.Find("BoardParent");
            if (existingBoard != null)
            {
                DestroyImmediate(existingBoard.gameObject);
            }
        }

        private void CreateBoardParent()
        {
            GameObject parent = new GameObject("BoardParent");
            parent.transform.SetParent(transform);
            parent.transform.localPosition = Vector3.zero;
            boardParent = parent.transform;
        }

        private void CreateTiles()
        {
            for (int tileNumber = 1; tileNumber <= rows * columns; tileNumber++)
            {
                Vector3 position = GetTilePosition(tileNumber);
                GameObject tile = CreateTile(tileNumber, position);
                tiles[tileNumber] = tile;
            }
        }

        private Vector3 GetTilePosition(int tileNumber)
        {
            // Convert tile number (1-100) to grid position
            // Numbers start at bottom-left and zigzag
            int index = tileNumber - 1;
            int row = index / columns;
            int col = index % columns;

            // Zigzag pattern: odd rows go right-to-left
            if (row % 2 == 1)
            {
                col = columns - 1 - col;
            }

            float x = col * (tileSize + tileSpacing);
            float z = row * (tileSize + tileSpacing);

            return new Vector3(x, 0, z);
        }

        private GameObject CreateTile(int tileNumber, Vector3 position)
        {
            // Create tile GameObject
            GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tile.name = "Tile_" + tileNumber;
            tile.transform.SetParent(boardParent);
            tile.transform.localPosition = position;
            tile.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);

            // Assign random color from palette
            Color tileColor = tileColors[Random.Range(0, tileColors.Length)];
            Renderer renderer = tile.GetComponent<Renderer>();

            if (tileMaterial != null)
            {
                renderer.material = new Material(tileMaterial);
            }
            renderer.material.color = tileColor;

            // Create number label
            CreateTileNumber(tile, tileNumber);

            // Mark special tiles
            if (tileNumber == 100)
            {
                // Golden trophy marker for tile 100
                renderer.material.color = new Color(1f, 0.84f, 0f); // Gold
                CreateTrophy(tile);
            }

            return tile;
        }

        private void CreateTileNumber(GameObject tile, int number)
        {
            GameObject textObj = new GameObject("Number");
            textObj.transform.SetParent(tile.transform);
            textObj.transform.localPosition = new Vector3(0, 0.06f, 0);
            textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            textObj.transform.localScale = Vector3.one * 0.3f;

            // Add TextMesh for the number
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = number.ToString();
            textMesh.fontSize = 50;
            textMesh.color = Color.black;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.1f;
        }

        private void CreateTrophy(GameObject tile)
        {
            GameObject trophy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trophy.name = "Trophy";
            trophy.transform.SetParent(tile.transform);
            trophy.transform.localPosition = new Vector3(0, 0.5f, 0);
            trophy.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);

            Renderer renderer = trophy.GetComponent<Renderer>();
            renderer.material.color = new Color(1f, 0.84f, 0f); // Gold
        }

        private void CreateSnakes()
        {
            for (int i = 0; i < snakes.GetLength(0); i++)
            {
                int head = snakes[i, 0];
                int tail = snakes[i, 1];
                CreateSnake(head, tail);
            }
        }

        private void CreateSnake(int headTile, int tailTile)
        {
            if (!tiles.ContainsKey(headTile) || !tiles.ContainsKey(tailTile))
                return;

            Vector3 headPos = tiles[headTile].transform.position + Vector3.up * 0.15f;
            Vector3 tailPos = tiles[tailTile].transform.position + Vector3.up * 0.15f;

            GameObject snake = new GameObject("Snake_" + headTile + "_to_" + tailTile);
            snake.transform.SetParent(boardParent);

            // Create snake body as a curved line
            LineRenderer line = snake.AddComponent<LineRenderer>();
            line.positionCount = 10;
            line.startWidth = 0.3f;
            line.endWidth = 0.2f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(0f, 0.5f, 0f); // Dark green
            line.endColor = new Color(0.5f, 0.8f, 0f); // Light green

            // Create curved path
            for (int i = 0; i < line.positionCount; i++)
            {
                float t = i / (float)(line.positionCount - 1);
                Vector3 point = Vector3.Lerp(headPos, tailPos, t);

                // Add curve to make it look natural
                float curve = Mathf.Sin(t * Mathf.PI) * 0.5f;
                point.y += curve;

                line.SetPosition(i, point);
            }

            // Mark the head tile
            if (tiles[headTile].TryGetComponent<Renderer>(out var headRenderer))
            {
                // Darker shade to indicate snake head
                Color currentColor = headRenderer.material.color;
                headRenderer.material.color = currentColor * 0.7f;
            }
        }

        private void CreateLadders()
        {
            for (int i = 0; i < ladders.GetLength(0); i++)
            {
                int bottom = ladders[i, 0];
                int top = ladders[i, 1];
                CreateLadder(bottom, top);
            }
        }

        private void CreateLadder(int bottomTile, int topTile)
        {
            if (!tiles.ContainsKey(bottomTile) || !tiles.ContainsKey(topTile))
                return;

            Vector3 bottomPos = tiles[bottomTile].transform.position + Vector3.up * 0.15f;
            Vector3 topPos = tiles[topTile].transform.position + Vector3.up * 0.15f;

            GameObject ladder = new GameObject("Ladder_" + bottomTile + "_to_" + topTile);
            ladder.transform.SetParent(boardParent);
            ladder.transform.position = bottomPos;

            // Create ladder sides
            CreateLadderSide(ladder, bottomPos, topPos, -0.15f);
            CreateLadderSide(ladder, bottomPos, topPos, 0.15f);

            // Create ladder rungs
            int rungCount = 5;
            for (int i = 0; i <= rungCount; i++)
            {
                float t = i / (float)rungCount;
                Vector3 rungPos = Vector3.Lerp(bottomPos, topPos, t);
                CreateLadderRung(ladder, rungPos);
            }

            // Mark the bottom tile
            if (tiles[bottomTile].TryGetComponent<Renderer>(out var bottomRenderer))
            {
                // Brighter shade to indicate ladder start
                Color currentColor = bottomRenderer.material.color;
                bottomRenderer.material.color = currentColor * 1.2f;
            }
        }

        private void CreateLadderSide(GameObject parent, Vector3 bottom, Vector3 top, float offset)
        {
            GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            side.name = "Side";
            side.transform.SetParent(parent.transform);

            Vector3 center = (bottom + top) / 2f;
            Vector3 offsetDir = Vector3.Cross((top - bottom).normalized, Vector3.up).normalized;
            center += offsetDir * offset;

            side.transform.position = center;
            side.transform.up = (top - bottom).normalized;

            float length = Vector3.Distance(bottom, top);
            side.transform.localScale = new Vector3(0.05f, length / 2f, 0.05f);

            Renderer renderer = side.GetComponent<Renderer>();
            renderer.material.color = new Color(0.6f, 0f, 1f); // Purple
        }

        private void CreateLadderRung(GameObject parent, Vector3 position)
        {
            GameObject rung = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rung.name = "Rung";
            rung.transform.SetParent(parent.transform);
            rung.transform.position = position;
            rung.transform.rotation = Quaternion.Euler(0, 0, 90);
            rung.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);

            Renderer renderer = rung.GetComponent<Renderer>();
            renderer.material.color = Color.white;
        }

        private void CreateBoardBorder()
        {
            GameObject border = new GameObject("BoardBorder");
            border.transform.SetParent(boardParent);

            float boardWidth = columns * (tileSize + tileSpacing);
            float boardHeight = rows * (tileSize + tileSpacing);
            float borderThickness = 0.2f;
            float borderHeight = 0.3f;

            // Create four border walls
            CreateBorderWall(border, new Vector3(boardWidth / 2f, borderHeight / 2f, -borderThickness / 2f),
                           new Vector3(boardWidth, borderHeight, borderThickness)); // Bottom
            CreateBorderWall(border, new Vector3(boardWidth / 2f, borderHeight / 2f, boardHeight + borderThickness / 2f),
                           new Vector3(boardWidth, borderHeight, borderThickness)); // Top
            CreateBorderWall(border, new Vector3(-borderThickness / 2f, borderHeight / 2f, boardHeight / 2f),
                           new Vector3(borderThickness, borderHeight, boardHeight)); // Left
            CreateBorderWall(border, new Vector3(boardWidth + borderThickness / 2f, borderHeight / 2f, boardHeight / 2f),
                           new Vector3(borderThickness, borderHeight, boardHeight)); // Right
        }

        private void CreateBorderWall(GameObject parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "BorderWall";
            wall.transform.SetParent(parent.transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;

            Renderer renderer = wall.GetComponent<Renderer>();
            renderer.material.color = new Color(1f, 0.84f, 0f); // Gold
        }

        /// <summary>
        /// Returns the snake and ladder configuration for BoardConfig
        /// </summary>
        public List<BoardJump> GetBoardJumps()
        {
            List<BoardJump> jumps = new List<BoardJump>();

            // Add snakes
            for (int i = 0; i < snakes.GetLength(0); i++)
            {
                jumps.Add(new BoardJump { from = snakes[i, 0], to = snakes[i, 1] });
            }

            // Add ladders
            for (int i = 0; i < ladders.GetLength(0); i++)
            {
                jumps.Add(new BoardJump { from = ladders[i, 0], to = ladders[i, 1] });
            }

            return jumps;
        }
    }
}
