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
        
        public bool Move(Board board, Direction direction, out int scoreGained)
        {
            var vertical = direction == Direction.Up || direction == Direction.Down;
            var lineCount = vertical ? board.Width : board.Height;
            var lineLength = vertical ? board.Height : board.Width;

            var changed = false;
            scoreGained = 0;

            for (var line = 0; line < lineCount; line++)
            {
                if (ResolveLine(board, direction, line, lineLength, out var lineScore)) changed = true;
                scoreGained += lineScore;
            }

            return changed;
        }

        private bool ResolveLine(
            Board board, Direction direction, int line, int length, out int score)
        {
            for (var p = 0; p < length; p++)
            {
                CoordFor(board, direction, line, p, out var x, out var y);
                _line[p] = board[x, y];
                _result[p] = 0;
            }

            Collapse(length, out score);

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
        
        private void Collapse(int length, out int score)
        {
            score = 0;
            var write = 0;
            var open = -1;

            for (var read = 0; read < length; read++)
            {
                var value = _line[read];
                if (value == 0) continue;

                if (open >= 0 && _result[open] == value)
                {
                    var merged = value * 2;
                    _result[open] = merged;
                    score += merged;
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
        
        public static bool HasAnyMove(Board board)
        {
            if (board.CountEmpty() > 0) return true;

            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var value = board[x, y];

                    if (x + 1 < board.Width && board[x + 1, y] == value) return true;
                    if (y + 1 < board.Height && board[x, y + 1] == value) return true;
                }
            }

            return false;
        }
    }
}
