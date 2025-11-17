using UnityEngine;
using Unity.Netcode;
using LAS.Core;
using LAS.Events;
using LAS.Entities;

namespace LAS.Networking
{
    /// <summary>
    /// Networked dice controller that synchronizes dice rolls across clients
    /// Only the current player can roll the dice, and results are broadcast to all players
    /// </summary>
    public class NetworkDiceController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DiceModel diceModel;

        private NetworkedGameManager networkManager;
        private NetworkedMultiplayerGameController gameController;
        private IEventBus eventBus;
        private bool isInitialized = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;

            // Get references
            eventBus = ServiceLocator.Get<IEventBus>();
            networkManager = NetworkedGameManager.Instance;
            gameController = FindObjectOfType<NetworkedMultiplayerGameController>();

            if (diceModel == null)
            {
                diceModel = GetComponent<DiceModel>();
            }

            if (diceModel == null)
            {
                Debug.LogError("[NetworkDiceController] DiceModel not found!");
                return;
            }

            // Subscribe to local dice roll requests
            if (eventBus != null)
            {
                eventBus.Subscribe<DiceRollRequestEvent>(OnDiceRollRequested);
            }

            isInitialized = true;
            Debug.Log("[NetworkDiceController] Initialized");
        }

        private void OnDestroy()
        {
            if (eventBus != null)
            {
                eventBus.Unsubscribe<DiceRollRequestEvent>(OnDiceRollRequested);
            }
        }

        private void OnDiceRollRequested(DiceRollRequestEvent evt)
        {
            // Check if it's this player's turn in networked game
            if (gameController != null && gameController.IsNetworkedGame())
            {
                if (!gameController.CanPlayerAct())
                {
                    Debug.LogWarning("[NetworkDiceController] Not your turn!");
                    return;
                }
            }

            // Perform local dice roll
            RollDice();
        }

        /// <summary>
        /// Roll the dice and broadcast result
        /// </summary>
        public void RollDice()
        {
            if (diceModel == null)
            {
                Debug.LogError("[NetworkDiceController] DiceModel is null!");
                return;
            }

            // Roll the dice locally
            int result = diceModel.Roll();
            int rawRoll = result; // You can modify this if you have special dice logic

            Debug.Log($"[NetworkDiceController] Rolled: {result}");

            // Broadcast the result
            if (gameController != null && gameController.IsNetworkedGame())
            {
                gameController.BroadcastDiceRoll(result, rawRoll);
            }
            else
            {
                // Local game - publish directly
                eventBus?.Publish(new DiceRolledEvent { result = result, rawRoll = rawRoll });
            }
        }
    }

    /// <summary>
    /// Event to request a dice roll
    /// </summary>
    public struct DiceRollRequestEvent { }
}
