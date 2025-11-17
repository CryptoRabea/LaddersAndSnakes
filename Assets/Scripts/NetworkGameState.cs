using UnityEngine;
using Fusion;

/// <summary>
/// Synchronized game state for networked Ladders and Snakes
/// Handles turn management, player positions, and dice rolls across network
/// </summary>
public class NetworkGameState : NetworkBehaviour
{
    [Header("Game State")]
    [Networked] public int CurrentPlayer { get; set; }
    [Networked] public int LastDiceRoll { get; set; }
    [Networked] public bool IsRolling { get; set; }
    [Networked] public int NumberOfPlayers { get; set; }
    [Networked] public int WinnerIndex { get; set; }
    [Networked] public bool GameEnded { get; set; }

    // Player positions (supports up to 4 players)
    [Networked] public int Player0Position { get; set; }
    [Networked] public int Player1Position { get; set; }
    [Networked] public int Player2Position { get; set; }
    [Networked] public int Player3Position { get; set; }

    // Reference to local game manager
    private ManualGameManager _gameManager;

    // Events for game state changes
    public System.Action<int> OnDiceRolled;
    public System.Action<int, int> OnPlayerMoved; // playerIndex, newPosition
    public System.Action<int> OnTurnChanged;
    public System.Action<int> OnGameEnded;

    public override void Spawned()
    {
        base.Spawned();

        // Initialize game state if we're the host
        if (Object.HasStateAuthority)
        {
            CurrentPlayer = 0;
            LastDiceRoll = 0;
            IsRolling = false;
            NumberOfPlayers = 2; // Default to 2 players
            WinnerIndex = -1;
            GameEnded = false;

            Player0Position = 0;
            Player1Position = 0;
            Player2Position = 0;
            Player3Position = 0;

            Debug.Log("NetworkGameState initialized as host!");
        }

        Debug.Log($"NetworkGameState spawned. HasStateAuthority: {Object.HasStateAuthority}");
    }

    /// <summary>
    /// Set the number of players (Host only)
    /// </summary>
    public void SetNumberOfPlayers(int count)
    {
        if (!Object.HasStateAuthority)
        {
            Debug.LogWarning("Only host can set number of players!");
            return;
        }

        NumberOfPlayers = Mathf.Clamp(count, 2, 4);
        Debug.Log($"Number of players set to: {NumberOfPlayers}");
    }

    /// <summary>
    /// Request to roll dice (called by current player)
    /// </summary>
    public void RequestRollDice()
    {
        if (IsRolling)
        {
            Debug.LogWarning("Already rolling!");
            return;
        }

        if (GameEnded)
        {
            Debug.LogWarning("Game has ended!");
            return;
        }

        // Request RPC to roll dice
        RPC_RollDice();
    }

    /// <summary>
    /// RPC to roll dice - callable by any player during their turn
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RollDice()
    {
        if (IsRolling || GameEnded) return;

        IsRolling = true;

        // Generate random dice result (1-6 for single die, 2-12 for double dice)
        int diceResult = Random.Range(1, 7); // Single die for now

        Debug.Log($"Host rolled dice: {diceResult}");

        // Broadcast result to all clients
        RPC_BroadcastDiceResult(diceResult);
    }

    /// <summary>
    /// Broadcast dice result to all clients
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastDiceResult(int result)
    {
        LastDiceRoll = result;
        Debug.Log($"Dice result received: {result}");

        OnDiceRolled?.Invoke(result);
    }

    /// <summary>
    /// Move player to new position (Host only)
    /// </summary>
    public void MovePlayer(int playerIndex, int newPosition)
    {
        if (!Object.HasStateAuthority)
        {
            // Request move from host
            RPC_RequestMove(playerIndex, newPosition);
            return;
        }

        SetPlayerPosition(playerIndex, newPosition);
        RPC_BroadcastPlayerMove(playerIndex, newPosition);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestMove(int playerIndex, int newPosition)
    {
        SetPlayerPosition(playerIndex, newPosition);
        RPC_BroadcastPlayerMove(playerIndex, newPosition);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastPlayerMove(int playerIndex, int newPosition)
    {
        SetPlayerPosition(playerIndex, newPosition);
        OnPlayerMoved?.Invoke(playerIndex, newPosition);
        Debug.Log($"Player {playerIndex} moved to position {newPosition}");
    }

    /// <summary>
    /// Set player position in networked state
    /// </summary>
    void SetPlayerPosition(int playerIndex, int position)
    {
        switch (playerIndex)
        {
            case 0: Player0Position = position; break;
            case 1: Player1Position = position; break;
            case 2: Player2Position = position; break;
            case 3: Player3Position = position; break;
            default: Debug.LogWarning($"Invalid player index: {playerIndex}"); break;
        }
    }

    /// <summary>
    /// Get player position from networked state
    /// </summary>
    public int GetPlayerPosition(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return Player0Position;
            case 1: return Player1Position;
            case 2: return Player2Position;
            case 3: return Player3Position;
            default:
                Debug.LogWarning($"Invalid player index: {playerIndex}");
                return 0;
        }
    }

    /// <summary>
    /// Advance to next player's turn (Host only)
    /// </summary>
    public void NextTurn()
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestNextTurn();
            return;
        }

        IsRolling = false;
        CurrentPlayer = (CurrentPlayer + 1) % NumberOfPlayers;

        RPC_BroadcastTurnChange(CurrentPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestNextTurn()
    {
        NextTurn();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastTurnChange(int newCurrentPlayer)
    {
        CurrentPlayer = newCurrentPlayer;
        OnTurnChanged?.Invoke(CurrentPlayer);
        Debug.Log($"Turn changed to Player {CurrentPlayer}");
    }

    /// <summary>
    /// End the game with a winner (Host only)
    /// </summary>
    public void EndGame(int winnerIndex)
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestEndGame(winnerIndex);
            return;
        }

        GameEnded = true;
        WinnerIndex = winnerIndex;

        RPC_BroadcastGameEnd(winnerIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestEndGame(int winnerIndex)
    {
        EndGame(winnerIndex);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastGameEnd(int winnerIndex)
    {
        GameEnded = true;
        WinnerIndex = winnerIndex;
        OnGameEnded?.Invoke(winnerIndex);
        Debug.Log($"Game ended! Winner: Player {winnerIndex}");
    }

    /// <summary>
    /// Reset game state (Host only)
    /// </summary>
    public void ResetGame()
    {
        if (!Object.HasStateAuthority)
        {
            RPC_RequestResetGame();
            return;
        }

        CurrentPlayer = 0;
        LastDiceRoll = 0;
        IsRolling = false;
        WinnerIndex = -1;
        GameEnded = false;

        Player0Position = 0;
        Player1Position = 0;
        Player2Position = 0;
        Player3Position = 0;

        RPC_BroadcastGameReset();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestResetGame()
    {
        ResetGame();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastGameReset()
    {
        Debug.Log("Game reset!");
        // Game manager will handle local reset
    }

    /// <summary>
    /// Set reference to game manager
    /// </summary>
    public void SetGameManager(ManualGameManager manager)
    {
        _gameManager = manager;
    }
}
