namespace Puzzle
{
    public struct TileMove
    {
        public int FromX;
        public int FromY;
        public int ToX;
        public int ToY;
 
        public int Value;
        public int ResultValue;
        public bool Merged;
    }
}