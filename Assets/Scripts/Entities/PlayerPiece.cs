using UnityEngine;
using LAS.Core;


namespace LAS.Entities
{
    [RequireComponent(typeof(Animator))]
    public class PlayerPiece : MonoBehaviour, IPoolable
    {
        public int playerIndex;
        public int currentIndex = 1;
        Animator animator;
        void Awake() { animator = GetComponent<Animator>(); }
        public void OnSpawn() { /* reset state */ }
        public void OnDespawn() { /* cleanup */ }
        public void PlayMoveAnim() { if (animator != null) animator.SetTrigger("Move"); }
        public void PlayIdle() { if (animator != null) animator.SetTrigger("Idle"); }
    }
}