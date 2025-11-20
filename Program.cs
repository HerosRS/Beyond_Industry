using Raylib_cs;
using BeyondIndustry.Utils;

namespace BeyondIndustry
{
    class Program
    {
        static int SCREEN_WIDTH = 800;
        static int SCREEN_HEIGHT = 450;
        static int CELL_SIZE = 32;

        static void Main()
        {
            // Fenster initialisieren
            Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Beyond Industry");
            Raylib.SetTargetFPS(60);

            // Haupt-Game-Loop
            while (!Raylib.WindowShouldClose())
            {
                // Update-Phase
                // Hier würde deine Logik stehen

                Grid grid = new Grid(CELL_SIZE, Color.Green);
                grid.Draw(SCREEN_WIDTH, SCREEN_HEIGHT);

                // Draw-Phase
                Raylib.BeginDrawing();
                Raylib.DrawText(grid.getMousePositionInGrid().ToString(), 10, 10, 20, Color.White);
                Raylib.ClearBackground(Color.DarkGray);
                Raylib.EndDrawing();
            }

            // Aufräumen
            Raylib.CloseWindow();
        }
    }
}