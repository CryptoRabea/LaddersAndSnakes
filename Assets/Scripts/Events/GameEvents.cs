namespace LAS.Events
{
    public struct DiceRolledEvent { public int result; public int rawRoll; }
    public struct MoveRequestedEvent { public int playerIndex; public int steps; }
    public struct PieceMovedEvent { public int playerIndex; public int from; public int to; }
    public struct TurnEndedEvent { public int playerIndex; }
    public struct GameStartedEvent { public int playerCount; }
    public struct GameOverEvent { public int winnerIndex; public string winnerName; }
    public struct PlayerJoinedEvent { public int playerIndex; public string playerName; }
    public struct SnakeHitEvent { public int playerIndex; public int from; public int to; }
    public struct LadderHitEvent { public int playerIndex; public int from; public int to; }
}