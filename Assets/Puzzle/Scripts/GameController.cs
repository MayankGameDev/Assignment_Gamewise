using UnityEngine;

namespace Puzzle
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private SwipeDetector _input;

        [SerializeField] private int _width = 4;
        [SerializeField] private int _height = 4;

        [SerializeField]
        private int _startingTiles = 2;

        [Tooltip("Chance a new tile is a 4 rather than a 2.")] [Range(0, 100)] [SerializeField]
        private int _fourChance = 10;

        [SerializeField] private int _seed;

        private Board _board;
        private TileSpawner _spawner;
        private MoveResolver _resolver;

        public Board Board => _board;

        private void Awake()
        {
            _board = new Board(_width, _height);
            _spawner = new TileSpawner(NewRandom(), _fourChance);
            _resolver = new MoveResolver(Mathf.Max(_width, _height));

            _boardView.Initialize(_board);
        }

        private void OnEnable()
        {
            if (_input != null) _input.Swiped += OnSwiped;
        }

        private void OnDisable()
        {
            if (_input != null) _input.Swiped -= OnSwiped;
        }

        private void Start()
        {
            NewGame();
        }

        [ContextMenu("New Game")]
        public void NewGame()
        {
            _board.Clear();
            _spawner.SpawnMany(_board, _startingTiles);
            _boardView.Render();
        }

        private void OnSwiped(Direction direction)
        {
            if (!_resolver.Move(_board, direction)) return;

            _spawner.TrySpawn(_board, out _, out _);
            _boardView.Render();
        }


        private System.Random NewRandom()
        {
            return _seed == 0 ? new System.Random() : new System.Random(_seed);
        }
    }
}