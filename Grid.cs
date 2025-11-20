using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Utils
{
    public class Grid
    {
        private int cellSize;
        private Color lineColor;

        public Grid(int cellSize, Color lineColor)
        {
            this.cellSize = cellSize;
            this.lineColor = lineColor;
        }

        public void Draw(int screenWidth, int screenHeight)
        {
            for (int x = 0; x < screenWidth; x += cellSize)
            {
                Raylib.DrawLine(x, 0, x, screenHeight, lineColor);
            }

            for (int y = 0; y < screenHeight; y += cellSize)
            {
                Raylib.DrawLine(0, y, screenWidth, y, lineColor);
            }
        }

        public Vector2 getMousePositionInGrid()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            int gridX = (int)(mousePos.X / cellSize);
            int gridY = (int)(mousePos.Y / cellSize);
            return new Vector2(gridX, gridY);
        }

        
    }
}