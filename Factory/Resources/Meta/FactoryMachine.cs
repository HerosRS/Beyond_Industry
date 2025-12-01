using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    public abstract class FactoryMachine
    {
        // Bestehende Properties...
        public Vector3 Position { get; set; }
        public Model Model { get; protected set; }
        public string MachineType { get; protected set; } = "";
        public bool IsRunning { get; protected set; }
        public float PowerConsumption { get; set; }
        public float ProductionCycleTime { get; set; }
        protected float productionTimer;
        
        // NEU: Manueller On/Off Toggle
        public bool IsManuallyEnabled { get; set; } = true;  // Maschine manuell ein/aus
        
        // NEU: Interaktiver Button
        private Vector3 buttonOffset = new Vector3(0.5f, 0.5f, 1);  // Rechts an der Seite
        private float buttonSize = 0.3f;
        
        public FactoryMachine(Vector3 position, Model model)
        {
            Position = position;
            Model = model;
            IsRunning = false;
            productionTimer = 0f;
            PowerConsumption = 5f;
            ProductionCycleTime = 1.0f;
        }
        
        public virtual void Update(float deltaTime)
        {
            // Nur laufen wenn manuell aktiviert UND Strom vorhanden
            IsRunning = IsManuallyEnabled && HasPower();
            
            if (!IsRunning) return;
            
            productionTimer += deltaTime;
            
            if (productionTimer >= ProductionCycleTime)
            {
                Process();
                productionTimer = 0f;
            }
        }
        
        protected virtual bool HasPower()
        {
            return true;
        }
        
        protected abstract void Process();
        
        public virtual void Draw()
        {
            // Button zeichnen
            DrawToggleButton();
        }
        
        // ===== TOGGLE BUTTON ZEICHNEN =====
        protected void DrawToggleButton()
        {
            Vector3 buttonPos = Position + buttonOffset;
            
            // Button-Box
            Color buttonColor = IsManuallyEnabled ? 
                new Color(0, 200, 0, 200) :      // Grün wenn an
                new Color(200, 0, 0, 200);        // Rot wenn aus
            
            Raylib.DrawCube(buttonPos, buttonSize, buttonSize, buttonSize, buttonColor);
            Raylib.DrawCubeWires(buttonPos, buttonSize, buttonSize, buttonSize, Color.White);
            
            // Symbol auf dem Button
            if (IsManuallyEnabled)
            {
                // "I" für On
                Vector3 lineTop = buttonPos + new Vector3(0, buttonSize * 0.2f, buttonSize * 0.5f);
                Vector3 lineBottom = buttonPos + new Vector3(0, -buttonSize * 0.2f, buttonSize * 0.5f);
                Raylib.DrawLine3D(lineTop, lineBottom, Color.White);
            }
            else
            {
                // "O" für Off (kleiner Kreis)
                Vector3 circleCenter = buttonPos + new Vector3(0, 0, buttonSize * 0.5f);
                Raylib.DrawSphere(circleCenter, buttonSize * 0.15f, Color.White);
            }
            
            // Hover-Effekt
            if (IsButtonHovered())
            {
                // Glowing Outline
                Raylib.DrawCubeWires(buttonPos, buttonSize * 1.1f, buttonSize * 1.1f, buttonSize * 1.1f, 
                    Color.Yellow);
            }
        }
        
        // ===== BUTTON HOVER DETECTION =====
        public bool IsButtonHovered()
        {
            Vector3 buttonPos = Position + buttonOffset;
            
            // Maus-Ray zur Welt konvertieren
            Ray mouseRay = Raylib.GetScreenToWorldRay(Raylib.GetMousePosition(), Data.GlobalData.camera);
            
            // Bounding Box für Button
            BoundingBox buttonBox = new BoundingBox(
                buttonPos - new Vector3(buttonSize / 2),
                buttonPos + new Vector3(buttonSize / 2)
            );
            
            // Ray-Box Collision
            RayCollision collision = Raylib.GetRayCollisionBox(mouseRay, buttonBox);
            return collision.Hit;
        }
        
        // ===== BUTTON CLICK HANDLING =====
        public bool CheckButtonClick()
        {
            if (IsButtonHovered() && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                ToggleMachine();
                return true;
            }
            return false;
        }
        
        // ===== MASCHINE TOGGLEN =====
        public void ToggleMachine()
        {
            IsManuallyEnabled = !IsManuallyEnabled;
            Console.WriteLine($"[{MachineType}] @ {Position}: {(IsManuallyEnabled ? "EIN" : "AUS")}");
        }
        
        public virtual string GetDebugInfo()
        {
            string status = IsManuallyEnabled ? 
                (IsRunning ? "RUNNING" : "NO POWER") : 
                "DISABLED";
            
            return $"{MachineType} | {status} | Power: {PowerConsumption}W";
        }
    }
}