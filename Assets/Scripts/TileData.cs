using UnityEngine;

public enum TileType
{
    Normal,
    LadderStart,
    SnakeHead
}

[CreateAssetMenu(menuName = "Game/Tile Data", fileName = "Tile_")]
public class TileData : ScriptableObject
{
    public int tileIndex;
    public TileType tileType;
    public int targetTileIndex;
}
