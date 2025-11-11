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
        IEventBus bus; PoolManager pool; public GameObject dicePrefab;
        void Awake() { bus = ServiceLocator.Get<IEventBus>(); pool = ServiceLocator.Get<PoolManager>(); }
        public void RollDice(int who) { StartCoroutine(RollRoutine(who)); }
        IEnumerator RollRoutine(int who)
        {
            int raw = UnityEngine.Random.Range(1, config.sides + 1);
            var diceGO = pool.Spawn(dicePrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
            var view = diceGO.GetComponent<DiceView>(); view.config = config; view.RollVisual(raw);
            yield return new WaitForSeconds(config.rollDuration);
            bus?.Publish(new DiceRolledEvent { result = raw, rawRoll = raw });
            pool.Despawn(dicePrefab, diceGO);
        }
    }
}