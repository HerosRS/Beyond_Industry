using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory.Resources;

namespace BeyondIndustry.Factory
{
    public class ConveyorItem
    {
        public string ResourceType { get; set; }
        public int Amount { get; set; }
        public float Progress { get; set; }
        
        public ConveyorItem(string resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
            Progress = 0.0f;
        }
    }
    
    // ===== BELT TYPEN =====
    public enum BeltType
    {
        Straight,      // Gerades Belt
        CurveLeft,     // 90° Kurve nach links
        CurveRight,    // 90° Kurve nach rechts
        Merger,        // 2 Inputs → 1 Output
        Splitter,      // 1 Input → 2 Outputs
        RampUp,        // Rampe hoch
        RampDown,      // Rampe runter
        Crossing       // Kreuzung
    }
    
    public class ConveyorBelt : FactoryMachine
    {
        // ===== VERBINDUNGEN =====
        public FactoryMachine? InputMachine { get; set; }
        public FactoryMachine? OutputMachine { get; set; }
        public FactoryMachine? SecondaryInputMachine { get; set; }
        public FactoryMachine? SecondaryOutputMachine { get; set; }
        
        // ===== EIGENSCHAFTEN =====
        public BeltType Type { get; private set; }
        public Vector3 Direction { get; private set; }
        public Vector3 SecondaryDirection { get; private set; }  // Für Kurven/Merger/Splitter
        public float BeltSpeed { get; set; } = 1.0f;
        public int MaxItemsOnBelt { get; set; } = 6;
        public float ItemHeight { get; set; } = 0.3f;
        
        public float SpawnPoint { get; set; } = -0.5f;
        public float EndPoint { get; set; } = 1.5f;
        public float MinItemSpacing { get; set; } = 0.33f;
        public float BeltLength { get; set; } = 1.0f;
        
        private List<ConveyorItem> items = new List<ConveyorItem>();
        private float updateAccumulator = 0f;
        private const float UPDATE_RATE = 1f / 60f;
        
        public ConveyorBelt(Vector3 position, Model model, Vector3 direction, BeltType type = BeltType.Straight) 
            : base(position, model)
        {
            MachineType = "ConveyorBelt";
            Type = type;
            Direction = Vector3.Normalize(direction);
            
            // Berechne Secondary Direction für Kurven
            CalculateSecondaryDirection();
            
            ProductionCycleTime = 0.3f;
            PowerConsumption = 2f;
        }
        
        // ===== BERECHNE SECONDARY DIRECTION =====
        private void CalculateSecondaryDirection()
        {
            if (Type == BeltType.CurveLeft)
            {
                // Drehe Direction 90° nach links (gegen Uhrzeigersinn)
                SecondaryDirection = new Vector3(-Direction.Z, 0, Direction.X);
            }
            else if (Type == BeltType.CurveRight)
            {
                // Drehe Direction 90° nach rechts (im Uhrzeigersinn)
                SecondaryDirection = new Vector3(Direction.Z, 0, -Direction.X);
            }
            else
            {
                SecondaryDirection = Direction;
            }
        }
        
        protected override bool HasPower()
        {
            return true;
        }
        
        public override void Update(float deltaTime)
        {
            IsRunning = IsManuallyEnabled && HasPower();
            
            if (Data.GlobalData.ShowDebugInfo)
            {
                HandleDebugInput();
            }
            
            if (!IsRunning) return;
            
            updateAccumulator += deltaTime;
            
            while (updateAccumulator >= UPDATE_RATE)
            {
                UpdateItems(UPDATE_RATE);
                updateAccumulator -= UPDATE_RATE;
            }
            
            productionTimer += deltaTime;
            if (productionTimer >= ProductionCycleTime)
            {
                Process();
                productionTimer = 0f;
            }
        }
        
        private void HandleDebugInput()
        {
            if (!IsNearMouse()) return;
            
            float adjustSpeed = 0.01f;
            
            if (Raylib.IsKeyDown(KeyboardKey.Kp7))
            {
                SpawnPoint += adjustSpeed;
                if (SpawnPoint > EndPoint - MinItemSpacing) SpawnPoint = EndPoint - MinItemSpacing;
            }
            if (Raylib.IsKeyDown(KeyboardKey.Kp4))
            {
                SpawnPoint -= adjustSpeed;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.Kp9))
            {
                EndPoint += adjustSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.Kp6))
            {
                EndPoint -= adjustSpeed;
                if (EndPoint < SpawnPoint + MinItemSpacing) EndPoint = SpawnPoint + MinItemSpacing;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.Kp8))
            {
                MinItemSpacing += adjustSpeed;
                if (MinItemSpacing > 1.0f) MinItemSpacing = 1.0f;
            }
            if (Raylib.IsKeyDown(KeyboardKey.Kp5))
            {
                MinItemSpacing -= adjustSpeed;
                if (MinItemSpacing < 0.05f) MinItemSpacing = 0.05f;
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.Kp0))
            {
                SpawnPoint = -0.5f;
                EndPoint = 1.5f;
                MinItemSpacing = 0.33f;
            }
        }
        
        private bool IsNearMouse()
        {
            Ray mouseRay = Raylib.GetScreenToWorldRay(Raylib.GetMousePosition(), Data.GlobalData.camera);
            BoundingBox beltBox = new BoundingBox(
                Position - new Vector3(0.5f, 0.5f, 0.5f),
                Position + new Vector3(0.5f, 1.5f, 0.5f)
            );
            RayCollision collision = Raylib.GetRayCollisionBox(mouseRay, beltBox);
            return collision.Hit;
        }
        
        private void UpdateItems(float deltaTime)
        {
            if (items.Count == 0) return;
            
            float deltaProgress = BeltSpeed * deltaTime;
            items.Sort((a, b) => a.Progress.CompareTo(b.Progress));
            
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ConveyorItem item = items[i];
                float newProgress = item.Progress + deltaProgress;
                
                if (i < items.Count - 1)
                {
                    ConveyorItem itemAhead = items[i + 1];
                    float maxAllowedProgress = itemAhead.Progress - MinItemSpacing;
                    
                    if (newProgress > maxAllowedProgress)
                    {
                        newProgress = maxAllowedProgress;
                    }
                }
                
                item.Progress = newProgress;
                
                if (item.Progress >= EndPoint)
                {
                    if (TryTransferToOutput(item))
                    {
                        items.RemoveAt(i);
                    }
                    else
                    {
                        item.Progress = EndPoint;
                    }
                }
            }
        }
        
        protected override void Process()
        {
            TryPickupFromInput();
        }
        
        public bool AddItem(string resourceType, int amount)
        {
            if (items.Count >= MaxItemsOnBelt)
                return false;
            
            float startProgress = SpawnPoint;
            
            if (items.Count > 0)
            {
                float minProgress = EndPoint;
                foreach (var item in items)
                {
                    if (item.Progress < minProgress)
                        minProgress = item.Progress;
                }
                
                if (minProgress < startProgress + MinItemSpacing)
                    return false;
            }
            
            var newItem = new ConveyorItem(resourceType, amount);
            newItem.Progress = startProgress;
            items.Add(newItem);
            
            return true;
        }
        
        private void TryPickupFromInput()
        {
            if (InputMachine == null) return;
            if (items.Count >= MaxItemsOnBelt) return;
            
            if (InputMachine is MiningMachine miner && miner.OutputBuffer > 0)
            {
                int taken = miner.TakeOutput(1);
                if (taken > 0)
                    AddItem(miner.ResourceType, taken);
            }
            else if (InputMachine is FurnaceMachine furnace && furnace.OutputBuffer > 0)
            {
                int taken = furnace.TakeOutput(1);
                if (taken > 0)
                    AddItem(furnace.OutputResource, taken);
            }
            else if (InputMachine is ConveyorBelt inputBelt && inputBelt.items.Count > 0)
            {
                var lastItem = inputBelt.items[inputBelt.items.Count - 1];
                if (lastItem.Progress >= inputBelt.EndPoint - 0.05f)
                {
                    if (AddItem(lastItem.ResourceType, lastItem.Amount))
                        inputBelt.items.RemoveAt(inputBelt.items.Count - 1);
                }
            }
        }
        
        private bool TryTransferToOutput(ConveyorItem item)
        {
            if (OutputMachine == null)
                return false;
            
            if (OutputMachine is FurnaceMachine furnace)
                return furnace.AddInput(item.ResourceType, item.Amount);
            
            if (OutputMachine is ConveyorBelt outputBelt)
                return outputBelt.AddItem(item.ResourceType, item.Amount);
            
            return false;
        }
        
        public override void Draw()
        {
            DrawBeltModel();
            DrawItems();
            
            if (Data.GlobalData.ShowDebugInfo)
                DrawConnections();
            
            base.Draw();
        }
        
        // ===== MODEL ZEICHNEN =====
        private void DrawBeltModel()
        {
            float rotationAngle = CalculateRotationAngle();
            Vector3 rotationAxis = new Vector3(0, 1, 0);
            
            // Debug
            if (Data.GlobalData.ShowDebugInfo && IsNearMouse())
            {
                Vector3 arrowStart = Position;
                arrowStart.Y += ItemHeight + 1.5f;
                Vector3 arrowEnd = arrowStart + Direction * 0.5f;
                
                Raylib.DrawLine3D(arrowStart, arrowEnd, Color.Orange);
                Raylib.DrawSphere(arrowEnd, 0.05f, Color.Orange);
                
                // Für Kurven: Zeige auch Secondary Direction
                if (Type == BeltType.CurveLeft || Type == BeltType.CurveRight)
                {
                    Vector3 secondaryArrowEnd = arrowStart + SecondaryDirection * 0.5f;
                    Raylib.DrawLine3D(arrowStart, secondaryArrowEnd, Color.Purple);
                    Raylib.DrawSphere(secondaryArrowEnd, 0.05f, Color.Purple);
                }
                
                Vector2 screenPos = Raylib.GetWorldToScreen(arrowStart, Data.GlobalData.camera);
                Raylib.DrawText($"Type: {Type}", (int)screenPos.X - 30, (int)screenPos.Y - 75, 12, Color.Orange);
                Raylib.DrawText($"Rot: {rotationAngle:F0}°", (int)screenPos.X - 30, (int)screenPos.Y - 60, 12, Color.Orange);
            }
            
            // Zeichne Model OHNE Farbänderung
            Raylib.DrawModelEx(Model, Position, rotationAxis, rotationAngle, Vector3.One, Color.White);
        }
        
        private float CalculateRotationAngle()
        {
            float baseAngle = (float)(Math.Atan2(Direction.X, Direction.Z) * (180.0 / Math.PI));
            float modelOffset = 90f;
            return baseAngle + modelOffset;
        }
        
        // ===== ITEMS ZEICHNEN =====
        private void DrawItems()
        {
            foreach (var item in items)
            {
                Vector3 itemPosition = CalculateItemPosition(item.Progress);
                
                Resource? resource = ResourceRegistry.Get(item.ResourceType);
                Color itemColor = resource?.Color ?? Color.White;
                
                float itemSize = 0.2f;
                Raylib.DrawCube(itemPosition, itemSize, itemSize, itemSize, itemColor);
                Raylib.DrawCubeWires(itemPosition, itemSize, itemSize, itemSize, 
                    new Color(0, 0, 0, 100));
                
                Vector3 glowPos = itemPosition + new Vector3(0, itemSize * 0.3f, 0);
                Raylib.DrawSphere(glowPos, itemSize * 0.15f, new Color(255, 255, 255, 100));
            }
        }
        
        // ===== ITEM POSITION BERECHNEN (UNTERSCHIEDLICH JE NACH TYP!) =====
        protected virtual Vector3 CalculateItemPosition(float progress)
        {
            Vector3 itemPosition;
            
            switch (Type)
            {
                case BeltType.Straight:
                    itemPosition = CalculateStraightPosition(progress);
                    break;
                    
                case BeltType.CurveLeft:
                case BeltType.CurveRight:
                    itemPosition = CalculateCurvePosition(progress);
                    break;
                    
                case BeltType.RampUp:
                    itemPosition = CalculateRampPosition(progress, true);
                    break;
                    
                case BeltType.RampDown:
                    itemPosition = CalculateRampPosition(progress, false);
                    break;
                    
                default:
                    itemPosition = CalculateStraightPosition(progress);
                    break;
            }
            
            return itemPosition;
        }
        
        // ===== GERADE POSITION =====
        private Vector3 CalculateStraightPosition(float progress)
        {
            float normalizedPosition = (progress - 0.5f);
            Vector3 pos = Position + Direction * normalizedPosition * BeltLength;
            pos.Y += ItemHeight + 1;
            return pos;
        }
        
        // ===== KURVEN POSITION (90° ARC) =====
        private Vector3 CalculateCurvePosition(float progress)
        {
            // Progress 0.0 bis 1.0 wird zu 0° bis 90°
            float angle = progress * 90f * (float)(Math.PI / 180.0);
            
            // Kurvenradius
            float radius = BeltLength * 1f; // <-- Anpassen falls nötig 0.5
            
            // Berechne Position auf Kreisbogen
            Vector3 center = Position;
            
            float x, z;
            if (Type == BeltType.CurveLeft)
            {
                // Linke Kurve (gegen Uhrzeigersinn)
                x = radius * (float)Math.Sin(angle);
                z = radius * (1f - (float)Math.Cos(angle));
            }
            else
            {
                // Rechte Kurve (im Uhrzeigersinn)
                x = radius * (float)Math.Sin(angle);
                z = -radius * (1f - (float)Math.Cos(angle));
            }
            
            // Transformiere relativ zur Direction
            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 worldPos = TransformToWorld(localPos, center, Direction);
            worldPos.Y += ItemHeight + 1;
            
            return worldPos;
        }
        
        // ===== RAMPEN POSITION =====
        private Vector3 CalculateRampPosition(float progress, bool goingUp)
        {
            float normalizedPosition = (progress - 0.5f);
            Vector3 pos = Position + Direction * normalizedPosition * BeltLength;
            
            // Höhenänderung über die Länge des Belts
            float heightChange = 1.0f;  // 1 Block hoch/runter
            float currentHeight = progress * heightChange;
            
            if (!goingUp)
                currentHeight = heightChange - currentHeight;
            
            pos.Y += ItemHeight + 1 + currentHeight;
            return pos;
        }
        
        // ===== HELPER: TRANSFORMIERE LOKALE POSITION ZU WELT-KOORDINATEN =====
        private Vector3 TransformToWorld(Vector3 localPos, Vector3 center, Vector3 forward)
        {
            // Berechne Right-Vector (senkrecht zu Forward)
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);
            
            // Transformiere
            Vector3 worldPos = center + 
                               right * localPos.X + 
                               new Vector3(0, localPos.Y, 0) + 
                               forward * localPos.Z;
            
            return worldPos;
        }
        
        // ===== VERBINDUNGEN =====
        private void DrawConnections()
        {
            float sphereSize = 0.1f;
            
            if (InputMachine != null)
            {
                Vector3 inputPos = CalculateItemPosition(SpawnPoint);
                Raylib.DrawSphere(inputPos, sphereSize, new Color(0, 255, 0, 255));
            }
            
            if (OutputMachine != null)
            {
                Vector3 outputPos = CalculateItemPosition(EndPoint);
                Raylib.DrawSphere(outputPos, sphereSize, new Color(0, 100, 255, 255));
            }
            
            if (Data.GlobalData.ShowDebugInfo && IsNearMouse())
            {
                Vector2 screenPos = Raylib.GetWorldToScreen(Position + new Vector3(0, 2, 0), Data.GlobalData.camera);
                int y = (int)screenPos.Y;
                
                Raylib.DrawText("=== BELT CONTROLS ===", (int)screenPos.X - 80, y, 14, Color.Yellow);
                y += 20;
                Raylib.DrawText("Numpad 7/4: SpawnPoint", (int)screenPos.X - 80, y, 12, Color.White);
                y += 15;
                Raylib.DrawText("Numpad 9/6: EndPoint", (int)screenPos.X - 80, y, 12, Color.White);
                y += 15;
                Raylib.DrawText("Numpad 8/5: Spacing", (int)screenPos.X - 80, y, 12, Color.White);
                y += 15;
                Raylib.DrawText($"Type: {Type}", (int)screenPos.X - 80, y, 12, Color.SkyBlue);
            }
        }
        
        public override string GetDebugInfo()
        {
            string info = base.GetDebugInfo();
            info += $" | Type: {Type}";
            info += $" | Items: {items.Count}/{MaxItemsOnBelt}";
            info += $" | Speed: {BeltSpeed:F1}";
            
            if (InputMachine != null)
                info += $" | In";
            if (OutputMachine != null)
                info += $" | Out";
            
            return info;
        }
        
        // ===== PROVIDER =====
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                // ===== GERADES BELT =====
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Straight)",
                    "Resources/belt_straight.glb",
                    BeltType.Straight,
                    defaultModel
                ));
                
                // ===== KURVE LINKS =====
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Curve Left)",
                    "Resources/belt_curve_left.glb",
                    BeltType.CurveLeft,
                    defaultModel
                ));
                
                // ===== KURVE RECHTS =====
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Curve Right)",
                    "Resources/belt_curve_right.glb",
                    BeltType.CurveRight,
                    defaultModel
                ));
                
                // ===== RAMPE HOCH =====
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Ramp Up)",
                    "Resources/belt_ramp_up.glb",
                    BeltType.RampUp,
                    defaultModel
                ));
                
                // ===== RAMPE RUNTER =====
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Ramp Down)",
                    "Resources/belt_ramp_down.glb",
                    BeltType.RampDown,
                    defaultModel
                ));
                
                return definitions;
            }
            
            // ===== HELPER: ERSTELLE BELT DEFINITION =====
            private MachineDefinition CreateBeltDefinition(
                string name, 
                string modelPath, 
                BeltType type, 
                Model fallbackModel)
            {
                Model model = LoadModelSafe(modelPath, fallbackModel);
                
                return new MachineDefinition
                {
                    Name = name,
                    MachineType = "ConveyorBelt",
                    Model = model,
                    PreviewColor = new Color(100, 100, 100, 128),
                    Size = new Vector3(1, 1, 1),
                    YOffset = 0.5f,
                    PowerConsumption = 2f,
                    CustomData = new Dictionary<string, object>
                    {
                        { "BeltType", type }
                    }
                };
            }
            
            // ===== HELPER: LADE MODEL SICHER =====
            private Model LoadModelSafe(string path, Model fallback)
            {
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        return Raylib.LoadModel(path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Belt] Fehler beim Laden von {path}: {ex.Message}");
                        return fallback;
                    }
                }
                
                Console.WriteLine($"[Belt] Model nicht gefunden: {path}, verwende Fallback");
                return fallback;
            }
        }
    }
    
    public static class MathHelper
    {
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0f, 1f);
        }
    }
}