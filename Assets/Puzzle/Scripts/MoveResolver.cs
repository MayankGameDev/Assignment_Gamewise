using System.Collections.Generic;

namespace Puzzle
{
    
    public class MoveResolver
    {
        private readonly int[] _line;
        private readonly int[] _result;
        private readonly int[] _moveTarget;
        private readonly bool[] _merged;

        public MoveResolver(int maxLineLength)
        {
            _line = new int[maxLineLength];
            _result = new int[maxLineLength];
            _moveTarget = new int[maxLineLength];
            _merged = new bool[maxLineLength];
        }
        
        public bool Move(Board board, Direction direction, out int scoreGained, List<TileMove> moves = null)
        {
            moves?.Clear();

            var vertical = direction == Direction.Up || direction == Direction.Down;
            var lineCount = vertical ? board.Width : board.Height;
            var lineLength = vertical ? board.Height : board.Width;

            var changed = false;
            scoreGained = 0;

            for (var line = 0; line < lineCount; line++)
            {
                if (ResolveLine(board, direction, line, lineLength, out var lineScore, moves)) changed = true;
                scoreGained += lineScore;
            }

            return changed;
        }

        private bool ResolveLine(
            Board board, Direction direction, int line, int length, out int score, List<TileMove> moves)
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
                if (_result[p] != _line[p]) changed = true;
            }

            if (moves != null)
            {
                for (var p = 0; p < length; p++)
                {
                    if (_line[p] == 0) continue;

                    var targetPos = _moveTarget[p];
                    CoordFor(board, direction, line, p, out var fromX, out var fromY);
                    CoordFor(board, direction, line, targetPos, out var toX, out var toY);

                    moves.Add(new TileMove
                    {
                        FromX = fromX,
                        FromY = fromY,
                        ToX = toX,
                        ToY = toY,
                        Value = _line[p],
                        ResultValue = _result[targetPos],
                        Merged = _merged[p]
                    });
                }
            }

            for (var p = 0; p < length; p++)
            {
                CoordFor(board, direction, line, p, out var x, out var y);
                board[x, y] = _result[p];
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
                if (value == 0)
                {
                    _moveTarget[read] = -1;
                    _merged[read] = false;
                    continue;
                }

                if (open >= 0 && _result[open] == value)
                {
                    var merged = value * 2;
                    _result[open] = merged;
                    score += merged;
                    _moveTarget[read] = open;
                    _merged[read] = true;
                    open = -1;
                    continue;
                }

                _result[write] = value;
                _moveTarget[read] = write;
                _merged[read] = false;
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