using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    public class TileView : MonoBehaviour
    {
        private Image _background;
        private TextMeshProUGUI _label;
        private RectTransform _rect;

        public RectTransform Rect => _rect;

        public static TileView Create(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<TileView>();
            view._rect = (RectTransform)go.transform;
            view._background = go.GetComponent<Image>();
            view._background.raycastTarget = false;

            view._rect.anchorMin = Vector2.zero;
            view._rect.anchorMax = Vector2.zero;
            view._rect.pivot = new Vector2(0.5f, 0.5f);

            var labelGo = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(go.transform, false);

            view._label = labelGo.AddComponent<TextMeshProUGUI>();
            view._label.alignment = TextAlignmentOptions.Center;
            view._label.fontStyle = FontStyles.Bold;
            view._label.raycastTarget = false;

            
            view._label.enableAutoSizing = true;
            view._label.fontSizeMin = 8f;
            view._label.fontSizeMax = 96f;

            if (TMP_Settings.defaultFontAsset != null)
            {
                view._label.font = TMP_Settings.defaultFontAsset;
            }

            var labelRect = view._label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 6f);
            labelRect.offsetMax = new Vector2(-6f, -6f);

            return view;
        }
        
        
        public static TileView CreateWall(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var view = go.AddComponent<TileView>();
            view._rect = (RectTransform)go.transform;
            view._background = go.GetComponent<Image>();
            view._background.color = color;
            view._background.raycastTarget = false;

            view._rect.anchorMin = Vector2.zero;
            view._rect.anchorMax = Vector2.zero;
            view._rect.pivot = new Vector2(0.5f, 0.5f);

            return view;
        }

        public void SetValue(int value, Color background, Color textColor)
        {
            _background.color = background;

            if (_label != null)
            {
                _label.text = value.ToString();
                _label.color = textColor;
            }
        }

        public void SetGeometry(Vector2 anchoredPosition, Vector2 size)
        {
            _rect.sizeDelta = size;
            _rect.anchoredPosition = anchoredPosition;
        }

        public void SetSize(Vector2 size)
        {
            _rect.sizeDelta = size;
        }
    }
}