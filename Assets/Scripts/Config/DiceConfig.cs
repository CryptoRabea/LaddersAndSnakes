using UnityEngine;


namespace LAS.Config
{
    [CreateAssetMenu(menuName = "LAS/DiceConfig")]
    public class DiceConfig : ScriptableObject
    {
        public int sides = 6;
        public float rollDuration = 1.2f;
        public Vector3 spinTorque = new Vector3(400, 400, 400);
    }
}