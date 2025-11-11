using System.Collections;
using UnityEngine;
using LAS.Config;
using LAS.Core;


namespace LAS.Entities
{
    [RequireComponent(typeof(Animator))]
    public class DiceView : MonoBehaviour, IPoolable
    {
        public Animator animator;
        public DiceConfig config;
        void Awake() { animator = GetComponent<Animator>(); }
        public void OnSpawn() { }
        public void OnDespawn() { }
        public void RollVisual(int finalFace)
        {
            if (animator != null) { animator.SetInteger("Face", finalFace); animator.SetTrigger("Roll"); }
            else StartCoroutine(SimpleSpinCoroutine(config.rollDuration));
        }
        IEnumerator SimpleSpinCoroutine(float duration)
        {
            float t = 0f; while (t < duration) { t += Time.deltaTime; transform.Rotate(config.spinTorque * Time.deltaTime); yield return null; }
        }
    }
}