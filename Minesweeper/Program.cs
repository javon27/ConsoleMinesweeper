using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    class Program
    {
        static int Width = 10;
        static int Height = 20;
        static int NumBombs = 10;

        static void Main(string[] args)
        {
            Console.SetWindowSize(75, 35);
            Console.BufferWidth = 75;
            Console.Clear();
            Console.WriteLine("Welcome to Minesweeper!");
            Console.Write("Press any key to start...");
            Console.ReadKey();

            bool playAgain;

            do
            {
                playAgain = false;

                var board = new Board(Width, Height, NumBombs);
                board.Draw();
                do
                {
                    // get user input
                    UserCommand uc;
                    bool valid = GetUserInput(out uc);
                    if (valid)
                    {
                        if (uc.Reveal)
                        {
                            valid = board.RevealCell(uc.x - 1, uc.y - 1);
                        }
                        else
                        {
                            valid = board.ToggleFlag(uc.x - 1, uc.y - 1);
                        }
                    }
                    if (!valid)
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid command. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }

                    board.Draw();

                } while (!board.GameOver);

                if (board.IsWinCondition())
                {
                    Console.WriteLine("You WON!!!");
                }
                else
                {
                    Console.WriteLine("You LOST...");
                }

                Console.WriteLine("Would you like to play again? y or n:");
                var key = Console.ReadKey();
                if (key.KeyChar == 'y')
                    playAgain = true;
            } while (playAgain);

            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        static bool GetUserInput(out UserCommand uc)
        {
            Console.WriteLine("Enter move in this format: m x y");
            Console.WriteLine("where m is either r for reveal or f for flag");
            Console.WriteLine($"and x is column number (1 to {Width}) and y is row number (1 to {Height})");
            Console.WriteLine("Enter q to quit");
            var c = Console.ReadLine();

            if (c.Trim().ToLower() == "q")
            {
                Console.WriteLine("Bye!");
                System.Threading.Thread.Sleep(1000);
                System.Environment.Exit(0);
            }

            var validCommand = true;
            uc = new UserCommand();

            var cList = c.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int x = 0, y = 0;
            if (cList.Length != 3
                || !(new string[] { "f", "r" }).Contains(cList[0].ToLower())
                || !int.TryParse(cList[1], out x) || !int.TryParse(cList[2], out y)) {
                validCommand = false;
            }
            else
            {
                uc.Reveal = cList[0].ToLower() == "r";
                uc.x = x;
                uc.y = y;
            }

            return validCommand;
        }
    }

    public struct UserCommand
    {
        public int x, y;
        public bool Reveal;
    }

    public class Cell
    {
        public bool HasBomb;
        public bool HasFlag;
        public int BombsNearCount;
        public bool Revealed;
        
        public Cell()
        {
            HasBomb = false;
            BombsNearCount = 0;
            Revealed = false;
            HasFlag = false;
        }
    }

    public class Board
    {
        private readonly int width;
        private readonly int height;
        private readonly int numBombs;
        private int lastX, lastY;

        public bool GameOver { get; private set; }

        private readonly Cell[,] boardArray;

        public Board(int width = 10, int height = 20, int numBombs = 50)
        {
            this.width = width;
            this.height = height;
            this.numBombs = numBombs;
            lastX = -1;
            lastY = -1;

            boardArray = new Cell[width, height];
            GameOver = false;

            InitializeBoard();
            FillBoard();
        }

        private void InitializeBoard()
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    boardArray[x, y] = new Cell();
                }
            }
        }

        private void FillBoard()
        {
            var rand = new Random();
            for (var i=0; i<numBombs; i++)
            {
                int x, y;
                do
                {
                    x = rand.Next(0, width);
                    y = rand.Next(0, height);
                } while (boardArray[x, y].HasBomb);
                boardArray[x, y].HasBomb = true;

                for (var m = x - 1; m <= x + 1; m++)
                {
                    for (var n = y - 1; n <= y + 1; n++)
                    {
                        if (m >= 0 && m < width && n >= 0 && n < height)
                            boardArray[m, n].BombsNearCount++;
                    }
                }
            }
        }

        public bool ToggleFlag(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            var cell = boardArray[x, y];
            if (cell.Revealed)
                return false;

            cell.HasFlag = !cell.HasFlag;
            return true;
        }

        public bool RevealCell(int x, int y, bool user = true)
        {
            // Check if valid move
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            var cell = boardArray[x, y];
            if (cell.Revealed || cell.HasFlag)
                return false;

            if (user)
            {
                lastX = x;
                lastY = y;
            }

            cell.Revealed = true;
            if (cell.HasBomb || IsWinCondition())
            {
                GameOver = true;
            }

            if (cell.BombsNearCount == 0)
            {
                for (var m = x - 1; m <= x + 1; m++)
                {
                    for (var n = y - 1; n <= y + 1; n++)
                    {
                        if (m >= 0 && m < width && n >= 0 && n < height)
                            if (!boardArray[m, n].HasBomb)
                                RevealCell(m, n, false);
                    }
                }
            }
            return true;
        }

        public bool IsWinCondition()
        {
            bool win = true;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var cell = boardArray[x, y];
                    if (cell.Revealed && cell.HasBomb)
                    {
                        win = false;
                    }
                    else if (!cell.HasBomb && !cell.Revealed)
                    {
                        win = false;
                    }
                }
            }
            return win;
        }

        public void Draw()
        {
            // TODO
            Console.Clear();
            Console.Write("     ");
            for (var x = 0; x < width; x++)
            {
                Console.Write((x+1) + " ");
            }
            Console.WriteLine("\n   -----------------------");

            for (var y = 0; y < height ; y++)
            {
                var line = (y + 1).ToString().PadLeft(2,' ');
                Console.Write(line + " | ");
                for (var x = 0; x < width; x++)
                {
                    var cell = boardArray[x, y];
                    if (GameOver && cell.HasBomb)
                    {
                        if (lastX == x && lastY == y)
                        {
                            Console.Write("X");
                        }
                        else
                        {
                            Console.Write("B");
                        }
                    }
                    else if (cell.Revealed)
                    {
                        if (cell.HasBomb)
                        {
                            Console.Write("B");
                        }
                        else
                        {
                            Console.Write(cell.BombsNearCount > 0 ? cell.BombsNearCount.ToString() : "_");
                        }
                    }
                    else
                    {
                        if (cell.HasFlag)
                        {
                            Console.Write("F");
                        }
                        else
                        {
                            Console.Write("■");
                        }
                    }
                    Console.Write(" ");
                }
                Console.WriteLine("| " + line);
            }

            Console.WriteLine("   -----------------------");
            Console.Write("     ");
            for (var x = 0; x < width; x++)
            {
                Console.Write((x + 1) + " ");
            }
            Console.WriteLine();
        }


    }
}
