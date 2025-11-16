using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LAS.Core;
using LAS.Events;
using LAS.Config;


namespace LAS.Entities
{
    public class MovementSystem : MonoBehaviour
    {
        public Transform[] playerPieces; // inspector assign
        public BoardModel boardModel;
        public GameConfig gameConfig;
        public Transform[] squareTransforms; // 1-based mapping
        PathFollower follower;
        IEventBus bus;
        Gameplay.GameController gameController;

        void Awake()
        {
            follower = gameObject.AddComponent<PathFollower>();
        }
        void OnEnable()
        {
            bus = ServiceLocator.Get<IEventBus>();
            if (bus != null)
            {
                bus.Subscribe<MoveRequestedEvent>(OnMoveRequested);
            }
            else
            {
                Debug.LogWarning("[MovementSystem] EventBus not found in ServiceLocator");
            }

            // Cache GameController reference
            if (gameController == null)
            {
                gameController = Object.FindAnyObjectByType<Gameplay.GameController>();
                if (gameController == null)
                {
                    Debug.LogWarning("[MovementSystem] GameController not found in scene");
                }
            }
        }
        void OnDisable()
        {
            if (bus != null)
            {
                bus.Unsubscribe<MoveRequestedEvent>(OnMoveRequested);
            }
        }
        void OnMoveRequested(MoveRequestedEvent evt) { StartCoroutine(HandleMove(evt)); }
        IEnumerator HandleMove(MoveRequestedEvent evt)
        {
            // Validate player index
            if (playerPieces == null || evt.playerIndex < 0 || evt.playerIndex >= playerPieces.Length)
            {
                Debug.LogError($"[MovementSystem] Invalid player index {evt.playerIndex} or playerPieces is null");
                yield break;
            }

            var pieceTrans = playerPieces[evt.playerIndex];
            if (pieceTrans == null)
            {
                Debug.LogError($"[MovementSystem] Player piece transform at index {evt.playerIndex} is null");
                yield break;
            }

            var piece = pieceTrans.GetComponent<PlayerPiece>();
            if (piece == null)
            {
                Debug.LogError($"[MovementSystem] PlayerPiece component not found on player {evt.playerIndex}");
                yield break;
            }

            // Validate squareTransforms
            if (squareTransforms == null || squareTransforms.Length == 0)
            {
                Debug.LogError("[MovementSystem] squareTransforms is null or empty");
                yield break;
            }

            int from = piece.currentIndex;
            int target = Mathf.Min(from + evt.steps, squareTransforms.Length);

            // build path positions
            List<Vector3> path = new List<Vector3>();
            for (int i = from + 1; i <= target; i++)
            {
                if (i - 1 >= 0 && i - 1 < squareTransforms.Length && squareTransforms[i - 1] != null)
                {
                    path.Add(squareTransforms[i - 1].position);
                }
                else
                {
                    Debug.LogError($"[MovementSystem] Square transform at index {i - 1} is null or out of bounds");
                    yield break;
                }
            }
            piece.PlayMoveAnim();
            yield return StartCoroutine(follower.MoveAlongPath(pieceTrans, path, gameConfig.moveSpeed, gameConfig.moveCurve));

            int landed = target;

            // Validate boardModel before applying jumps
            if (boardModel == null)
            {
                Debug.LogError("[MovementSystem] boardModel is null, cannot apply jumps");
            }
            else
            {
                int jumped = boardModel.ApplyJumps(landed);
                if (jumped != landed)
                {
                    // Validate jump target
                    if (jumped - 1 >= 0 && jumped - 1 < squareTransforms.Length && squareTransforms[jumped - 1] != null)
                    {
                        var jumpPath = new List<Vector3>() { pieceTrans.position, squareTransforms[jumped - 1].position };
                        yield return StartCoroutine(follower.MoveAlongPath(pieceTrans, jumpPath, gameConfig.moveSpeed * 1.4f, gameConfig.moveCurve));
                        landed = jumped;
                    }
                    else
                    {
                        Debug.LogError($"[MovementSystem] Jump target square at index {jumped - 1} is null or out of bounds");
                    }
                }
            }
            piece.currentIndex = landed; piece.PlayIdle(); bus?.Publish(new PieceMovedEvent { playerIndex = evt.playerIndex, from = from, to = landed });

            // Check for win condition (reached the final square)
            if (landed >= squareTransforms.Length)
            {
                Debug.Log($"Player {evt.playerIndex} wins!");
                bus?.Publish(new GameOverEvent { winnerIndex = evt.playerIndex });
                yield break; // Don't end turn, game is over
            }

            // End turn using cached controller reference
            if (gameController != null)
            {
                gameController.EndTurn();
            }
            else
            {
                Debug.LogWarning("[MovementSystem] Cannot end turn - GameController is null");
            }
        }
    }
}