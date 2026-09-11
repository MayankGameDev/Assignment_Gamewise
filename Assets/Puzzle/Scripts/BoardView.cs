using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private float _spacing = 12f;
        [SerializeField] private Color _cellColor = new Color(0.80f, 0.76f, 0.71f);

        [SerializeField] private float _moveDuration = 0.12f;
        [SerializeField] private float _popDuration = 0.12f;

        [SerializeField] private Color _wallColor = new Color(0.35f, 0.33f, 0.31f);

        private Coroutine _animRoutine;
        public bool IsAnimating => _animRoutine != null;

        [Tooltip("Indexed by rank, so element 0 is the 2 tile. Bigger values reuse the last one.")] [SerializeField]
        private Color[] _tileColors =
        {
            new Color(0.965f, 0.906f, 0.847f), // 2    
            new Color(0.953f, 0.784f, 0.604f), // 4    
            new Color(0.949f, 0.573f, 0.420f), // 8    
            new Color(0.914f, 0.416f, 0.302f), // 16  
            new Color(0.788f, 0.310f, 0.357f), // 32   
            new Color(0.612f, 0.275f, 0.439f), // 64  
            new Color(0.424f, 0.282f, 0.525f), // 128  
            new Color(0.294f, 0.306f, 0.620f), // 256  
            new Color(0.196f, 0.400f, 0.690f), // 512  
            new Color(0.122f, 0.541f, 0.549f), // 1024 
            new Color(0.949f, 0.718f, 0.020f)  // 2048
        };

        [SerializeField] private Color _darkText = new Color(0.36f, 0.33f, 0.30f);
        [SerializeField] private Color _lightText = new Color(0.98f, 0.96f, 0.94f);

        [SerializeField] private int _lightTextFrom = 8;

        private Board _board;
        private RectTransform _rect;
        private RectTransform _cellRoot;
        private RectTransform _tileRoot;

        private RectTransform[] _cells;
        private TileView[] _tiles;

        private float _cellSize;
        private Vector2 _origin;

        private RectTransform Rect
        {
            get
            {
                if (_rect == null) _rect = (RectTransform)transform;
                return _rect;
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (_cells == null) return;

            Measure();
            ApplyLayout();
        }

        public void Initialize(Board board)
        {
            _board = board;

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
            Render();
        }

        public void Render()
        {
            if (_board == null) return;

            for (var i = 0; i < _board.CellCount; i++)
            {
                ShowTile(i, _board.CellAt(i));
            }
        }

        public void AnimateMoves(List<TileMove> moves, Action onComplete)
        {
            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(AnimateMovesRoutine(moves, onComplete));
        }

        public void AnimateShuffle(Action onComplete)
        {
            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(AnimateShuffleRoutine(onComplete));
        }

        private IEnumerator AnimateShuffleRoutine(Action onComplete)
        {
            
            for (var i = 0; i < _tiles.Length; i++)
            {
                var tile = _tiles[i];
                if (tile == null) continue;
                if (_board.CellAt(i) == Board.Wall) continue; 

                _tiles[i] = null;
                StartCoroutine(ScaleOutAndDestroy(tile, _popDuration));
            }

            yield return new WaitForSeconds(_popDuration);

            for (var i = 0; i < _board.CellCount; i++)
            {
                if (_board.CellAt(i) > 0) ShowNewTile(i);
            }

            _animRoutine = null;
            onComplete?.Invoke();
        }

        private static IEnumerator ScaleOutAndDestroy(TileView tile, float duration)
        {
            var rect = tile.Rect;
            var t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                rect.localScale = Vector3.one * (1f - Mathf.Clamp01(t / duration));
                yield return null;
            }

            DestroyObject(tile.gameObject);
        }

        private IEnumerator AnimateMovesRoutine(List<TileMove> moves, Action onComplete)
        {
            var newTiles = new TileView[_board.CellCount];
            Array.Copy(_tiles, newTiles, _tiles.Length);

            foreach (var move in moves)
            {
                var fromIndex = _board.Index(move.FromX, move.FromY);
                var toIndex = _board.Index(move.ToX, move.ToY);

                var tile = _tiles[fromIndex];
                if (tile == null) continue;

                newTiles[fromIndex] = null;
                var targetPos = PositionFor(toIndex);

                if (move.Merged)
                {
                    StartCoroutine(SlideTile(tile, targetPos, _moveDuration, () =>
                    {
                        DestroyObject(tile.gameObject);
                    }));
                }
                else
                {
                    newTiles[toIndex] = tile;
                    var finalValue = move.ResultValue;
                    var startValue = move.Value;

                    StartCoroutine(SlideTile(tile, targetPos, _moveDuration, () =>
                    {
                        if (finalValue != startValue)
                        {
                            tile.SetValue(finalValue, ColorForValue(finalValue), TextColorForValue(finalValue));
                            StartCoroutine(PopTile(tile, _popDuration));
                        }
                    }));
                }
            }

            yield return new WaitForSeconds(_moveDuration);

            _tiles = newTiles;
            _animRoutine = null;
            onComplete?.Invoke();
        }

        private static IEnumerator SlideTile(TileView tile, Vector2 targetPos, float duration, Action onArrive)
        {
            var rect = tile.Rect;
            var start = rect.anchoredPosition;
            var t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                var k = Mathf.Clamp01(t / duration);
                k = 1f - Mathf.Pow(1f - k, 3f); // ease-out cubic
                rect.anchoredPosition = Vector2.LerpUnclamped(start, targetPos, k);
                yield return null;
            }

            rect.anchoredPosition = targetPos;
            onArrive?.Invoke();
        }

        private static IEnumerator PopTile(TileView tile, float duration)
        {
            var rect = tile.Rect;
            var t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                var k = Mathf.Clamp01(t / duration);
                var scale = 1f + (0.25f * Mathf.Sin(k * Mathf.PI));
                rect.localScale = Vector3.one * scale;
                yield return null;
            }

            rect.localScale = Vector3.one;
        }

        public void ShowNewTile(int index)
        {
            if (index < 0 || _board == null) return;

            var value = _board.CellAt(index);
            if (value <= 0) return; 

            _board.CoordAt(index, out var x, out var y);
            var tile = TileView.Create(_tileRoot, $"Tile {x},{y}");
            tile.SetValue(value, ColorForValue(value), TextColorForValue(value));
            tile.SetGeometry(PositionFor(index), Vector2.one * _cellSize);
            tile.Rect.localScale = Vector3.zero;

            _tiles[index] = tile;
            StartCoroutine(ScaleIn(tile, _popDuration));
        }

        private static IEnumerator ScaleIn(TileView tile, float duration)
        {
            var rect = tile.Rect;
            var t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                rect.localScale = Vector3.one * Mathf.Clamp01(t / duration);
                yield return null;
            }

            rect.localScale = Vector3.one;
        }

        private void ShowTile(int index, int value)
        {
            if (value == Board.Wall)
            {
                if (_tiles[index] == null)
                {
                    _board.CoordAt(index, out var x, out var y);
                    _tiles[index] = TileView.CreateWall(_tileRoot, $"Wall {x},{y}", _wallColor);
                }

                _tiles[index].SetGeometry(PositionFor(index), Vector2.one * _cellSize);
                return;
            }

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