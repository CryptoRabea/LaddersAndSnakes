using System;
using System.Collections.Generic;
using UnityEngine;


namespace LAS.Config
{
    [CreateAssetMenu(menuName = "LAS/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        public List<BoardJump> jumps = new List<BoardJump>();

    }


    [Serializable]
    public struct BoardJump
    {
        public int from;
        public int to;
        public bool isLadder;

        public BoardJump(int from, int to, bool isLadder = false)
        {
            this.from = from;
            this.to = to;
            this.isLadder = isLadder;
        }

        // Helper properties for compatibility with new generator
        public int fromTile => from;
        public int toTile => to;
    }
}