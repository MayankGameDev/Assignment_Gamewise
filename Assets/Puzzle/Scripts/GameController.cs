using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private SwipeDetector _input;

        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 4;

        [SerializeField] private int _startingTiles = 2;

        [Tooltip("Chance a new tile is a 4 rather than a 2.")] [Range(0, 100)] [SerializeField]
        private int _fourChance = 10;

        [SerializeField] private int _seed;

        private Board _board;
        private TileSpawner _spawner;
        private MoveResolver _resolver;
        private MoveHistory _history;

        public Board Board => _board;

        [SerializeField] private int _undoLimit = 20;
        [SerializeField] private Button _undoButton;
        public bool CanUndo => _history != null && _history.CanUndo;

        [SerializeField] private TextMeshProUGUI _scoreLabel;
        public int Score { get; private set; }

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
        }

        private void OnDisable()
        {
            if (_input != null) _input.Swiped -= OnSwiped;
            if (_undoButton != null) _undoButton.onClick.RemoveListener(OnUndoClicked);
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
            _spawner.SpawnMany(_board, _startingTiles);
            _boardView.Render();
            RefreshUndoButton();
            Score = 0;
        }

        private void OnSwiped(Direction direction)
        {
            _history.Push(_board, Score, Random.state);

            if (!_resolver.Move(_board, direction, out var gained))
            {
                _history.Discard();
                return;
            }

            Score += gained;

            _spawner.TrySpawn(_board, out _, out _);
            _boardView.Render();
            RefreshUi();
        }


        private System.Random NewRandom()
        {
            return _seed == 0 ? new System.Random() : new System.Random(_seed);
        }

        public bool Undo()
        {
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

        private void RefreshUndoButton()
        {
            if (_undoButton != null) _undoButton.interactable = CanUndo;
        }

        private void RefreshUi()
        {
            if (_undoButton != null) _undoButton.interactable = CanUndo;
            if (_scoreLabel != null) _scoreLabel.text = Score.ToString("N0");
        }
    }
}