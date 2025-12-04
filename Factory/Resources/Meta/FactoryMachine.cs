using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    public abstract class FactoryMachine
    {
        public Vector3 Position { get; set; }
        public Model Model { get; protected set; }
        public string MachineType { get; protected set; } = "";
        public bool IsRunning { get; protected set; }
        public float PowerConsumption { get; set; }
        public float ProductionCycleTime { get; set; }
        protected float productionTimer;

        public bool IsManuallyEnabled { get; set; } = true;

        private Vector3 buttonOffset = new Vector3(0.5f, 0.5f, 1f);
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
            IsRunning = IsManuallyEnabled && HasPower();

            if (!IsRunning)
                return;

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
            // Hier dein Model zeichnen, falls benötigt
            // Raylib.DrawModel(Model, Position, 1.0f, Color.White);

            DrawToggleButton();
        }

        protected void DrawToggleButton()
        {
            Vector3 buttonPos = Position + buttonOffset;

            // Panel hinter dem Button
            float panelSize = buttonSize * 1.8f;
            Vector3 panelPos = buttonPos + new Vector3(0, 0, -buttonSize * 0.2f);
            Raylib.DrawCube(panelPos, panelSize, panelSize, buttonSize * 0.3f, new Color(30, 30, 35, 255));
            Raylib.DrawCubeWires(panelPos, panelSize, panelSize, buttonSize * 0.3f, new Color(80, 80, 90, 255));

            // Button-Körper
            Color stateColor = IsManuallyEnabled
                ? (IsRunning ? new Color(0, 200, 60, 255) : new Color(220, 180, 0, 255))
                : new Color(160, 0, 0, 255);

            Vector3 buttonTopPos = buttonPos + new Vector3(0, 0, buttonSize * 0.15f);
            Raylib.DrawCube(buttonTopPos, buttonSize, buttonSize, buttonSize * 0.7f, stateColor);
            Raylib.DrawCubeWires(buttonTopPos, buttonSize, buttonSize, buttonSize * 0.7f, Color.Black);

            // Symbol je nach Zustand
            if (IsManuallyEnabled)
            {
                Vector3 lineTop = buttonTopPos + new Vector3(0, buttonSize * 0.2f, buttonSize * 0.5f);
                Vector3 lineBottom = buttonTopPos + new Vector3(0, -buttonSize * 0.2f, buttonSize * 0.5f);
                Raylib.DrawLine3D(lineTop, lineBottom, Color.White);
            }
            else
            {
                Vector3 circleCenter = buttonTopPos + new Vector3(0, 0, buttonSize * 0.5f);
                Raylib.DrawSphere(circleCenter, buttonSize * 0.15f, Color.White);
            }

            // Hover-Glow
            if (IsButtonHovered())
            {
                Raylib.DrawCubeWires(
                    buttonTopPos,
                    buttonSize * 1.2f,
                    buttonSize * 1.2f,
                    buttonSize * 0.9f,
                    new Color(255, 255, 0, 200)
                );
            }
        }

        public bool IsButtonHovered()
        {
            Vector3 buttonPos = Position + buttonOffset;

            Ray mouseRay = Raylib.GetScreenToWorldRay(Raylib.GetMousePosition(), Data.GlobalData.camera);

            BoundingBox buttonBox = new BoundingBox(
                buttonPos - new Vector3(buttonSize / 2f),
                buttonPos + new Vector3(buttonSize / 2f)
            );

            RayCollision collision = Raylib.GetRayCollisionBox(mouseRay, buttonBox);
            return collision.Hit;
        }

        public bool CheckButtonClick()
        {
            if (IsButtonHovered() && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                ToggleMachine();
                return true;
            }
            return false;
        }

        public void ToggleMachine()
        {
            IsManuallyEnabled = !IsManuallyEnabled;
            Console.WriteLine($"[{MachineType}] @ {Position}: {(IsManuallyEnabled ? "EIN" : "AUS")}");
        }

        public virtual string GetDebugInfo()
        {
            string status = IsManuallyEnabled
                ? (IsRunning ? "RUNNING" : "NO POWER")
                : "DISABLED";

            return $"{MachineType} | {status} | Power: {PowerConsumption}W";
        }

        // Optional: 2D-Tablo (z.B. für selektierte Maschine)
        public virtual void DrawTablo2D(int x, int y)
        {
            int width = 220;
            int height = 80;

            Raylib.DrawRectangle(x, y, width, height, new Color(25, 25, 30, 220));
            Raylib.DrawRectangleLines(x, y, width, height, new Color(120, 120, 140, 255));

            Raylib.DrawRectangle(x, y, width, 20, new Color(40, 40, 55, 255));
            Raylib.DrawText(MachineType, x + 6, y + 3, 12, Color.White);

            Color statusColor = !IsManuallyEnabled ? Color.Red :
                                (IsRunning ? Color.Lime : Color.Yellow);

            Raylib.DrawText(
                $"Status: {(IsManuallyEnabled ? (IsRunning ? "RUN" : "NO POWER") : "DISABLED")}",
                x + 6, y + 26, 12, statusColor);
            Raylib.DrawText($"Power: {PowerConsumption:F1} W", x + 6, y + 40, 12, Color.LightGray);
            Raylib.DrawText($"Cycle: {ProductionCycleTime:F2} s", x + 6, y + 54, 12, Color.LightGray);
        }
    }
}
