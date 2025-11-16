namespace LAS.Events
{
    public struct DiceRolledEvent { public int result; public int rawRoll; }
    public struct MoveRequestedEvent { public int playerIndex; public int steps; }
    public struct PieceMovedEvent { public int playerIndex; public int from; public int to; }
    public struct TurnEndedEvent { public int playerIndex; }
    public struct GameOverEvent { public int winnerIndex; }
}