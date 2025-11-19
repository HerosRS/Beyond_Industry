using System.Collections.Generic;
using System.Numerics;

namespace BeyondIndustry.Core
{
    /// <summary>
    /// Verwaltet das Grid-System und alle platzierten Gebäude
    /// </summary>
    public class GridManager
    {
        public int Width { get; }
        public int Height { get; }
        public int CellSize { get; }
        
        // Speichert alle Gebäude: Key = Grid-Position, Value = Building-Objekt
        private Dictionary<Vector2, Building> _buildings = new();
        
        public GridManager(int width, int height, int cellSize)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
        }
        
        /// <summary>
        /// Konvertiert Pixel-Position zu Grid-Koordinaten
        /// </summary>
        public Vector2 WorldToGrid(Vector2 worldPos)
        {
            return new Vector2(
                (int)(worldPos.X / CellSize),
                (int)(worldPos.Y / CellSize)
            );
        }
        
        /// <summary>
        /// Konvertiert Grid-Koordinaten zu Pixel-Position
        /// </summary>
        public Vector2 GridToWorld(Vector2 gridPos)
        {
            return new Vector2(
                gridPos.X * CellSize,
                gridPos.Y * CellSize
            );
        }
        
        /// <summary>
        /// Prüft ob Position gültig und frei ist
        /// </summary>
        public bool IsPositionValid(Vector2 gridPos)
        {
            // Außerhalb des Grids?
            if (gridPos.X < 0 || gridPos.X >= Width) return false;
            if (gridPos.Y < 0 || gridPos.Y >= Height) return false;
            
            // Bereits belegt?
            return !_buildings.ContainsKey(gridPos);
        }
        
        /// <summary>
        /// Platziert ein Gebäude auf dem Grid
        /// </summary>
        public bool PlaceBuilding(Vector2 gridPos, Building building)
        {
            if (!IsPositionValid(gridPos))
                return false;
            
            building.GridPosition = gridPos;
            _buildings[gridPos] = building;
            return true;
        }
        
        /// <summary>
        /// Entfernt ein Gebäude vom Grid
        /// </summary>
        public bool RemoveBuilding(Vector2 gridPos)
        {
            return _buildings.Remove(gridPos);
        }
        
        /// <summary>
        /// Gibt alle Gebäude zurück (zum Zeichnen)
        /// </summary>
        public IEnumerable<Building> GetAllBuildings()
        {
            return _buildings.Values;
        }
        
        /// <summary>
        /// Gibt Anzahl der Gebäude zurück
        /// </summary>
        public int BuildingCount => _buildings.Count;
    }
}