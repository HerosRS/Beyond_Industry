using System;
using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Buildings;

namespace BeyondIndustry.Core
{
    public class Game
    {
        private GridManager _gridManager;
        private Type _selectedBuildingType;
        
        public Game(int gridWidth, int gridHeight, int cellSize)
        {
            _gridManager = new GridManager(gridWidth, gridHeight, cellSize);
            _selectedBuildingType = typeof(Factory);  // Standard: Factory
        }
        
        /// <summary>
        /// Initialisiert das Fenster
        /// </summary>
        public void Initialize()
        {
            int windowWidth = _gridManager.Width * _gridManager.CellSize;
            int windowHeight = _gridManager.Height * _gridManager.CellSize;
            
            Raylib.InitWindow(windowWidth, windowHeight, "Factory Game 2D");
            Raylib.SetTargetFPS(60);
        }
        
        /// <summary>
        /// Haupt-Game-Loop
        /// </summary>
        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                float deltaTime = Raylib.GetFrameTime();
                
                Update(deltaTime);
                Draw();
            }
            
            Cleanup();
        }
        
        private void Update(float deltaTime)
        {
            HandleInput();
            
            // Update alle Gebäude
            foreach (var building in _gridManager.GetAllBuildings())
            {
                building.Update(deltaTime);
            }
        }
        
        private void HandleInput()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Vector2 gridPos = _gridManager.WorldToGrid(mousePos);
            
            // Tastatur: Wähle Gebäude-Typ
            if (Raylib.IsKeyPressed(KeyboardKey.One))
                _selectedBuildingType = typeof(Factory);
            if (Raylib.IsKeyPressed(KeyboardKey.Two))
                _selectedBuildingType = typeof(Miner);
            if (Raylib.IsKeyPressed(KeyboardKey.Three))
                _selectedBuildingType = typeof(Storage);
            if (Raylib.IsKeyPressed(KeyboardKey.Four))        // ← NEU
                _selectedBuildingType = typeof(Smelter);
            
            // Linksklick: Platziere Gebäude
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (_gridManager.IsPositionValid(gridPos))
                {
                    Building? building = CreateBuilding(_selectedBuildingType);
                    if (building != null)
                    {
                        _gridManager.PlaceBuilding(gridPos, building);
                    }
                }
            }
            
            // Rechtsklick: Entferne Gebäude
            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                _gridManager.RemoveBuilding(gridPos);
            }
        }
        
        private Building? CreateBuilding(Type buildingType)
        {
            if (buildingType == typeof(Factory))
                return new Factory();
            if (buildingType == typeof(Miner))
                return new Miner();
            if (buildingType == typeof(Storage))
                return new Storage();
            if (buildingType == typeof(Smelter))              // ← NEU
                return new Smelter();        
            
            
            return null;
        }
        
        private void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            
            DrawGrid();
            DrawBuildings();
            DrawPreview();
            DrawUI();
            
            Raylib.EndDrawing();
        }
        
        private void DrawGrid()
        {
            int width = _gridManager.Width * _gridManager.CellSize;
            int height = _gridManager.Height * _gridManager.CellSize;
            
            // Vertikale Linien
            for (int x = 0; x <= _gridManager.Width; x++)
            {
                int posX = x * _gridManager.CellSize;
                Raylib.DrawLine(posX, 0, posX, height, Color.DarkGray);
            }
            
            // Horizontale Linien
            for (int y = 0; y <= _gridManager.Height; y++)
            {
                int posY = y * _gridManager.CellSize;
                Raylib.DrawLine(0, posY, width, posY, Color.DarkGray);
            }
        }
        
        private void DrawBuildings()
        {
            foreach (var building in _gridManager.GetAllBuildings())
            {
                building.Draw(_gridManager.CellSize);
            }
        }
        
        private void DrawPreview()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Vector2 gridPos = _gridManager.WorldToGrid(mousePos);
            
            if (gridPos.X >= 0 && gridPos.X < _gridManager.Width &&
                gridPos.Y >= 0 && gridPos.Y < _gridManager.Height)
            {
                bool isValid = _gridManager.IsPositionValid(gridPos);
                Color previewColor = isValid 
                    ? new Color(0, 255, 0, 80) 
                    : new Color(255, 0, 0, 80);
                
                Vector2 worldPos = _gridManager.GridToWorld(gridPos);
                Raylib.DrawRectangle(
                    (int)worldPos.X,
                    (int)worldPos.Y,
                    _gridManager.CellSize,
                    _gridManager.CellSize,
                    previewColor
                );
            }
        }
        
        private void DrawUI()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Vector2 gridPos = _gridManager.WorldToGrid(mousePos);
            
            Raylib.DrawText($"Grid: ({gridPos.X}, {gridPos.Y})", 10, 10, 20, Color.White);
            Raylib.DrawText($"Buildings: {_gridManager.BuildingCount}", 10, 35, 20, Color.White);
            Raylib.DrawText($"Selected: {_selectedBuildingType.Name}", 10, 60, 20, Color.Yellow);
                Raylib.DrawText("1: Factory | 2: Miner | 3: Storage | 4: Smelter", 10, 85, 16, Color.LightGray);  // ← GEÄNDERT
            Raylib.DrawText("LMB: Place | RMB: Remove", 10, 105, 16, Color.LightGray);
        }
        
        private void Cleanup()
        {
            Raylib.CloseWindow();
        }
    }
}