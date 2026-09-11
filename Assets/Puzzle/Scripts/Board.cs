namespace Puzzle
{
    
    public class Board
    {
        public const int Wall = -1;

        private readonly int[] _cells;

        public Board(int width, int height)
        {
            if (width < 2 || height < 2)
            {
                throw new System.ArgumentException($"Grid must be at least 2x2, got {width}x{height}.");
            }

            Width = width;
            Height = height;
            _cells = new int[width * height];
        }

        public int Width { get; }

        public int Height { get; }

        public int CellCount => _cells.Length;

        public int this[int x, int y]
        {
            get => _cells[Index(x, y)];
            set => _cells[Index(x, y)] = value;
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
        
        public int Index(int x, int y)
        {
            if (!InBounds(x, y))
            {
                throw new System.IndexOutOfRangeException(
                    $"({x}, {y}) is outside a {Width}x{Height} grid.");
            }

            return (y * Width) + x;
        }

        public void CoordAt(int index, out int x, out int y)
        {
            x = index % Width;
            y = index / Width;
        }

        public int CellAt(int index) => _cells[index];

        public void SetAt(int index, int value) => _cells[index] = value;

        public bool IsEmpty(int x, int y) => this[x, y] == 0;

        public bool IsWall(int x, int y) => this[x, y] == Wall;

        public int CountEmpty()
        {
            var count = 0;
            for (var i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == 0) count++;
            }

            return count;
        }
        
        public int IndexOfNthEmpty(int number)
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != 0) continue;
                if (number == 0) return i;
                number--;
            }

            return -1;
        }
        
        public int HighestValue()
        {
            var highest = 0;
            for (var i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] > highest) highest = _cells[i];
            }

            return highest;
        }

        public void Clear()
        {
            System.Array.Clear(_cells, 0, _cells.Length);
        }
    }
}