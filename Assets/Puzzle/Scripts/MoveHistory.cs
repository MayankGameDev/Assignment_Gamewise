using System.Collections.Generic;

namespace Puzzle
{
    public class MoveHistory
    {
        private readonly List<int[]> _snapshots = new List<int[]>();
        private readonly int _capacity;

        public MoveHistory(int capacity = 20)
        {
            _capacity = capacity < 1 ? 1 : capacity;
        }

        public int Count => _snapshots.Count;

        public bool CanUndo => _snapshots.Count > 0;


        public void Push(Board board)
        {
            var snapshot = new int[board.CellCount];
            for (var i = 0; i < snapshot.Length; i++)
            {
                snapshot[i] = board.CellAt(i);
            }

            _snapshots.Add(snapshot);

            if (_snapshots.Count > _capacity)
            {
                _snapshots.RemoveAt(0);
            }
        }

        public bool TryPop(Board board)
        {
            if (_snapshots.Count == 0) return false;

            var last = _snapshots.Count - 1;
            var snapshot = _snapshots[last];

            if (snapshot.Length != board.CellCount)
            {
                Clear();
                return false;
            }

            for (var i = 0; i < snapshot.Length; i++)
            {
                board.SetAt(i, snapshot[i]);
            }

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