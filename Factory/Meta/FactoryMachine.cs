using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Data;

namespace BeyondIndustry.Factory
{
    public abstract class FactoryMachine : ISaveable
    {
        public string MachineType { get; set; } = "Unknown";
        public Vector3 Position { get; set; }
        public Model Model { get; set; }
        
        public bool IsManuallyEnabled { get; set; } = true;
        public bool IsRunning { get; protected set; } = false;
        
        public float ProductionCycleTime { get; set; } = 1.0f;
        public float PowerConsumption { get; set; } = 0f;
        
        protected float productionTimer = 0f;
        
        // ===== BUTTON SYSTEM =====
        protected Vector3 buttonPosition;
        protected bool isButtonHovered = false;
        private float buttonRadius = 0.15f;      // Kleiner Button
        private float buttonDistance = 0.8f;     // Näher an der Maschine
        private float maxClickDistance = 3.0f;   // NEU: Maximale Klick-Distanz
        
        protected FactoryMachine(Vector3 position, Model model)
        {
            Position = position;
            Model = model;
            UpdateButtonPosition();
        }
        
        // ===== ABSTRACT METHODS =====
        public abstract void Update(float deltaTime);
        protected abstract void Process();
        
        // ===== VIRTUAL METHODS (können überschrieben werden) =====
        protected virtual bool HasPower()
        {
            return true;
        }
        
        public virtual string GetDebugInfo()  // ← NICHT MEHR ABSTRACT!
        {
            return $"{MachineType} | Pos: ({Position.X:F0}, {Position.Y:F0}, {Position.Z:F0}) | " +
                   $"Enabled: {IsManuallyEnabled} | Running: {IsRunning}";
        }
        
        public virtual void Draw()
        {
            Raylib.DrawModel(Model, Position, 1.0f, Color.White);
        }
        
        // ===== BUTTON METHODS =====
        protected void UpdateButtonPosition()
        {
            buttonPosition = Position + new Vector3(0, 1.5f, 0);
        }
        
        public void CheckButtonClick(Vector2 mousePosition, Camera3D camera)
        {
            if (isButtonHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                IsManuallyEnabled = !IsManuallyEnabled;
                Console.WriteLine($"[{MachineType}] Button clicked - Enabled: {IsManuallyEnabled}");
            }
        }
        
        public virtual bool IsButtonHovered()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            Ray ray = Raylib.GetMouseRay(mousePos, Data.GlobalData.camera);
            
            Vector3 buttonPos = Position + new Vector3(0, buttonDistance, 0);
            
            // NEU: Prüfe zuerst Distanz zur Kamera
            float distanceToCamera = Vector3.Distance(Data.GlobalData.camera.Position, buttonPos);
            if (distanceToCamera > maxClickDistance)
                return false;
            
            // Prüfe Ray-Sphere Kollision
            RayCollision collision = Raylib.GetRayCollisionSphere(ray, buttonPos, buttonRadius);
            return collision.Hit;
        }
        
    protected void DrawButton(Camera3D camera)
    {
        // Berechne Button-Position (über der Maschine)
        Vector3 buttonPos = Position + new Vector3(0, buttonDistance, 0);
        
        // Prüfe Distanz zur Kamera
        float distanceToCamera = Vector3.Distance(camera.Position, buttonPos);
        
        // Zeige Button nur wenn Kamera nah genug ist
        if (distanceToCamera > 15.0f)  // Nur wenn näher als 15 Einheiten
            return;
        
        // Button-Farbe basierend auf Status
        Color buttonColor = IsManuallyEnabled ? Color.Green : Color.Red;
        
        // Hover-Check
        if (IsButtonHovered())
        {
            buttonColor = IsManuallyEnabled 
                ? new Color(100, 255, 100, 255)  // Helles Grün
                : new Color(255, 100, 100, 255); // Helles Rot
        }
        
        // Zeichne Button als Sphere
        Raylib.DrawSphere(buttonPos, buttonRadius, buttonColor);
        
        // Optional: Outline für bessere Sichtbarkeit
        Raylib.DrawSphereWires(buttonPos, buttonRadius + 0.02f, 8, 8, Color.White);
    }
        
        // ===== SAVEABLE IMPLEMENTATION =====
        public virtual string GetSaveId()
        {
            return MachineType;
        }
        
        public virtual Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "IsEnabled", IsManuallyEnabled },
                { "ProductionTimer", productionTimer }
            };
        }
        
        public virtual void Deserialize(Dictionary<string, object> data)
        {
            if (data.ContainsKey("IsEnabled"))
                IsManuallyEnabled = Convert.ToBoolean(data["IsEnabled"]);
            
            if (data.ContainsKey("ProductionTimer"))
                productionTimer = Convert.ToSingle(data["ProductionTimer"]);
        }
        
        public float GetProductionTimer() => productionTimer;
    }
}