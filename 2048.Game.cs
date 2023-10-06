using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByDenisRafi
{
    class Program
    {
        static void Main()
        {
            Console.Title = "2048 Game. Keys - Up, Down, Right, Left.";
            DR game = new DR();
            game.Run();
        }
    }
    class DR
    {
        public ulong Score { get; private set; }
        public ulong[,] Board { get; private set; }
        private readonly int nRows;
        private readonly int nCols;
        private readonly Random random = new Random();
        public DR()
        {
            this.Board = new ulong[4, 4];
            this.nRows = this.Board.GetLength(0);
            this.nCols = this.Board.GetLength(1);
            this.Score = 0;
        }
        public void Run()
        {
            bool hasUpdated = true;
            do
            {
                if (hasUpdated)
                {
                    PutNewValue();
                }
                Display();
                if (IsDead())
                {
                    using (new ColorOutput(ConsoleColor.Red))
                    {
                        Console.WriteLine("You Lose!");
                        break;
                    }
                }
                ConsoleKeyInfo input = Console.ReadKey(true); 
                Console.WriteLine(input.Key.ToString());
                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        hasUpdated = Update(Direction.Up);
                        break;
                    case ConsoleKey.DownArrow:
                        hasUpdated = Update(Direction.Down);
                        break;
                    case ConsoleKey.LeftArrow:
                        hasUpdated = Update(Direction.Left);
                        break;
                    case ConsoleKey.RightArrow:
                        hasUpdated = Update(Direction.Right);
                        break;
                    default:
                        hasUpdated = false;
                        break;
                }
            }
            while (true); 
            Console.Read();
        }
        private static ConsoleColor GetNumberColor(ulong num)
        {
            switch (num)
            {
                case 0:
                    return ConsoleColor.DarkGray;
                case 2:
                    return ConsoleColor.Cyan;
                case 4:
                    return ConsoleColor.Magenta;
                case 8:
                    return ConsoleColor.Red;
                case 16:
                    return ConsoleColor.Green;
                case 32:
                    return ConsoleColor.Yellow;
                case 64:
                    return ConsoleColor.Yellow;
                case 128:
                    return ConsoleColor.DarkCyan;
                case 256:
                    return ConsoleColor.Cyan;
                case 512:
                    return ConsoleColor.DarkMagenta;
                case 1024:
                    return ConsoleColor.Magenta;
                default:
                    return ConsoleColor.Red;
            }
        }
        private static bool Update(ulong[,] board, Direction direction, out ulong score)
        {
            int nRows = board.GetLength(0);
            int nCols = board.GetLength(1);

            score = 0;
            bool hasUpdated = false;
            bool isAlongRow = direction == Direction.Left || direction == Direction.Right;
            bool isIncreasing = direction == Direction.Left || direction == Direction.Up;
            int outterCount = isAlongRow ? nRows : nCols;
            int innerCount = isAlongRow ? nCols : nRows;
            int innerStart = isIncreasing ? 0 : innerCount - 1;
            int innerEnd = isIncreasing ? innerCount - 1 : 0;
            Func<int, int> drop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex - 1)
                : new Func<int, int>(innerIndex => innerIndex + 1);
            Func<int, int> reverseDrop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex + 1)
                : new Func<int, int>(innerIndex => innerIndex - 1);
            Func<ulong[,], int, int, ulong> getValue = isAlongRow
                ? new Func<ulong[,], int, int, ulong>((x, i, j) => x[i, j])
                : new Func<ulong[,], int, int, ulong>((x, i, j) => x[j, i]);
            Action<ulong[,], int, int, ulong> setValue = isAlongRow
                ? new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[i, j] = v)
                : new Action<ulong[,], int, int, ulong>((x, i, j, v) => x[j, i] = v);
            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd)
            <= index && index <= Math.Max(innerStart, innerEnd);
            for (int i = 0; i < outterCount; i++)
            {
                for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                {
                    if (getValue(board, i, j) == 0)
                    {
                        continue;
                    }
                    int newJ = j;
                    do
                    {
                        newJ = drop(newJ);
                    }
                    while (innerCondition(newJ) && getValue(board, i, newJ) == 0);
                    if (innerCondition(newJ) && getValue(board, i, newJ) == getValue(board, i, j))
                    {
                        ulong newValue = getValue(board, i, newJ) * 2;
                        setValue(board, i, newJ, newValue);
                        setValue(board, i, j, 0);
                        hasUpdated = true;
                        score += newValue;
                    }
                    else
                    {
                        newJ = reverseDrop(newJ); 
                        if (newJ != j)
                        {
                            hasUpdated = true;
                        }
                        ulong value = getValue(board, i, j);
                        setValue(board, i, j, 0);
                        setValue(board, i, newJ, value);
                    }
                }
            }
            return hasUpdated;
        }
        private bool Update(Direction dir)
        {
            ulong score;
            bool isUpdated = DR.Update(this.Board, dir, out score);
            this.Score += score;
            return isUpdated;
        }
        private bool IsDead()
        {
            ulong score;
            foreach (Direction dir in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                ulong[,] clone = (ulong[,])Board.Clone();
                if (DR.Update(clone, dir, out score))
                {
                    return false;
                }
            }
            return true;
        }
        private void Display()
        {
            Console.Clear();
            Console.WriteLine();
            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    using (new ColorOutput(DR.GetNumberColor(Board[i, j])))
                    {
                        Console.Write(string.Format("{0,6}", Board[i, j]));
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            Console.WriteLine("Score: {0}", this.Score);
            Console.WriteLine();
        }
        private void PutNewValue()
        {
            List<Tuple<int, int>> emptySlots = new List<Tuple<int, int>>();
            for (int iRow = 0; iRow < nRows; iRow++)
            {
                for (int iCol = 0; iCol < nCols; iCol++)
                {
                    if (Board[iRow, iCol] == 0)
                    {
                        emptySlots.Add(new Tuple<int, int>(iRow, iCol));
                    }
                }
            }
            int iSlot = random.Next(0, emptySlots.Count); 
            ulong value = random.Next(0, 100) < 95 ? (ulong)2 : (ulong)4;
            Board[emptySlots[iSlot].Item1, emptySlots[iSlot].Item2] = value;
        }
        #region 
        enum Direction
        {
            Up,
            Down,
            Right,
            Left,
        }
        class ColorOutput : IDisposable
        {
            public ColorOutput(ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
            {
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
            }
            public void Dispose()
            {
                Console.ResetColor();
            }
        }
        #endregion 
    }
}