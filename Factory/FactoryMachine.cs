using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== BASIS-KLASSE FÜR ALLE FACTORY-MASCHINEN =====
    // Jede Maschine in deiner Factory erbt von dieser Klasse
    public abstract class FactoryMachine
    {
        // Grundlegende Eigenschaften jeder Maschine
        public Vector3 Position { get; set; }           // Wo steht die Maschine?
        public string MachineType { get; protected set; }   // Was für eine Maschine (z.B. "Miner", "Smelter")
        public bool IsRunning { get; set; }             // Läuft die Maschine gerade?
        public Model Model { get; set; }                // 3D-Modell
        public Color Tint { get; set; }                 // Farbe der Maschine
        
        // Energie-System
        public float PowerConsumption { get; set; }   // Wie viel Strom verbraucht sie?
        public float CurrentPower { get; set; }                 // Wie viel Strom hat sie gerade?
        
        // Timer für Produktion
        protected float productionTimer = 0f;           // Interner Timer
        public float ProductionCycleTime { get; protected set; }    // Wie lange dauert ein Produktions-Zyklus?
        
        // Status-Anzeige
        public float ProductionProgress => ProductionCycleTime > 0 ? productionTimer / ProductionCycleTime : 0f;
        
        // Konstruktor - wird aufgerufen wenn eine neue Maschine erstellt wird
        protected FactoryMachine(Vector3 position, Model model)
        {
            Position = position;
            Model = model;
            Tint = Color.White;
            IsRunning = false;
            CurrentPower = 0f;
        }
        
        // ===== UPDATE-METHODE =====
        // Diese Methode wird jeden Frame aufgerufen und verarbeitet die Maschinen-Logik
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
                    Process();  // Rufe Verarbeitungs-Methode auf
                    productionTimer = 0f;  // Reset Timer
                }
            }
            else
            {
                IsRunning = false;
                // Timer läuft nicht weiter wenn kein Strom
            }
        }
        
        // ===== ABSTRAKTE VERARBEITUNGS-METHODE =====
        // Jede Maschine muss ihre eigene Process-Methode implementieren
        protected abstract void Process();
        
        // ===== DRAW-METHODE =====
        // Zeichnet die Maschine (kann in abgeleiteten Klassen überschrieben werden)
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