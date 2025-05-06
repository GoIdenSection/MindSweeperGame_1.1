using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MindSweeperGame
{
    public class Grid
    {
        public Cell[,] Cells;
        public int Width { get; }
        public int Height { get; }
        public int CellSize { get; }
        public int MineCount { get; }
        public bool GameOver { get; set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        private readonly Random random = new Random();

        public Grid(int width, int height, int cellSize, int mines)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            MineCount = mines;
            Cells = new Cell[Width, Height];
            InitializeCells(); // Skapar tomma celler utan minor
        }
        public void CalculateOffset(int screenWidth, int screenHeight, int topBarHeight)
        {
            int gridWidthPx = Width * CellSize;
            int gridHeightPx = Height * CellSize;

            // Beräkna offset för centrering
            OffsetX = Math.Max(0, (screenWidth - gridWidthPx) / 2);
            OffsetY = Math.Max(topBarHeight, (screenHeight - gridHeightPx) / 2) + topBarHeight / 2;
        }

        private void InitializeCells()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Cells[x, y] = new Cell(x, y, CellSize);
        }

        public void GenerateMinesAfterFirstClick(int avoidX, int avoidY)
        {
            int placed = 0;
            while (placed < MineCount)
            {
                int x = random.Next(Width);
                int y = random.Next(Height);

                // Undvik 3x3 området runt första klicket
                if (Math.Abs(x - avoidX) <= 1 && Math.Abs(y - avoidY) <= 1)
                    continue;

                if (!Cells[x, y].HasMine)
                {
                    Cells[x, y].HasMine = true;
                    placed++;
                }
            }
            CalculateAdjacentMines();
        }

        private void CalculateAdjacentMines()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y].HasMine) continue;

                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (nx >= 0 && ny >= 0 && nx < Width && ny < Height)
                                if (Cells[nx, ny].HasMine)
                                    count++;
                        }
                    }
                    Cells[x, y].AdjacentMines = count;
                }
            }
        }

        public bool HandleLeftClick(int mouseX, int mouseY, int currentMoveCount)
        {
            int x = mouseX / CellSize;
            int y = mouseY / CellSize;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;

            var cell = Cells[x, y];

            if (cell.IsFlagged || cell.IsRevealed)
                return false;

            // Första klicket: Generera minor och floodfill
            if (currentMoveCount == 0)
            {
                GenerateMinesAfterFirstClick(x, y);
                cell = Cells[x, y]; // Uppdatera referens efter mingenerering
            }

            if (cell.HasMine)
            {
                cell.IsRevealed = true;
                GameOver = true;
                RevealAllMines();
                return true;
            }

            if (cell.AdjacentMines == 0)
                FloodFill(x, y);
            else
                cell.IsRevealed = true;

            if (CheckWin())
                GameOver = true;

            return true;
        }
        public void HandleRightClick(int mouseX, int mouseY)
        {
            int x = mouseX / CellSize;
            int y = mouseY / CellSize;

            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            var cell = Cells[x, y];

            if (!cell.IsRevealed)
                cell.IsFlagged = !cell.IsFlagged;
        }

        private void RevealAllMines()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Cells[x, y].HasMine)
                    {
                        Cells[x, y].IsRevealed = true;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D closed, Texture2D open, Texture2D flag, Texture2D mine, Texture2D[] numbers)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Rectangle drawRect = new Rectangle(
                        Cells[x, y].Bounds.X + OffsetX,
                        Cells[x, y].Bounds.Y + OffsetY,
                        Cells[x, y].Bounds.Width,
                        Cells[x, y].Bounds.Height
                    );
                    Cells[x, y].Draw(spriteBatch, closed, open, flag, mine, numbers, drawRect);
                }
            }
        }

        private void FloodFill(int startX, int startY)
        {
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(startX, startY));

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                int x = p.X;
                int y = p.Y;

                // Kolla gränser
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    continue;

                Cell cell = Cells[x, y];

                // Avbryt om cellen redan är avslöjad, är mina eller flaggad
                if (cell.IsRevealed || cell.HasMine || cell.IsFlagged)
                    continue;

                cell.IsRevealed = true;

                // Fortsätt bara floodfilla om det inte finns närliggande minor
                if (cell.AdjacentMines == 0)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            queue.Enqueue(new Point(x + dx, y + dy));
                        }
                    }
                }
            }
        }
        public bool CheckWin()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var cell = Cells[x, y];

                    // En cell ska räknas som "klar" om:
                    // 1. Det är en mina OCH är flaggad, ELLER
                    // 2. Det inte är en mina OCH är avslöjad
                    if (!cell.HasMine && !cell.IsRevealed)
                        return false;
                }
            }
            return true;
        }
    }
}
