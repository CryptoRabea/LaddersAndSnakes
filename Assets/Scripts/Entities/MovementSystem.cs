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
        void Awake() { follower = gameObject.AddComponent<PathFollower>(); }
        void OnEnable() { bus = ServiceLocator.Get<IEventBus>(); bus?.Subscribe<MoveRequestedEvent>(OnMoveRequested); }
        void OnDisable() { bus?.Unsubscribe<MoveRequestedEvent>(OnMoveRequested); }
        void OnMoveRequested(MoveRequestedEvent evt) { StartCoroutine(HandleMove(evt)); }
        IEnumerator HandleMove(MoveRequestedEvent evt)
        {
            var pieceTrans = playerPieces[evt.playerIndex]; var piece = pieceTrans.GetComponent<PlayerPiece>();
            int from = piece.currentIndex; int target = Mathf.Min(from + evt.steps, squareTransforms.Length);
            // build path positions
            List<Vector3> path = new List<Vector3>(); for (int i = from + 1; i <= target; i++) path.Add(squareTransforms[i - 1].position);
            piece.PlayMoveAnim(); yield return StartCoroutine(follower.MoveAlongPath(pieceTrans, path, gameConfig.moveSpeed, gameConfig.moveCurve));
            int landed = target; int jumped = boardModel.ApplyJumps(landed);
            if (jumped != landed)
            {
                var jumpPath = new List<Vector3>() { pieceTrans.position, squareTransforms[jumped - 1].position };
                yield return StartCoroutine(follower.MoveAlongPath(pieceTrans, jumpPath, gameConfig.moveSpeed * 1.4f, gameConfig.moveCurve));
                landed = jumped;
            }
            piece.currentIndex = landed; piece.PlayIdle(); bus?.Publish(new PieceMovedEvent { playerIndex = evt.playerIndex, from = from, to = landed });

            // Check for win condition (reached the final square)
            if (landed >= squareTransforms.Length)
            {
                Debug.Log($"Player {evt.playerIndex} wins!");
                bus?.Publish(new GameOverEvent { winnerIndex = evt.playerIndex });
                yield break; // Don't end turn, game is over
            }

            var ctrl = Object.FindAnyObjectByType<Gameplay.GameController>(); ctrl?.EndTurn();
        }
    }
}