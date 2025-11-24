using Raylib_cs;
using BeyondIndustry.Utils;
using BeyondIndustry.Data;
using BeyondIndustry.DebugView;
using BeyondIndustry.Debug;
using System.Numerics;

namespace BeyondIndustry
{
    class Program
    {
        static void Main()
        {
            Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");
            Raylib.SetTargetFPS(60);

            Grid.Initialize();
            
            // NEU: Sprites laden
            SpriteManager.LoadSprite("floor", "Resources/sprites/floor.png");
            SpriteManager.LoadSprite("machine", "Resources/sprites/machine.png");
            SpriteManager.LoadSprite("furnace", "Resources/sprites/furnace.png");
            
            // NEU: Sprites aktivieren
            Grid.UseSprites = true;
            
            int selectedTool = 1;

            while (!Raylib.WindowShouldClose())
            {
                // ===== UPDATE =====
                DebugConsole.Update();
                
                if (!DebugConsole.IsOpen())
                {
                    Vector2 mouseGridPos = Grid.getMousePositionInGrid();
                    int mouseCol = (int)mouseGridPos.X;
                    int mouseRow = (int)mouseGridPos.Y;
                    
                    if (Raylib.IsKeyPressed(KeyboardKey.One))
                        selectedTool = 1;
                    if (Raylib.IsKeyPressed(KeyboardKey.Two))
                        selectedTool = 2;
                    if (Raylib.IsKeyPressed(KeyboardKey.Three))
                        selectedTool = 3;
                    
                    if (Raylib.IsMouseButtonDown(MouseButton.Left))
                        Grid.SetCell(mouseCol, mouseRow, selectedTool);
                    
                    if (Raylib.IsMouseButtonDown(MouseButton.Right))
                        Grid.SetCell(mouseCol, mouseRow, 0);
                }
                
                // ===== DRAW =====
                Raylib.BeginDrawing();
                Raylib.ClearBackground(GlobalColor.BACKGROUND_COLOR);
                
                Grid.DrawCells();
               
                
                if (!DebugConsole.IsOpen())
                {
                    string toolName = selectedTool switch
                    {
                        1 => "Boden",
                        2 => "Maschine",
                        3 => "Ofen",
                        _ => "Unbekannt"
                    };
                    Raylib.DrawText($"Werkzeug: {toolName}", 10, 100, 20, Color.Black);
                }
                
                UI.DebugDataUI();
                DebugConsole.Draw();
                
                Raylib.EndDrawing();
            }

            // Aufräumen
            SpriteManager.UnloadAll();
            Raylib.CloseWindow();
        }
    }
}