namespace Puzzle
{
    
    public class MoveResolver
    {
        private readonly int[] _line;
        private readonly int[] _result;

        public MoveResolver(int maxLineLength)
        {
            _line = new int[maxLineLength];
            _result = new int[maxLineLength];
        }
        
        public bool Move(Board board, Direction direction)
        {
            var vertical = direction == Direction.Up || direction == Direction.Down;
            var lineCount = vertical ? board.Width : board.Height;
            var lineLength = vertical ? board.Height : board.Width;

            var changed = false;
            for (var line = 0; line < lineCount; line++)
            {
                if (ResolveLine(board, direction, line, lineLength)) changed = true;
            }

            return changed;
        }

        private bool ResolveLine(Board board, Direction direction, int line, int length)
        {
            for (var p = 0; p < length; p++)
            {
                CoordFor(board, direction, line, p, out var x, out var y);
                _line[p] = board[x, y];
                _result[p] = 0;
            }

            Collapse(length);

            var changed = false;
            for (var p = 0; p < length; p++)
            {
                if (_result[p] == _line[p]) continue;

                CoordFor(board, direction, line, p, out var x, out var y);
                board[x, y] = _result[p];
                changed = true;
            }

            return changed;
        }
        
        private void Collapse(int length)
        {
            var write = 0;
            
            var open = -1;

            for (var read = 0; read < length; read++)
            {
                var value = _line[read];
                if (value == 0) continue;

                if (open >= 0 && _result[open] == value)
                {
                    _result[open] = value * 2;
                    open = -1;
                    continue;
                }

                _result[write] = value;
                open = write;
                write++;
            }
        }
        
        private static void CoordFor(
            Board board, Direction direction, int line, int position, out int x, out int y)
        {
            switch (direction)
            {
                case Direction.Up:
                    x = line;
                    y = board.Height - 1 - position;
                    break;

                case Direction.Down:
                    x = line;
                    y = position;
                    break;

                case Direction.Right:
                    x = board.Width - 1 - position;
                    y = line;
                    break;

                default:
                    x = position;
                    y = line;
                    break;
            }
        }
    }
}
