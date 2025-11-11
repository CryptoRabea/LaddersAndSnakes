using UnityEngine;
using LAS.Core;
using LAS.Events;


namespace LAS.Gameplay
{
    public abstract class GameState
    {
        protected GameController controller;
        public virtual void Enter(GameController c) { controller = c; }
        public virtual void Exit() { }
        public virtual void OnDiceRolled(int value) { }
    }


    public class IdleState : GameState
    {
        public override void Enter(GameController c) { base.Enter(c); }
        public override void OnDiceRolled(int value) { controller.TransitionTo(new MovingState(value)); }
    }


    public class MovingState : GameState
    {
        int steps;
        public MovingState(int steps) { this.steps = steps; }
        public override void Enter(GameController c)
        {
            base.Enter(c);
            ServiceLocator.Get<IEventBus>()?.Publish(new MoveRequestedEvent { playerIndex = c.CurrentPlayer, steps = steps });
        }
    }
}