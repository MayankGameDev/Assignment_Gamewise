using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Puzzle
{
    public class TileSpawner
    {
        private readonly int _fourChance;

        public TileSpawner(int fourChancePercent = 10)
        {
            _fourChance = Mathf.Clamp(fourChancePercent, 0, 100);
        }
        
        public bool TrySpawn(Board board, out int index, out int value)
        {
            var empties = board.CountEmpty();
            if (empties == 0)
            {
                index = -1;
                value = 0;
                return false;
            }
            
            index = board.IndexOfNthEmpty(Random.Range(0, empties));
            value = Random.Range(0, 100) < _fourChance ? 4 : 2;

            board.SetAt(index, value);
            return true;
        }
        
        public int SpawnMany(Board board, int count)
        {
            var spawned = 0;
            for (var i = 0; i < count; i++)
            {
                if (!TrySpawn(board, out _, out _)) break;
                spawned++;
            }

            return spawned;
        }
    }
}