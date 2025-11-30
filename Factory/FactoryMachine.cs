using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== BASIS-KLASSE FÜR ALLE FACTORY-MASCHINEN =====
    public abstract class FactoryMachine
    {
        // Grundlegende Eigenschaften
        public Vector3 Position { get; set; }
        public string MachineType { get; protected set; } = "";
        public bool IsRunning { get; set; }
        public Model Model { get; set; }
        public Color Tint { get; set; }
        
        // Energie-System
        public float PowerConsumption { get; set; }
        public float CurrentPower { get; set; }
        
        // Timer für Produktion
        protected float productionTimer = 0f;
        public float ProductionCycleTime { get; set; }
        
        // Status-Anzeige
        public float ProductionProgress => ProductionCycleTime > 0 ? productionTimer / ProductionCycleTime : 0f;
        
        // Konstruktor
        protected FactoryMachine(Vector3 position, Model model)
        {
            Position = position;
            Model = model;
            Tint = Color.White;
            IsRunning = false;
            CurrentPower = 0f;
        }
        
        // ===== UPDATE-METHODE =====
        public virtual void Update(float deltaTime)
        {
            // Nur aktiv wenn genug Strom vorhanden
            if (CurrentPower >= PowerConsumption)
            {
                IsRunning = true;
                
                // Timer hochzählen
                productionTimer += deltaTime;
                
                // Wenn Timer abgelaufen, produziere etwas
                if (productionTimer >= ProductionCycleTime)
                {
                    Process();
                    productionTimer = 0f;
                }
            }
            else
            {
                IsRunning = false;
            }
        }
        
        // ===== ABSTRAKTE VERARBEITUNGS-METHODE =====
        protected abstract void Process();
        
        // ===== DRAW-METHODE =====
        public virtual void Draw()
        {
            // Ändere Farbe basierend auf Status
            Color drawColor = IsRunning ? Color.Green : Color.Gray;
            Raylib.DrawModel(Model, Position, 1.0f, drawColor);
            
            // Zeige Produktions-Fortschritt als kleine Bar über der Maschine
            if (IsRunning)
            {
                Vector3 barPos = new Vector3(Position.X, Position.Y + 1.5f, Position.Z);
                float barWidth = 0.8f;
                float barHeight = 0.1f;
                
                // Hintergrund (grau)
                Raylib.DrawCube(barPos, barWidth, barHeight, 0.05f, Color.DarkGray);
                
                // Fortschritts-Bar (grün)
                float progressWidth = barWidth * ProductionProgress;
                Vector3 progressPos = new Vector3(
                    barPos.X - (barWidth - progressWidth) / 2, 
                    barPos.Y, 
                    barPos.Z
                );
                Raylib.DrawCube(progressPos, progressWidth, barHeight, 0.05f, Color.Lime);
            }
        }
        
        // ===== DEBUG-INFO =====
        public virtual string GetDebugInfo()
        {
            return $"{MachineType} | Running: {IsRunning} | Power: {CurrentPower:F0}/{PowerConsumption:F0} | Progress: {ProductionProgress:P0}";
        }
    }
}