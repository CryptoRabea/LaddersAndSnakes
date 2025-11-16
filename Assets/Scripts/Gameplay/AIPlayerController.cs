using System.Collections;
using UnityEngine;
using LAS.Core;
using LAS.Events;
using LAS.Entities;
using LAS.Networking;

namespace LAS.Gameplay
{
    /// <summary>
    /// Controls AI player behavior - automatically rolls dice when it's the AI's turn
    /// </summary>
    public class AIPlayerController : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private float thinkDelay = 1.0f; // Delay before AI rolls the dice
        [SerializeField] private int aiPlayerIndex = 1; // Which player index is the AI (default: player 1)

        private IEventBus eventBus;
        private NetworkManager networkManager;
        private DiceModel diceModel;
        private bool isProcessing = false;

        private void Awake()
        {
            eventBus = ServiceLocator.Get<IEventBus>();
            networkManager = NetworkManager.Instance;
        }

        private void Start()
        {
            // Subscribe to turn events
            if (eventBus != null)
            {
                eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            }
            else
            {
                Debug.LogWarning("[AIPlayerController] EventBus not found in ServiceLocator");
            }

            // Check if AI should go first (unlikely in single player, but handle it)
            StartCoroutine(CheckInitialTurn());
        }

        private void EnsureDiceModel()
        {
            // Lazy initialization of DiceModel
            if (diceModel == null)
            {
                diceModel = FindFirstObjectByType<DiceModel>();
                if (diceModel == null)
                {
                    Debug.LogWarning("[AIPlayerController] DiceModel not found in scene");
                }
            }
        }

        private IEnumerator CheckInitialTurn()
        {
            // Wait a frame for all components to initialize
            yield return null;

            // Only process if we're in single player AI mode
            if (networkManager == null || !networkManager.IsSinglePlayerAI)
                yield break;

            var gameController = GetComponent<MultiplayerGameController>();
            if (gameController == null)
            {
                gameController = FindFirstObjectByType<MultiplayerGameController>();
            }

            // If it's the AI's turn at game start, make the first move
            if (gameController != null && gameController.CurrentPlayer == aiPlayerIndex)
            {
                yield return StartCoroutine(AITurnRoutine());
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (eventBus != null)
            {
                eventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            // Only process if we're in single player AI mode
            if (networkManager == null || !networkManager.IsSinglePlayerAI)
                return;

            // Get the game controller to check whose turn it is
            var gameController = GetComponent<MultiplayerGameController>();
            if (gameController == null)
            {
                gameController = FindFirstObjectByType<MultiplayerGameController>();
            }

            if (gameController == null)
                return;

            // Check if it's the AI's turn (next player after turn ended)
            if (gameController.CurrentPlayer == aiPlayerIndex && !isProcessing)
            {
                StartCoroutine(AITurnRoutine());
            }
        }

        private IEnumerator AITurnRoutine()
        {
            isProcessing = true;

            // Wait a bit to simulate "thinking"
            yield return new WaitForSeconds(thinkDelay);

            // Ensure we have a reference to DiceModel
            EnsureDiceModel();

            // Roll the dice for the AI player
            if (diceModel != null)
            {
                Debug.Log($"[AIPlayerController] AI is rolling the dice");
                diceModel.RollDice(aiPlayerIndex);
            }
            else
            {
                Debug.LogError("[AIPlayerController] Cannot roll dice - DiceModel is still null after search");
            }

            isProcessing = false;
        }
    }
}
