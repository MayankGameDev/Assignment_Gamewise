using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    public class GameSetupPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_InputField _widthInput;
        [SerializeField] private TMP_InputField _heightInput;
        [SerializeField] private TMP_InputField _winValueInput;
        [SerializeField] private TextMeshProUGUI _hintLabel;
        [SerializeField] private Button _startButton;

        public event Action<GameSettings> StartPressed;
        
        private GameSettings _current;

        public bool IsVisible => _root != null && _root.activeSelf;

        private string DefaultHint =>
            $"Board sides {GameSettings.MinSize}-{GameSettings.MaxSize}. " +
            $"Win value must be a power of 2 ({GameSettings.MinWinValue:N0}-{GameSettings.MaxWinValue:N0}).";

        private void Awake()
        {
            if (_startButton != null) _startButton.onClick.AddListener(OnStartClicked);

            if (_widthInput != null) _widthInput.onEndEdit.AddListener(_ => Normalize());
            if (_heightInput != null) _heightInput.onEndEdit.AddListener(_ => Normalize());
            if (_winValueInput != null) _winValueInput.onEndEdit.AddListener(_ => Normalize());

            Hide();
        }

        public void Show(GameSettings current)
        {
            _current = current.Sanitized();
            Write(_current);
            SetHint(DefaultHint);

            if (_root != null) _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        private void OnStartClicked()
        {
            var settings = Normalize();
            Hide();
            StartPressed?.Invoke(settings);
        }
        
        private GameSettings Normalize()
        {
            var typed = new GameSettings(
                ParseOr(_widthInput, _current.Width),
                ParseOr(_heightInput, _current.Height),
                ParseOr(_winValueInput, _current.WinValue));

            var settings = typed.Sanitized();
            _current = settings;
            Write(settings);

            if (settings.WinValue != typed.WinValue)
            {
                SetHint($"Win value adjusted to {settings.WinValue:N0} (nearest power of 2).");
            }
            else if (settings.Width != typed.Width || settings.Height != typed.Height)
            {
                SetHint($"Board size adjusted to {settings.Width}x{settings.Height}.");
            }
            else
            {
                SetHint(DefaultHint);
            }

            return settings;
        }

        private void Write(GameSettings settings)
        {
            if (_widthInput != null) _widthInput.SetTextWithoutNotify(settings.Width.ToString());
            if (_heightInput != null) _heightInput.SetTextWithoutNotify(settings.Height.ToString());
            if (_winValueInput != null) _winValueInput.SetTextWithoutNotify(settings.WinValue.ToString());
        }

        private void SetHint(string text)
        {
            if (_hintLabel != null) _hintLabel.text = text;
        }

        private static int ParseOr(TMP_InputField field, int fallback)
        {
            if (field == null) return fallback;
            return int.TryParse(field.text, out var value) ? value : fallback;
        }
    }
}
