using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MindSweeperGame
{
    public class Cell
    {
        public Rectangle Bounds;
        public bool HasMine;
        public bool IsRevealed;
        public bool IsFlagged;
        public int AdjacentMines;

        public Cell(int gridX, int gridY, int size)
        {
            // Beräknar cellens position och storlek i spelet
            Bounds = new Rectangle(gridX * size, gridY * size, size, size);
            HasMine = false;
            IsRevealed = false;
            IsFlagged = false;
            AdjacentMines = 0;
        }

        // Ritar cellen baserat på dess nuvarande tillstån
        public void Draw(SpriteBatch spriteBatch, Texture2D closed, Texture2D open, Texture2D flag, Texture2D mine, Texture2D[] numbers, Rectangle drawRect)
        {
            if (IsRevealed)
            {
                if (HasMine)
                    spriteBatch.Draw(mine, drawRect, Color.White);
                else if (AdjacentMines > 0)
                    spriteBatch.Draw(numbers[AdjacentMines], drawRect, Color.White);
                else
                    spriteBatch.Draw(open, drawRect, Color.White);
            }
            else
            {
                if (IsFlagged)
                    spriteBatch.Draw(flag, drawRect, Color.White);
                else
                    spriteBatch.Draw(closed, drawRect, Color.White);
            }
        }
    }
}
