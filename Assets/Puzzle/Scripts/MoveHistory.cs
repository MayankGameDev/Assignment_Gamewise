using System.Collections.Generic;
using UnityEngine;

namespace Puzzle
{
    public class MoveHistory
    {
        private class Snapshot
        {
            public int[] Cells;
            public int Score;
            public Random.State RandomState;
        }
        
        private readonly List<Snapshot> _snapshots = new List<Snapshot>();
        private readonly int _capacity;

        public MoveHistory(int capacity = 20)
        {
            _capacity = capacity < 1 ? 1 : capacity;
        }

        public int Count => _snapshots.Count;

        public bool CanUndo => _snapshots.Count > 0;


        public void Push(Board board, int score, Random.State randomState)
        {
            var cells = new int[board.CellCount];
            for (var i = 0; i < cells.Length; i++)
            {
                cells[i] = board.CellAt(i);
            }
            _snapshots.Add(new Snapshot { Cells = cells, Score = score, RandomState = randomState });
            if (_snapshots.Count > _capacity)
            {
                _snapshots.RemoveAt(0);
            }
        }

        public bool TryPop(Board board, out int score, out Random.State randomState)
        {
            score = 0;
            randomState = default;

            if (_snapshots.Count == 0) return false;

            var last = _snapshots.Count - 1;
            var snapshot = _snapshots[last];

            if (snapshot.Cells.Length != board.CellCount)
            {
                
                Clear();
                return false;
            }

            for (var i = 0; i < snapshot.Cells.Length; i++)
            {
                board.SetAt(i, snapshot.Cells[i]);
            }

            score = snapshot.Score;
            randomState = snapshot.RandomState;

            _snapshots.RemoveAt(last);
            return true;
        }


        public void Discard()
        {
            if (_snapshots.Count == 0) return;

            _snapshots.RemoveAt(_snapshots.Count - 1);
        }

        public void Clear()
        {
            _snapshots.Clear();
        }
    }
}