using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Data;
public class Building
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Building(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
    }

    public static void DrawBorderWallWithModel(Model wandModel, int gridSize, float cellSize)
        {
            // Nord-Wand (Z = 0)
            for (int x = 0; x < gridSize; x++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, 0);
                Raylib.DrawModelEx(wandModel, pos, new Vector3(0, 1, 0), 90.0f, Vector3.One, Color.White);
            }
                        
            // West-Wand (X = 0)
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 pos = new Vector3(0, 0, z * cellSize);
                // 90° rotiert für Seiten-Wände
                Raylib.DrawModelEx(wandModel, pos, new Vector3(0, 1, 0), 0.0f, Vector3.One,  Color.White);
            }
            
        }
}