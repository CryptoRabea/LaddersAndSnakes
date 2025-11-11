using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LAS.Core;
using LAS.Events;
using LAS.Config;
using LAS.Gameplay;


namespace LAS.Entities
{
    /// <summary>
    /// Handles player piece movement along the board
    /// </summary>
    public class MovementSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardModel boardModel;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private GameController gameController;

        private PathFollower follower;
        private IEventBus bus;

        void Awake()
        {
            follower = gameObject.AddComponent<PathFollower>();
        }

        void OnEnable()
        {
            bus = ServiceLocator.Get<IEventBus>();
            bus?.Subscribe<MoveRequestedEvent>(OnMoveRequested);
        }

        void OnDisable()
        {
            bus?.Unsubscribe<MoveRequestedEvent>(OnMoveRequested);
        }

        void OnMoveRequested(MoveRequestedEvent evt)
        {
            StartCoroutine(HandleMove(evt));
        }

        IEnumerator HandleMove(MoveRequestedEvent evt)
        {
            // Get player manager and piece
            PlayerManager playerManager = gameController?.GetPlayerManager();
            if (playerManager == null)
            {
                Debug.LogError("PlayerManager not found!");
                yield break;
            }

            PlayerPiece piece = playerManager.GetPlayerPiece(evt.playerIndex);
            if (piece == null)
            {
                Debug.LogError($"Player piece {evt.playerIndex} not found!");
                yield break;
            }

            Transform pieceTrans = piece.transform;
            int from = piece.currentIndex;
            int target = from + evt.steps;

            // Check for exact roll to win
            if (gameController.RequiresExactRollToWin())
            {
                if (target > gameController.GetWinningPosition())
                {
                    Debug.Log($"Player {evt.playerIndex} needs exact roll to win. Staying at {from}");
                    gameController.EndTurn();
                    yield break;
                }
            }
            else
            {
                // Clamp to winning position
                target = Mathf.Min(target, gameController.GetWinningPosition());
            }

            // Build path positions
            List<Vector3> path = new List<Vector3>();
            for (int i = from + 1; i <= target; i++)
            {
                Vector3 pos = boardManager != null ? boardManager.GetTilePosition(i) : Vector3.zero;
                path.Add(pos);
            }

            // Animate movement
            piece.PlayMoveAnim();
            yield return StartCoroutine(follower.MoveAlongPath(pieceTrans, path, gameConfig.moveSpeed, gameConfig.moveCurve));

            int landed = target;

            // Check for snakes or ladders
            int jumped = boardModel.ApplyJumps(landed);

            if (jumped != landed)
            {
                // Determine if it's a snake or ladder
                bool isLadder = jumped > landed;

                if (isLadder)
                {
                    bus?.Publish(new LadderHitEvent
                    {
                        playerIndex = evt.playerIndex,
                        from = landed,
                        to = jumped
                    });
                    Debug.Log($"Player {evt.playerIndex} climbed a ladder from {landed} to {jumped}!");
                }
                else
                {
                    bus?.Publish(new SnakeHitEvent
                    {
                        playerIndex = evt.playerIndex,
                        from = landed,
                        to = jumped
                    });
                    Debug.Log($"Player {evt.playerIndex} hit a snake from {landed} to {jumped}!");
                }

                // Animate jump/slide
                Vector3 jumpStart = boardManager != null ? boardManager.GetTilePosition(landed) : pieceTrans.position;
                Vector3 jumpEnd = boardManager != null ? boardManager.GetTilePosition(jumped) : Vector3.zero;

                var jumpPath = new List<Vector3>() { jumpStart, jumpEnd };
                yield return StartCoroutine(follower.MoveAlongPath(pieceTrans, jumpPath, gameConfig.moveSpeed * 1.4f, gameConfig.moveCurve));

                landed = jumped;
            }

            // Update piece position
            piece.currentIndex = landed;
            piece.PlayIdle();

            // Publish move completed event
            bus?.Publish(new PieceMovedEvent
            {
                playerIndex = evt.playerIndex,
                from = from,
                to = landed
            });

            // End turn
            if (gameController != null)
            {
                gameController.EndTurn();
            }
        }
    }
}
