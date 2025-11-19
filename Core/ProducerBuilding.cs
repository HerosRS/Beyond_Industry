using System;
using Raylib_cs;

namespace BeyondIndustry.Core
{
    /// <summary>
    /// Basisklasse für Gebäude die Items produzieren
    /// </summary>
    public class ProducerBuilding : Building
    {
        public Item OutputItem { get; protected set; }
        public float ProductionTime { get; protected set; }  // Sekunden pro Item
        public ItemStack Storage { get; protected set; }
        public int MaxStorage { get; protected set; }
        
        private float _productionProgress = 0f;  // 0.0 bis 1.0
        
        public ProducerBuilding(string name, Color color, Item outputItem, float productionTime, int maxStorage = 10)
            : base(name, color)
        {
            OutputItem = outputItem;
            ProductionTime = productionTime;
            MaxStorage = maxStorage;
            Storage = new ItemStack(outputItem, 0);
        }
        
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            
            // Nur produzieren wenn Lager nicht voll
            if (Storage.Count < MaxStorage)
            {
                _productionProgress += deltaTime / ProductionTime;
                
                // Item fertig produziert?
                if (_productionProgress >= 1.0f)
                {
                    _productionProgress = 0f;
                    Storage.Add(1);
                }
            }
        }
        
        public override void Draw(int cellSize)
        {
            base.Draw(cellSize);
            
            int x = (int)GridPosition.X * cellSize;
            int y = (int)GridPosition.Y * cellSize;
            
            // Zeichne Produktions-Fortschritt (Balken)
            if (_productionProgress > 0 && Storage.Count < MaxStorage)
            {
                int barWidth = (int)((cellSize - 8) * _productionProgress);
                Raylib.DrawRectangle(
                    x + 4,
                    y + cellSize - 8,
                    barWidth,
                    4,
                    Color.Green
                );
            }
            
            // Zeichne kleinen Punkt für das Item im Lager
            if (Storage.Count > 0)
            {
                int dotSize = 6;
                Raylib.DrawRectangle(
                    x + cellSize / 2 - dotSize / 2,
                    y + cellSize / 2 - dotSize / 2,
                    dotSize,
                    dotSize,
                    OutputItem.Color
                );
                
                // Zeige Anzahl
                string countText = Storage.Count.ToString();
                Raylib.DrawText(
                    countText,
                    x + cellSize / 2 + 6,
                    y + cellSize / 2 - 6,
                    12,
                    Color.White
                );
            }
        }
    }
}