using UnityEngine;
using UnityEngine.UI;
using LAS.Core;
using LAS.Events;
using LAS.Config;

namespace LAS.Entities
{
    /// <summary>
    /// Handles dice rolling logic and visual representation
    /// </summary>
    public class DiceController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DiceConfig config;
        [SerializeField] private DiceView diceView;
        [SerializeField] private Button rollButton;
        [SerializeField] private TMPro.TextMeshProUGUI resultText;

        [Header("Settings")]
        [SerializeField] private bool autoRollEnabled = false;
        [SerializeField] private float autoRollDelay = 0.5f;

        private IEventBus eventBus;
        private bool canRoll = true;
        private int lastRollResult;

        private void Awake()
        {
            eventBus = ServiceLocator.Get<IEventBus>();

            if (rollButton != null)
            {
                rollButton.onClick.AddListener(OnRollButtonClicked);
            }
        }

        private void OnEnable()
        {
            eventBus?.Subscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus?.Subscribe<GameStartedEvent>(OnGameStarted);
        }

        private void OnDisable()
        {
            eventBus?.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus?.Unsubscribe<GameStartedEvent>(OnGameStarted);
        }

        private void OnGameStarted(GameStartedEvent evt)
        {
            EnableRoll();
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            EnableRoll();
        }

        private void OnRollButtonClicked()
        {
            if (canRoll)
            {
                RollDice();
            }
        }

        /// <summary>
        /// Rolls the dice and publishes the result
        /// </summary>
        public void RollDice()
        {
            if (!canRoll) return;

            canRoll = false;
            if (rollButton != null) rollButton.interactable = false;

            // Generate random result
            int result = Random.Range(1, config.sides + 1);
            lastRollResult = result;

            // Play visual animation
            if (diceView != null)
            {
                diceView.RollVisual(result);
            }

            // Update result text
            if (resultText != null)
            {
                resultText.text = $"Rolled: {result}";
            }

            // Publish event after animation delay
            Invoke(nameof(PublishRollResult), config.rollDuration);
        }

        private void PublishRollResult()
        {
            eventBus?.Publish(new DiceRolledEvent
            {
                result = lastRollResult,
                rawRoll = lastRollResult
            });

            Debug.Log($"Dice rolled: {lastRollResult}");
        }

        private void EnableRoll()
        {
            if (autoRollEnabled)
            {
                Invoke(nameof(RollDice), autoRollDelay);
            }
            else
            {
                canRoll = true;
                if (rollButton != null) rollButton.interactable = true;
            }
        }

        public void SetAutoRoll(bool enabled)
        {
            autoRollEnabled = enabled;
        }

        /// <summary>
        /// Force roll with a specific value (for testing)
        /// </summary>
        public void ForceRoll(int value)
        {
            if (value < 1 || value > config.sides) return;

            lastRollResult = value;

            if (diceView != null)
            {
                diceView.RollVisual(value);
            }

            if (resultText != null)
            {
                resultText.text = $"Rolled: {value}";
            }

            Invoke(nameof(PublishRollResult), config.rollDuration);
        }
    }
}
