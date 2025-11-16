using UnityEngine;
using LAS.Core;
using LAS.Config;
using LAS.Events;


namespace LAS.Gameplay
{
    public class GameController : MonoBehaviour
    {
        public int CurrentPlayer { get; protected set; } = 0;
        protected GameState state;

        protected virtual void Awake()
        {
            // bootstrap core services
            ServiceLocator.Register<IEventBus>(new EventBus());
            ServiceLocator.Register(new PoolManager());
        }

        protected virtual void Start()
        {
            state = new IdleState();
            state.Enter(this);
            ServiceLocator.Get<IEventBus>()?.Subscribe<DiceRolledEvent>(OnDiceRolled);
        }

        protected virtual void OnDestroy()
        {
            ServiceLocator.Get<IEventBus>()?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
            ServiceLocator.Clear();
        }

        protected virtual void OnDiceRolled(DiceRolledEvent evt)
        {
            state.OnDiceRolled(evt.result);
        }

        public virtual void TransitionTo(GameState newState)
        {
            state?.Exit();
            state = newState;
            state.Enter(this);
        }

        public virtual void EndTurn()
        {
            ServiceLocator.Get<IEventBus>()?.Publish(new TurnEndedEvent { playerIndex = CurrentPlayer });
            CurrentPlayer = (CurrentPlayer + 1) % 2;
            TransitionTo(new IdleState());
        }
    }
}