using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private int _width = 4;


        [SerializeField] private int _height = 4;


        [SerializeField] private int _startingTiles = 2;

        [Tooltip("Chance a new tile is a 4 rather than a 2.")] [Range(0, 100)] [SerializeField]
        private int _fourChance = 10;
        
        [SerializeField] private int _seed;
        
        [SerializeField] private float _spacing = 12f;
        [SerializeField] private Color _cellColor = new Color(0.80f, 0.76f, 0.71f);


        [SerializeField] private Color[] _tileColors =
        {
            new Color(0.93f, 0.89f, 0.85f), // 2
            new Color(0.93f, 0.88f, 0.78f), // 4
            new Color(0.95f, 0.69f, 0.47f), // 8
            new Color(0.96f, 0.58f, 0.39f), // 16
            new Color(0.96f, 0.49f, 0.37f), // 32
            new Color(0.96f, 0.37f, 0.23f), // 64
            new Color(0.93f, 0.81f, 0.45f), // 128
            new Color(0.93f, 0.79f, 0.35f), // 256
            new Color(0.93f, 0.77f, 0.25f), // 512
            new Color(0.93f, 0.76f, 0.18f), // 1024
            new Color(0.24f, 0.77f, 0.62f) // 2048 and up
        };

        [SerializeField] private Color _darkText = new Color(0.36f, 0.33f, 0.30f);
        [SerializeField] private Color _lightText = new Color(0.98f, 0.96f, 0.94f);

        [SerializeField] private int _lightTextFrom = 8;

        private Board _board;
        private TileSpawner _spawner;

        private RectTransform _rect;
        private RectTransform _cellRoot;
        private RectTransform _tileRoot;

        private RectTransform[] _cells;
        private TileView[] _tiles;

        private float _cellSize;
        private Vector2 _origin;

        public Board Board => _board;

        private RectTransform Rect
        {
            get
            {
                if (_rect == null) _rect = (RectTransform)transform;
                return _rect;
            }
        }

        private void Awake()
        {
            Build();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_cells == null) return;

            Measure();
            ApplyLayout();
        }

        [ContextMenu("Rebuild")]
        public void Build()
        {
            _board = new Board(_width, _height);
            _spawner = new TileSpawner(NewRandom(), _fourChance);

            ClearChildren();
            _cellRoot = CreateLayer("Cells");
            _tileRoot = CreateLayer("Tiles");

            _cells = new RectTransform[_board.CellCount];
            _tiles = new TileView[_board.CellCount];

            for (var i = 0; i < _board.CellCount; i++)
            {
                _board.CoordAt(i, out var x, out var y);
                _cells[i] = CreateCell(x, y);
            }

            Measure();
            ApplyLayout();

            _spawner.SpawnMany(_board, _startingTiles);
            RefreshTiles();
        }

        private System.Random NewRandom()
        {
            return _seed == 0 ? new System.Random() : new System.Random(_seed);
        }


        public bool SpawnTile()
        {
            if (_board == null || !_spawner.TrySpawn(_board, out var index, out var value))
            {
                return false;
            }

            ShowTile(index, value);
            return true;
        }

        public void RefreshTiles()
        {
            for (var i = 0; i < _board.CellCount; i++)
            {
                ShowTile(i, _board.CellAt(i));
            }
        }

        private void ShowTile(int index, int value)
        {
            if (value == 0)
            {
                if (_tiles[index] == null) return;

                DestroyObject(_tiles[index].gameObject);
                _tiles[index] = null;
                return;
            }

            if (_tiles[index] == null)
            {
                _board.CoordAt(index, out var x, out var y);
                _tiles[index] = TileView.Create(_tileRoot, $"Tile {x},{y}");
            }

            _tiles[index].SetValue(value, ColorForValue(value), TextColorForValue(value));
            _tiles[index].SetGeometry(PositionFor(index), Vector2.one * _cellSize);
        }

        private Color ColorForValue(int value)
        {
            if (_tileColors == null || _tileColors.Length == 0) return Color.white;

            var rank = Mathf.FloorToInt(Mathf.Log(Mathf.Max(2, value), 2f) + 0.5f) - 1;
            return _tileColors[Mathf.Clamp(rank, 0, _tileColors.Length - 1)];
        }

        private Color TextColorForValue(int value)
        {
            return value >= _lightTextFrom ? _lightText : _darkText;
        }

        private RectTransform CreateLayer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private RectTransform CreateCell(int x, int y)
        {
            var go = new GameObject($"Cell {x},{y}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_cellRoot, false);

            var image = go.GetComponent<Image>();
            image.color = _cellColor;
            image.raycastTarget = false;

            var rect = (RectTransform)go.transform;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            return rect;
        }


        private void Measure()
        {
            var size = Rect.rect.size;

            var availableW = size.x - (_spacing * (_board.Width + 1));
            var availableH = size.y - (_spacing * (_board.Height + 1));

            _cellSize = Mathf.Max(1f, Mathf.Min(availableW / _board.Width, availableH / _board.Height));

            var gridW = (_cellSize * _board.Width) + (_spacing * (_board.Width + 1));
            var gridH = (_cellSize * _board.Height) + (_spacing * (_board.Height + 1));

            _origin = new Vector2((size.x - gridW) * 0.5f, (size.y - gridH) * 0.5f);
        }

        private void ApplyLayout()
        {
            var size = Vector2.one * _cellSize;

            for (var i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != null)
                {
                    _cells[i].sizeDelta = size;
                    _cells[i].anchoredPosition = PositionFor(i);
                }

                if (_tiles != null && _tiles[i] != null)
                {
                    _tiles[i].SetGeometry(PositionFor(i), size);
                }
            }
        }

        public Vector2 PositionFor(int index)
        {
            _board.CoordAt(index, out var x, out var y);
            return PositionFor(x, y);
        }

        public Vector2 PositionFor(int x, int y)
        {
            var pitch = _cellSize + _spacing;

            return new Vector2(
                _origin.x + _spacing + (x * pitch) + (_cellSize * 0.5f),
                _origin.y + _spacing + (y * pitch) + (_cellSize * 0.5f));
        }

        private void ClearChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyObject(transform.GetChild(i).gameObject);
            }

            _cells = null;
            _tiles = null;
        }

        private static void DestroyObject(GameObject go)
        {
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }
}