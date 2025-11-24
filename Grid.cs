using System.Numerics;
using BeyondIndustry.Data;
using Raylib_cs;

namespace BeyondIndustry.Utils
{
    public class Grid
    {
        private static int[,] cells = new int[0, 0];
        private static int rows;
        private static int cols;
        
        // NEU: Flag ob Sprites verwendet werden sollen
        public static bool UseSprites = false;
        
        public static void Initialize()
        {
            cols = GlobalData.SCREEN_WIDTH / GlobalData.CELL_SIZE;
            rows = GlobalData.SCREEN_HEIGHT / GlobalData.CELL_SIZE;
            cells = new int[rows, cols];
        }
        
        public static void SetCell(int col, int row, int value)
        {
            if (col >= 0 && col < cols && row >= 0 && row < rows)
            {
                cells[row, col] = value;
            }
        }
        
        public static int GetCell(int col, int row)
        {
            if (col >= 0 && col < cols && row >= 0 && row < rows)
            {
                return cells[row, col];
            }
            return 0;
        }
        
        public static void FillAll(int value)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    cells[row, col] = value;
                }
            }
        }
        
        public static void ClearAll()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    cells[row, col] = 0;
                }
            }
        }
        
        // ERWEITERT: DrawCells mit Sprite-Support
        public static void DrawCells()
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int x = col * GlobalData.CELL_SIZE;
                    int y = row * GlobalData.CELL_SIZE;
                    int cellValue = cells[row, col];
                    
                    if (cellValue == 0) 
                    {
                        // Leer - zeichne weißen Hintergrund
                        Raylib.DrawRectangle(x, y, GlobalData.CELL_SIZE, GlobalData.CELL_SIZE, Color.White);
                        continue;
                    }
                    
                    // Versuche Sprite zu laden
                    string spriteName = GetSpriteNameForValue(cellValue);
                    
                    if (UseSprites && SpriteManager.HasSprite(spriteName))
                    {
                        // Zeichne Sprite
                        Texture2D sprite = SpriteManager.GetSprite(spriteName);
                        
                        // Berechne Skalierung (falls Sprite nicht 32x32 ist)
                        float scale = (float)GlobalData.CELL_SIZE / sprite.Width;
                        
                        Raylib.DrawTextureEx(
                            sprite,
                            new Vector2(x, y),
                            0f,           // Rotation
                            scale,        // Skalierung
                            Color.White   // Tint
                        );
                    }
                    else
                    {
                        // Fallback: Farb-Rechteck
                        Color cellColor = GetColorForCell(cellValue);
                        Raylib.DrawRectangle(x, y, GlobalData.CELL_SIZE, GlobalData.CELL_SIZE, cellColor);
                    }
                }
            }
        }
        
        // NEU: Sprite-Name für Wert
        private static string GetSpriteNameForValue(int value)
        {
            return value switch
            {
                1 => "floor",
                2 => "machine",
                3 => "furnace",
                _ => "unknown"
            };
        }
        
        // Farb-Fallback
        private static Color GetColorForCell(int value)
        {
            return value switch
            {
                0 => Color.White,
                1 => new Color(220, 220, 220, 255),
                2 => new Color(70, 130, 180, 255),
                3 => new Color(220, 20, 60, 255),
                _ => Color.White
            };
        }
        
        public static Vector2 getMousePositionInGrid()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            int gridX = (int)(mousePos.X / GlobalData.CELL_SIZE);
            int gridY = (int)(mousePos.Y / GlobalData.CELL_SIZE);
            return new Vector2(gridX, gridY);
        }
    }
}