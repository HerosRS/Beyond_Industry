using System.Numerics;
using BeyondIndustry.Data;
using Raylib_cs;

namespace BeyondIndustry.Utils
{
    public class Grid
    {
        // Grid-Daten (2D Array bleibt - das ist deine Spiel-Logik!)
        private int[,] grid;
        
        public Grid(int width = 100, int height = 100)
        {
            grid = new int[width, height];
        }
        
        // Neue 3D-Methode: Maus zu 3D-Grid mit Raycast
        public Vector2 GetMousePositionIn3DGrid()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Ray ray = Raylib.GetScreenToWorldRay(mousePos, GlobalData.camera);
            
            // Raycast auf Y=0 Ebene (Boden)
            if (ray.Direction.Y != 0) // Division durch 0 vermeiden
            {
                float t = -ray.Position.Y / ray.Direction.Y;
                
                // Nur wenn der Strahl nach unten geht (t > 0)
                if (t > 0)
                {
                    Vector3 hitPoint = ray.Position + ray.Direction * t;

                    // Grid-Koordinaten berechnen
                    int gridX = (int)(hitPoint.X / GlobalData.CELL_SIZE);
                    int gridZ = (int)(hitPoint.Z / GlobalData.CELL_SIZE);
                    
                    // Bounds checking
                    if (gridX >= 0 && gridX < grid.GetLength(0) && 
                        gridZ >= 0 && gridZ < grid.GetLength(1))
                    {
                        return new Vector2(gridX, gridZ);
                    }
                }
            }
            
            return new Vector2(-1, -1); // Ungültige Position
        }

        // Grid-Zugriff Methoden
        public int GetCell(int x, int z)
        {
            if (x >= 0 && x < grid.GetLength(0) && z >= 0 && z < grid.GetLength(1))
                return grid[x, z];
            return -1;
        }

        public void SetCell(int x, int z, int value)
        {
            if (x >= 0 && x < grid.GetLength(0) && z >= 0 && z < grid.GetLength(1))
                grid[x, z] = value;
        }

        public bool IsValidPosition(int x, int z)
        {
            return x >= 0 && x < grid.GetLength(0) && z >= 0 && z < grid.GetLength(1);
        }

        // 3D World Position von Grid-Koordinaten berechnen
        public static Vector3 GridToWorld(int x, int z)
        {
            return new Vector3(
                x * GlobalData.CELL_SIZE + GlobalData.CELL_SIZE / 2,  // Zentriert
                0, 
                z * GlobalData.CELL_SIZE + GlobalData.CELL_SIZE / 2   // Zentriert
            );
        }

        // Grid Größe
        public int GetWidth() => grid.GetLength(0);
        public int GetHeight() => grid.GetLength(1);
    }
}