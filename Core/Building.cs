using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Core
{
    /// <summary>
    /// Basisklasse für alle Gebäude
    /// </summary>
    public class Building
    {
        public Vector2 GridPosition { get; set; }
        public string Name { get; protected set; }
        public Color Color { get; protected set; }
        
        public Building(string name, Color color)
        {
            Name = name;
            Color = color;
        }
        
        /// <summary>
        /// Wird jeden Frame aufgerufen - für Logik/Animation
        /// </summary>
        public virtual void Update(float deltaTime)
        {
            // Override in abgeleiteten Klassen
        }
        
        /// <summary>
        /// Zeichnet das Gebäude
        /// </summary>
        public virtual void Draw(int cellSize)
        {
            int x = (int)GridPosition.X * cellSize;
            int y = (int)GridPosition.Y * cellSize;
            
            // Zeichne Rechteck mit kleinem Rand
            Raylib.DrawRectangle(
                x + 2,
                y + 2,
                cellSize - 4,
                cellSize - 4,
                Color
            );
        }
    }
}