using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle
{
    
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _message;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _secondaryButton;
        [SerializeField] private TextMeshProUGUI _secondaryLabel;

        public event Action NewGamePressed;
        public event Action SecondaryPressed;

        public bool IsVisible => _root != null && _root.activeSelf;

        private void Awake()
        {
            if (_newGameButton != null) _newGameButton.onClick.AddListener(() => NewGamePressed?.Invoke());
            if (_secondaryButton != null) _secondaryButton.onClick.AddListener(() => SecondaryPressed?.Invoke());

            Hide();
        }
        
        public void Show(string title, string message, string secondaryLabel)
        {
            if (_title != null) _title.text = title;
            if (_message != null) _message.text = message;

            if (_secondaryButton != null)
            {
                _secondaryButton.gameObject.SetActive(!string.IsNullOrEmpty(secondaryLabel));
            }

            if (_secondaryLabel != null && !string.IsNullOrEmpty(secondaryLabel))
            {
                _secondaryLabel.text = secondaryLabel;
            }

            if (_root != null) _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }
    }
}
