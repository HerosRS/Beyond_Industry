using Raylib_cs;
using BeyondIndustry.Utils;
using System.ComponentModel;
using BeyondIndustry.Data;
using BeyondIndustry.DebugView;

namespace BeyondIndustry
{
    class Program
    {
        static void Main()
        {
            // Fenster initialisieren
            Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");
            Raylib.SetTargetFPS(60);

            // Haupt-Game-Loop
            while (!Raylib.WindowShouldClose())
            {
                

                Raylib.BeginDrawing();
                Raylib.ClearBackground(GlobalColor.BACKGROUND_COLOR);
                UI.ViewGrid();
                UI.InfoUI();
                
                Raylib.EndDrawing();
            }

            
            Raylib.CloseWindow();
        }
    }
}