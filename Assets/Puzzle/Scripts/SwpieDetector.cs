using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzle
{
    public class SwipeDetector : MonoBehaviour
    {
        
        [Range(0.01f, 0.25f)]
        [SerializeField]
        private float _threshold = 0.05f;

        [Tooltip("Arrow keys and WASD, handy for testing in the editor.")] [SerializeField]
        private bool _keyboardEnabled = true;

        private Pointer _activePointer;
        private bool _dragging;
        private bool _sent;
        private Vector2 _startPos;

        public event Action<Direction> Swiped;

        private float ThresholdPixels => Mathf.Min(Screen.width, Screen.height) * _threshold;

        private void Update()
        {
            if (_keyboardEnabled) PollKeys();
            PollPointer();
        }

        private void PollKeys()
        {
            var keys = Keyboard.current;
            if (keys == null) return;

            if (keys.upArrowKey.wasPressedThisFrame || keys.wKey.wasPressedThisFrame)
            {
                Swiped?.Invoke(Direction.Up);
            }
            else if (keys.downArrowKey.wasPressedThisFrame || keys.sKey.wasPressedThisFrame)
            {
                Swiped?.Invoke(Direction.Down);
            }
            else if (keys.leftArrowKey.wasPressedThisFrame || keys.aKey.wasPressedThisFrame)
            {
                Swiped?.Invoke(Direction.Left);
            }
            else if (keys.rightArrowKey.wasPressedThisFrame || keys.dKey.wasPressedThisFrame)
            {
                Swiped?.Invoke(Direction.Right);
            }
        }

        private void PollPointer()
        {
            var pointer = ResolvePointer();
            if (pointer == null) return;

            var pos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                _activePointer = pointer;
                _dragging = true;
                _sent = false;
                _startPos = pos;
                return;
            }

            if (_dragging && pointer.press.isPressed)
            {
                if (_sent) return;

                var delta = pos - _startPos;
                if (delta.magnitude < ThresholdPixels) return;

                _sent = true;
                Swiped?.Invoke(ToDirection(delta));
                return;
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                _dragging = false;
                _activePointer = null;
            }
        }
        
        private Pointer ResolvePointer()
        {
            if (_dragging && _activePointer != null && _activePointer.added)
            {
                return _activePointer;
            }

            var touch = Touchscreen.current;
            if (touch != null && touch.added) return touch;

            var mouse = Mouse.current;
            if (mouse != null && mouse.added) return mouse;

            return Pointer.current;
        }

        private static Direction ToDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                return delta.x > 0f ? Direction.Right : Direction.Left;
            }

            return delta.y > 0f ? Direction.Up : Direction.Down;
        }
    }
}