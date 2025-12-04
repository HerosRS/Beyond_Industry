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
        public Model Model { get; protected set; }
        
        public bool IsManuallyEnabled { get; set; } = true;
        public bool IsRunning { get; protected set; } = false;
        
        public float ProductionCycleTime { get; set; } = 1.0f;
        public float PowerConsumption { get; set; } = 0f;
        
        protected float productionTimer = 0f;
        
        // ===== BUTTON SYSTEM =====
        protected Vector3 buttonPosition;
        protected float buttonRadius = 0.3f;
        protected bool isButtonHovered = false;
        
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
        
        public bool IsButtonHovered()
        {
            return isButtonHovered;
        }
        
        protected void DrawButton(Camera3D camera)
        {
            Ray mouseRay = Raylib.GetScreenToWorldRay(Raylib.GetMousePosition(), camera);
            float distance = Vector3.Distance(mouseRay.Position, buttonPosition);
            Vector3 toButton = buttonPosition - mouseRay.Position;
            float dotProduct = Vector3.Dot(Vector3.Normalize(toButton), mouseRay.Direction);
            
            isButtonHovered = distance < 50f && dotProduct > 0.98f;
            
            Color buttonColor = IsManuallyEnabled ? 
                (isButtonHovered ? Color.Lime : Color.Green) : 
                (isButtonHovered ? Color.Orange : Color.Red);
            
            Raylib.DrawSphere(buttonPosition, buttonRadius, buttonColor);
            
            if (isButtonHovered)
            {
                Raylib.DrawSphereWires(buttonPosition, buttonRadius + 0.05f, 8, 8, Color.Yellow);
            }
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