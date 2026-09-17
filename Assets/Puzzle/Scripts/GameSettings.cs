using UnityEngine;

namespace Puzzle
{
    [System.Serializable]
    public struct GameSettings
    {
        public const int MinSize = 2;
        public const int MaxSize = 8;
        public const int MinWinValue = 8;
        public const int MaxWinValue = 65536;

        public int Width;
        public int Height;
        public int WinValue;

        public GameSettings(int width, int height, int winValue)
        {
            Width = width;
            Height = height;
            WinValue = winValue;
        }
        
        public GameSettings Sanitized()
        {
            return new GameSettings(
                Mathf.Clamp(Width, MinSize, MaxSize),
                Mathf.Clamp(Height, MinSize, MaxSize),
                Mathf.Clamp(Mathf.ClosestPowerOfTwo(Mathf.Max(1, WinValue)), MinWinValue, MaxWinValue));
        }
    }
}
