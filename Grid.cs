using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Data;

namespace BeyondIndustry.Utils
{
    public class Grid
    {
        public int Size { get; set; } = 100;
        public float CellSize { get; set; } = 1.0f;
        
        public void Draw()
        {
            // Nur zeichnen wenn aktiviert
            if (!GlobalData.ShowGrid) return;
            
            int halfSize = Size / 2;
            
            // LOD basierend auf Kamera-Höhe
            float camHeight = GlobalData.camera.Position.Y;
            int lineStep = 1;
            if (camHeight > 20f) lineStep = 2;
            if (camHeight > 40f) lineStep = 5;
            if (camHeight > 80f) lineStep = 10;
            
            // Horizontale Linien (entlang Z-Achse)
            for (int i = -halfSize; i <= halfSize; i += lineStep)
            {
                float x = i * CellSize;
                Vector3 start = new Vector3(x, 0, -halfSize * CellSize);
                Vector3 end = new Vector3(x, 0, halfSize * CellSize);
                
                Color lineColor = GlobalColor.GRID_MAIN;
                if (i == 0)
                {
                    lineColor = GlobalColor.GRID_SUB;  // X-Achse
                }
                else if (i % 10 == 0)
                {
                    lineColor = new Color(120, 120, 120, 120);
                }
                
                Raylib.DrawLine3D(start, end, lineColor);
            }
            
            // Vertikale Linien (entlang X-Achse)
            for (int i = -halfSize; i <= halfSize; i += lineStep)
            {
                float z = i * CellSize;
                Vector3 start = new Vector3(-halfSize * CellSize, 0, z);
                Vector3 end = new Vector3(halfSize * CellSize, 0, z);
                
                Color lineColor = GlobalColor.GRID_MAIN;
                if (i == 0)
                {
                    lineColor = GlobalColor.GRID_SUB;  // Z-Achse
                }
                else if (i % 10 == 0)
                {
                    lineColor = new Color(120, 120, 120, 120);
                }
                
                Raylib.DrawLine3D(start, end, lineColor);
            }
            
            // Koordinaten-Marker im Debug-Modus
            if (GlobalData.ShowDebugInfo)
            {
                DrawCoordinateMarkers(halfSize, lineStep);
            }
        }
        
        private void DrawCoordinateMarkers(int halfSize, int step)
        {
            for (int x = -halfSize; x <= halfSize; x += step * 5)
            {
                for (int z = -halfSize; z <= halfSize; z += step * 5)
                {
                    if (x == 0 && z == 0) continue;
                    
                    Vector3 pos = new Vector3(x * CellSize, 0.1f, z * CellSize);
                    Vector2 screenPos = Raylib.GetWorldToScreen(pos, GlobalData.camera);
                    
                    if (screenPos.X > 0 && screenPos.X < GlobalData.SCREEN_WIDTH &&
                        screenPos.Y > 0 && screenPos.Y < GlobalData.SCREEN_HEIGHT)
                    {
                        string label = $"{x},{z}";
                        Raylib.DrawText(label, (int)screenPos.X, (int)screenPos.Y, 10, 
                            GlobalColor.WithAlpha(GlobalColor.TEXT_COLOR, 100));
                    }
                }
            }
        }
        
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int gridX = (int)MathF.Round(worldPos.X / CellSize);
            int gridZ = (int)MathF.Round(worldPos.Z / CellSize);
            return new Vector2Int(gridX, gridZ);
        }
        
        public Vector3 GridToWorld(int gridX, int gridZ, float y = 0f)
        {
            return new Vector3(gridX * CellSize, y, gridZ * CellSize);
        }
        
        public bool IsInBounds(int gridX, int gridZ)
        {
            int halfSize = Size / 2;
            return gridX >= -halfSize && gridX <= halfSize &&
                   gridZ >= -halfSize && gridZ <= halfSize;
        }
    }
    
    public struct Vector2Int
    {
        public int X { get; set; }
        public int Z { get; set; }
        
        public Vector2Int(int x, int z)
        {
            X = x;
            Z = z;
        }
        
        public override string ToString() => $"({X}, {Z})";
    }
}