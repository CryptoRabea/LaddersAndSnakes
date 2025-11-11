using UnityEngine;


namespace LAS.Config
{
    [CreateAssetMenu(menuName = "LAS/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public int boardSize = 100;
        public float moveSpeed = 4f; // units/sec
        public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}