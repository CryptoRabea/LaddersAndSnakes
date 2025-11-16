using System.Collections;
using UnityEngine;
using LAS.Core;
using LAS.Config;
using LAS.Events;


namespace LAS.Entities
{
    public class DiceModel : MonoBehaviour
    {
        public DiceConfig config;
        public DiceView diceView;
        IEventBus bus; PoolManager pool; public GameObject dicePrefab;
        void Awake() { bus = ServiceLocator.Get<IEventBus>(); pool = ServiceLocator.Get<PoolManager>(); }

        // Simple roll method for UI
        public void Roll() { StartCoroutine(RollRoutine(0)); }

        public void RollDice(int who) { StartCoroutine(RollRoutine(who)); }
        IEnumerator RollRoutine(int who)
        {
            int raw = UnityEngine.Random.Range(1, config.sides + 1);

            // Use pooling if available, otherwise use attached view
            if (pool != null && dicePrefab != null)
            {
                var diceGO = pool.Spawn(dicePrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
                var view = diceGO.GetComponent<DiceView>();
                if (view != null)
                {
                    view.config = config;
                    view.RollVisual(raw);
                }
                yield return new WaitForSeconds(config.rollDuration);
                bus?.Publish(new DiceRolledEvent { result = raw, rawRoll = raw });
                pool.Despawn(dicePrefab, diceGO);
            }
            else if (diceView != null)
            {
                // Fallback to attached view
                diceView.RollVisual(raw);
                yield return new WaitForSeconds(config.rollDuration);
                bus?.Publish(new DiceRolledEvent { result = raw, rawRoll = raw });
            }
            else
            {
                // No visual, just publish event
                yield return new WaitForSeconds(config.rollDuration);
                bus?.Publish(new DiceRolledEvent { result = raw, rawRoll = raw });
            }
        }
    }
}