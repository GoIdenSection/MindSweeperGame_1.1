using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MindSweeperGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Grid grid;
        SpriteFont font;

        Texture2D textureClosed;
        Texture2D textureOpen;
        Texture2D textureMine;
        Texture2D textureFlag;
        Texture2D[] numberTextures;
        Texture2D topBarTexture;

        int cellSize = 32;
        int totalSeconds = 0;
        int moveCount = 0;
        int topBarHeight = 40;
        double elapsedTime = 0;
        bool isGameOver = false;
        bool isChoosingDifficulty = true;
        private bool isGameStarted = false;

        MouseState previousMouse;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 800;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }

        //laddar spelets resurser tex mina bilder och min font
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("bFont");

            //skapar en enfärgad textur som används som topprad
            topBarTexture = new Texture2D(GraphicsDevice, 1, 1);
            topBarTexture.SetData(new[] { Color.Black });

            //textur för spelbrikor
            textureClosed = Content.Load<Texture2D>("stängd");
            textureOpen = Content.Load<Texture2D>("open");
            textureMine = Content.Load<Texture2D>("minorna");
            textureFlag = Content.Load<Texture2D>("minaFunnen");

            //laddar in texturer för siffror 0-8
            numberTextures = new Texture2D[9];
            for (int i = 0; i <= 8; i++)
            {
                numberTextures[i] = Content.Load<Texture2D>($"nr_{i}");
            }
        }

        //uppdaterat spelets logik varige frame
        protected override void Update(GameTime gameTime)
        {
            if (isChoosingDifficulty)
            {
                //startar nytt spel boreonde på vald svårightsgrad
                if (Keyboard.GetState().IsKeyDown(Keys.D1))
                    StartNewGame(10, 10, 10);
                else if (Keyboard.GetState().IsKeyDown(Keys.D2))
                    StartNewGame(15, 15, 40);
                else if (Keyboard.GetState().IsKeyDown(Keys.D3))
                    StartNewGame(20, 20, 80);
            }
            else if (!isGameOver)
            {
                MouseState mouse = Mouse.GetState();

                //vensker klik hantering
                if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                {
                    bool moveWasMade = grid.HandleLeftClick(
                        mouse.X - grid.OffsetX,
                        mouse.Y - grid.OffsetY,
                        moveCount
                    );

                    if (moveWasMade)
                    {
                        moveCount++;
                        if (!isGameStarted)
                        {
                            isGameStarted = true;
                        }
                    }
                }

                // högerklock handering
                if (mouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Released)
                {
                    grid.HandleRightClick(mouse.X - grid.OffsetX, mouse.Y - grid.OffsetY);
                }

                if (grid.GameOver)
                    isGameOver = true;

                previousMouse = mouse;
            }

            // om spelaren trycker på Q så startar ett nytt spel
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                isChoosingDifficulty = true;
                isGameOver = false;
                isGameStarted = false;
            }

            //uppdaterar tiden
            if (!isChoosingDifficulty && !isGameOver && isGameStarted)
            {
                elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

                if (elapsedTime >= 1)
                {
                    totalSeconds += 1;
                    elapsedTime -= 1;
                }
            }

            base.Update(gameTime);
        }

        //ritar allt grafiskt på skärmen
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // Ritatar toppraden med tid och antal drag
            spriteBatch.Draw(topBarTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, topBarHeight), Color.Black);
            string scoreboard = $"Time: {totalSeconds}   Moves: {moveCount}";

            Vector2 textSize = font.MeasureString(scoreboard);
            Vector2 centeredPos = new Vector2(
                (graphics.PreferredBackBufferWidth - textSize.X) / 2,
                (topBarHeight - textSize.Y) / 2 + 2 // Justera vertikalt
            );

            spriteBatch.DrawString(font, scoreboard, centeredPos, Color.White);


            if (isChoosingDifficulty)
            {
                // visar menyn för att välja svårighetsgrad
                string prompt = "Choose difficulty level:";
                Vector2 promptSize = font.MeasureString(prompt);
                float centeredX = (graphics.PreferredBackBufferWidth - promptSize.X) / 2;

                spriteBatch.DrawString(font, prompt, new Vector2(centeredX, 100), Color.White);
                spriteBatch.DrawString(font, "1. Small map (10x10)", new Vector2(centeredX, 140), Color.White);
                spriteBatch.DrawString(font, "2. Medium map (15x15)", new Vector2(centeredX, 180), Color.White);
                spriteBatch.DrawString(font, "3. Large map (20x20)", new Vector2(centeredX, 220), Color.White);
            }
            else
            {
                // Rita själva banan
                grid.Draw(spriteBatch, textureClosed, textureOpen, textureFlag, textureMine, numberTextures);

                //visar medelande om spelvinsr eller förlust
                if (isGameOver)
                {
                    string message = grid.CheckWin() ? "You won! Press Q to restart" : "Game Over! Press Q to restart";
                    Color textColor = grid.CheckWin() ? Color.GreenYellow : Color.Red;
                    spriteBatch.DrawString(font, message, new Vector2(50, topBarHeight + 5), textColor);
                }
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }

        //startar nytt spel med angivna parameterar
        private void StartNewGame(int width, int height, int mines)
        {
            moveCount = 0;
            totalSeconds = 0;
            elapsedTime = 0;
            isGameStarted = false;

            grid = new Grid(width, height, cellSize, mines);
            isChoosingDifficulty = false;
            isGameOver = false;

            //Beräknar var på skärmen spelplanen ska ritas
            grid.CalculateOffset(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, topBarHeight);
        }
    }
}
