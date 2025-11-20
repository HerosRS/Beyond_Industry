using System.Numerics;
using BeyondIndustry.Data;
using Raylib_cs;

namespace BeyondIndustry.Utils
{
    public class Grid
    {
        public static Vector2 getMousePositionInGrid()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            int gridX = (int)(mousePos.X / GlobalData.CELL_SIZE);
            int gridY = (int)(mousePos.Y / GlobalData.CELL_SIZE);
            return new Vector2(gridX, gridY);
        }    
    }
}