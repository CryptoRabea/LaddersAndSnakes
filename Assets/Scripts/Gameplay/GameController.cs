using UnityEngine;
using LAS.Core;
using LAS.Config;
using LAS.Events;


namespace LAS.Gameplay
{
    public class GameController : MonoBehaviour
    {
        public int CurrentPlayer { get; private set; } = 0;
        GameState state;
        void Awake()
        {
            // bootstrap core services
            ServiceLocator.Register<IEventBus>(new EventBus());
            ServiceLocator.Register(new PoolManager());
        }
        void Start()
        {
            state = new IdleState(); state.Enter(this);
            ServiceLocator.Get<IEventBus>()?.Subscribe<DiceRolledEvent>(OnDiceRolled);
        }
        void OnDestroy()
        {
            ServiceLocator.Get<IEventBus>()?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
            ServiceLocator.Clear();
        }
        void OnDiceRolled(DiceRolledEvent evt) { state.OnDiceRolled(evt.result); }
        public void TransitionTo(GameState newState) { state?.Exit(); state = newState; state.Enter(this); }
        public void EndTurn() { ServiceLocator.Get<IEventBus>()?.Publish(new TurnEndedEvent { playerIndex = CurrentPlayer }); CurrentPlayer = (CurrentPlayer + 1) % 2; TransitionTo(new IdleState()); }
    }
}