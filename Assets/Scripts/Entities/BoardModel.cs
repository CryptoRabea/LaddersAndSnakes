using System.Collections.Generic;
using UnityEngine;
using LAS.Config;


namespace LAS.Entities
{
    public class BoardModel : MonoBehaviour
    {
        public BoardConfig config;
        public Dictionary<int, int> jumps = new Dictionary<int, int>();
        void Awake() { if (config != null) foreach (var j in config.jumps) jumps[j.from] = j.to; }
        public int ApplyJumps(int position) { return jumps.TryGetValue(position, out var to) ? to : position; }
    }
}