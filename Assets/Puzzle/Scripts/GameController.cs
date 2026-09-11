using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private SwipeDetector _input;
        [SerializeField] private GameOverPanel _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _scoreLabel;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _shuffleButton;

        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 4;

        [SerializeField] private int _startingTiles = 2;

        [Tooltip("Chance a new tile is a 4 rather than a 2.")] [Range(0, 100)] [SerializeField]
        private int _fourChance = 10;

        [SerializeField] private int _seed;

        [Tooltip("Reach this tile to win.")] [SerializeField]
        private int _winValue = 2048;

        [Tooltip("Number of blocked cells placed on the board at the start of each game.")] [SerializeField]
        private int _wallCount = 2;

        private Board _board;
        private TileSpawner _spawner;
        private MoveResolver _resolver;
        private MoveHistory _history;
        private bool _targetReached;
        private bool _animating;
        private readonly List<TileMove> _moves = new List<TileMove>();

        public Board Board => _board;

        [SerializeField] private int _undoLimit = 20;

        public int Score { get; private set; }
        public bool CanUndo => _history != null && _history.CanUndo;
        public GameState State { get; private set; }

        private void Awake()
        {
            _board = new Board(_width, _height);
            _spawner = new TileSpawner(_fourChance);
            _resolver = new MoveResolver(Mathf.Max(_width, _height));
            _history = new MoveHistory(_undoLimit);

            _boardView.Initialize(_board);
        }

        private void OnEnable()
        {
            if (_input != null) _input.Swiped += OnSwiped;
            if (_undoButton != null) _undoButton.onClick.AddListener(OnUndoClicked);
            if (_shuffleButton != null) _shuffleButton.onClick.AddListener(OnShuffleClicked);
            if (_gameOverPanel != null)
            {
                _gameOverPanel.NewGamePressed += NewGame;
                _gameOverPanel.SecondaryPressed += OnSecondaryPressed;
            }
        }

        private void OnDisable()
        {
            if (_input != null) _input.Swiped -= OnSwiped;
            if (_undoButton != null) _undoButton.onClick.RemoveListener(OnUndoClicked);
            if (_shuffleButton != null) _shuffleButton.onClick.RemoveListener(OnShuffleClicked);
        }

        private void Start()
        {
            NewGame();
        }

        [ContextMenu("New Game")]
        public void NewGame()
        {
            _board.Clear();
            _history.Clear();
            PlaceWalls();
            _spawner.SpawnMany(_board, _startingTiles);
            _boardView.Render();
            Score = 0;
            State = GameState.Playing;
            _targetReached = false;
            _animating = false;
            RefreshUi();
            if (_gameOverPanel != null) _gameOverPanel.Hide();
        }

        private void PlaceWalls()
        {
            var maxWalls = Mathf.Max(0, _board.CellCount - _startingTiles - 1);
            var count = Mathf.Clamp(_wallCount, 0, maxWalls);

            var placed = 0;
            var guard = 0;

            while (placed < count && guard < 1000)
            {
                guard++;

                var index = Random.Range(0, _board.CellCount);
                _board.CoordAt(index, out var x, out var y);

                if (!_board.IsEmpty(x, y)) continue;

                _board[x, y] = Board.Wall;
                placed++;
            }
        }

        private void OnSwiped(Direction direction)
        {
            if (_animating) return;

            _history.Push(_board, Score, Random.state);

            if (!_resolver.Move(_board, direction, out var gained, _moves))
            {
                _history.Discard();
                return;
            }

            Score += gained;
            _animating = true;

            _boardView.AnimateMoves(_moves, () =>
            {
                _spawner.TrySpawn(_board, out var spawnIndex, out _);
                _boardView.ShowNewTile(spawnIndex);

                // IMPORTANT: flip _animating back BEFORE calling anything
                // that reads it (CheckEndOfGame / RefreshUi), otherwise
                // RefreshUi sees the stale "still animating" value and
                // leaves the Shuffle button disabled until something else
                // (like Undo) happens to call RefreshUi later.
                _animating = false;
                CheckEndOfGame();
                RefreshUi();
            });
        }

        private void CheckEndOfGame()
        {
            if (!_targetReached && _board.HighestValue() >= _winValue)
            {
                _targetReached = true;
                State = GameState.Won;

                if (_gameOverPanel != null)
                {
                    _gameOverPanel.Show(
                        "You win!",
                        $"You reached {_winValue:N0} with {Score:N0} points.",
                        "Keep going");
                }

                return;
            }

            if (MoveResolver.HasAnyMove(_board)) return;
            Debug.Log("Game Lost before 2048");
            State = GameState.Lost;

            if (_gameOverPanel != null)
            {
                _gameOverPanel.Show(
                    "Game over",
                    $"No moves left. Final score {Score:N0}.",
                    CanUndo ? "Undo last move" : null);
            }
        }

        private void OnSecondaryPressed()
        {
            if (State == GameState.Won)
            {
                State = GameState.Playing;
                if (_gameOverPanel != null) _gameOverPanel.Hide();
                return;
            }

            Undo();
        }

        private System.Random NewRandom()
        {
            return _seed == 0 ? new System.Random() : new System.Random(_seed);
        }

        public bool Undo()
        {
            if (_animating) return false;

            if (!_history.TryPop(_board, out var score, out var randomState))
            {
                RefreshUi();
                return false;
            }

            Score = score;
            Random.state = randomState;

            _boardView.Render();
            RefreshUi();
            return true;
        }

        private void OnUndoClicked()
        {
            Undo();
        }

        private bool CanShuffle()
        {
            var movable = 0;
            for (var i = 0; i < _board.CellCount; i++)
            {
                if (_board.CellAt(i) != Board.Wall) movable++;
            }

            return movable >= 2;
        }

        private void OnShuffleClicked()
        {
            if (_animating) return;
            if (State != GameState.Playing) return;
            if (!CanShuffle()) return;

            _history.Push(_board, Score, Random.state);
            _animating = true;

            ShuffleBoardValues();

            _boardView.AnimateShuffle(() =>
            {
                // Same fix as OnSwiped: reset the flag before anything
                // that reads it.
                _animating = false;
                CheckEndOfGame();
                RefreshUi();
            });
        }

        private void ShuffleBoardValues()
        {
            var indices = new List<int>();
            for (var i = 0; i < _board.CellCount; i++)
            {
                if (_board.CellAt(i) != Board.Wall) indices.Add(i);
            }

            var values = new List<int>(indices.Count);
            foreach (var index in indices) values.Add(_board.CellAt(index));

            for (var i = values.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                var temp = values[i];
                values[i] = values[j];
                values[j] = temp;
            }

            for (var k = 0; k < indices.Count; k++)
            {
                _board.SetAt(indices[k], values[k]);
            }
        }

        private void RefreshUi()
        {
            if (_undoButton != null) _undoButton.interactable = CanUndo;
            if (_shuffleButton != null) _shuffleButton.interactable = !_animating && State == GameState.Playing && CanShuffle();
            if (_scoreLabel != null) _scoreLabel.text = Score.ToString("N0");
        }
    }
}