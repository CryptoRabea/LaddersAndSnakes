using System;
using System.Collections.Generic;
using UnityEngine;


namespace LAS.Config
{
    [CreateAssetMenu(menuName = "LAS/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        public List<BoardJump> jumps = new List<BoardJump>();
        public TileData[] tiles;

    }


    [Serializable]
    public struct BoardJump { public int from; public int to; }
}