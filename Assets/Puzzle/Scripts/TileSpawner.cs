using System;

namespace Puzzle
{
    public class TileSpawner
    {
        private readonly Random _rng;
        private readonly int _fourChance;

        public TileSpawner(Random rng, int fourChancePercent = 10)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _fourChance = Math.Min(Math.Max(fourChancePercent, 0), 100);
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

            index = board.IndexOfNthEmpty(_rng.Next(empties));
            value = _rng.Next(100) < _fourChance ? 4 : 2;
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