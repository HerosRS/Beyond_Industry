using System.Numerics;
using BeyondIndustry.Data;
using BeyondIndustry.Utils;
using Raylib_cs;

namespace BeyondIndustry.DebugView
{
    public class UI
    {
        public static void InfoUI()
        {
            Raylib.DrawText(Raylib.GetFPS().ToString(), 10, 0, 20, Color.White);
            Raylib.DrawText(Grid.getMousePositionInGrid().ToString(), 10, 30, 20, Color.White);

        }
        public static void ViewGrid()
        {
            for (int x = 0; x < GlobalData.SCREEN_WIDTH; x += GlobalData.CELL_SIZE)
            {
                Raylib.DrawLine(x, 0, x, GlobalData.SCREEN_HEIGHT, GlobalColor.FORGROUND_COLOR);
            }

            for (int y = 0; y < GlobalData.SCREEN_HEIGHT; y += GlobalData.CELL_SIZE)
            {
                Raylib.DrawLine(0, y, GlobalData.SCREEN_WIDTH, y, GlobalColor.FORGROUND_COLOR);
            }
        }
    } 
}