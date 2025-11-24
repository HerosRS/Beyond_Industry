using System.Numerics;
using BeyondIndustry.Data;
using BeyondIndustry.Utils;
using BeyondIndustry.Debug;
using Raylib_cs;

namespace BeyondIndustry.DebugView
{
    public class UI
    {
        public static void DebugDataUI()
        {
            //Funktionen für das, was über das Debug Menü angezeigt werden soll.
            int yPos = 10;
            
            if (DebugConsole.ShowFPS)
            {
                Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, yPos, 20, GlobalColor.DEBUG_GREEN_COLOR);
                yPos += 30;
            }
            
            if (DebugConsole.ShowMousePos)
            {
                Vector2 mouseGridPos = Grid.getMousePositionInGrid();
                Raylib.DrawText($"Grid: ({(int)mouseGridPos.X}, {(int)mouseGridPos.Y})", 10, yPos, 20, GlobalColor.DEBUG_GREEN_COLOR);
                yPos += 30;
            }

            if (!DebugConsole.ShowGrid)
            {
                for (int x = 0; x < GlobalData.SCREEN_WIDTH; x += GlobalData.CELL_SIZE)
                {
                    Raylib.DrawLine(x, 0, x, GlobalData.SCREEN_HEIGHT, GlobalColor.FORGROUND_COLOR);
                }

                for (int y = 0; y < GlobalData.SCREEN_HEIGHT; y += GlobalData.CELL_SIZE)
                {
                    Raylib.DrawLine(0, y, GlobalData.SCREEN_WIDTH, y, GlobalColor.FORGROUND_COLOR);
                }
               return; 
            } 
            
            
            
            Raylib.DrawText("[F1] Debug Console", 10, GlobalData.SCREEN_HEIGHT - 25, 14, Color.DarkGray);
        }
    }
}