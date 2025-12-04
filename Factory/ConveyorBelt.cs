using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory.Resources;
using BeyondIndustry.Data;

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
        public Vector3 SecondaryDirection { get; private set; }
        public float BeltSpeed { get; set; } = 1.0f;
        public int MaxItemsOnBelt { get; set; } = 6;
        public float ItemHeight { get; set; } = 0.3f;
        
        public float SpawnPoint { get; set; } = -0.5f;
        public float EndPoint { get; set; } = 1.5f;
        public float MinItemSpacing { get; set; } = 0.33f;
        public float BeltLength { get; set; } = 1.0f;
        
        public float CurveRadius { get; set; } = 0.5f;
        
        private List<ConveyorItem> items = new List<ConveyorItem>();
        private float updateAccumulator = 0f;
        private const float UPDATE_RATE = 1f / 60f;
        
        public ConveyorBelt(Vector3 position, Model model, Vector3 direction, BeltType type = BeltType.Straight) 
            : base(position, model)
        {
            MachineType = "ConveyorBelt";
            Type = type;
            Direction = Vector3.Normalize(direction);
            
            CalculateSecondaryDirection();
            
            ProductionCycleTime = 0.3f;
            PowerConsumption = 2f;
        }
        
        private void CalculateSecondaryDirection()
        {
            if (Type == BeltType.CurveLeft)
            {
                SecondaryDirection = new Vector3(-Direction.Z, 0, Direction.X);
            }
            else if (Type == BeltType.CurveRight)
            {
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
        
        // ===== LIVE DEBUG INPUT (ERWEITERT) =====
        private void HandleDebugInput()
        {
            if (!IsNearMouse()) return;
            
            float adjustSpeed = 0.01f;
            
            // Numpad 7/4 - SpawnPoint anpassen
            if (Raylib.IsKeyDown(KeyboardKey.Kp7))
            {
                SpawnPoint += adjustSpeed;
                if (SpawnPoint > EndPoint - MinItemSpacing) SpawnPoint = EndPoint - MinItemSpacing;
                Console.WriteLine($"[Belt] SpawnPoint: {SpawnPoint:F3}");
            }
            if (Raylib.IsKeyDown(KeyboardKey.Kp4))
            {
                SpawnPoint -= adjustSpeed;
                Console.WriteLine($"[Belt] SpawnPoint: {SpawnPoint:F3}");
            }
            
            // Numpad 9/6 - EndPoint anpassen
            if (Raylib.IsKeyDown(KeyboardKey.Kp9))
            {
                EndPoint += adjustSpeed;
                Console.WriteLine($"[Belt] EndPoint: {EndPoint:F3}");
            }
            if (Raylib.IsKeyDown(KeyboardKey.Kp6))
            {
                EndPoint -= adjustSpeed;
                if (EndPoint < SpawnPoint + MinItemSpacing) EndPoint = SpawnPoint + MinItemSpacing;
                Console.WriteLine($"[Belt] EndPoint: {EndPoint:F3}");
            }
            
            // Numpad 8/5 - MinItemSpacing anpassen
            if (Raylib.IsKeyDown(KeyboardKey.Kp8))
            {
                MinItemSpacing += adjustSpeed;
                if (MinItemSpacing > 1.0f) MinItemSpacing = 1.0f;
                Console.WriteLine($"[Belt] MinItemSpacing: {MinItemSpacing:F3}");
            }
            if (Raylib.IsKeyDown(KeyboardKey.Kp5))
            {
                MinItemSpacing -= adjustSpeed;
                if (MinItemSpacing < 0.05f) MinItemSpacing = 0.05f;
                Console.WriteLine($"[Belt] MinItemSpacing: {MinItemSpacing:F3}");
            }
            
            // ===== NEU: Numpad 1/2 - KURVEN-RADIUS anpassen (nur für Kurven) =====
            if (Type == BeltType.CurveLeft || Type == BeltType.CurveRight)
            {
                if (Raylib.IsKeyDown(KeyboardKey.Kp1))
                {
                    CurveRadius += adjustSpeed * 2;
                    if (CurveRadius > 2.0f) CurveRadius = 2.0f;
                    Console.WriteLine($"[Belt] CurveRadius: {CurveRadius:F3}");
                }
                if (Raylib.IsKeyDown(KeyboardKey.Kp2))
                {
                    CurveRadius -= adjustSpeed * 2;
                    if (CurveRadius < 0.1f) CurveRadius = 0.1f;
                    Console.WriteLine($"[Belt] CurveRadius: {CurveRadius:F3}");
                }
            }
            
            // Numpad 0 - Reset zu optimalen Werten
            if (Raylib.IsKeyPressed(KeyboardKey.Kp0))
            {
                SpawnPoint = -0.5f;
                EndPoint = 1.5f;
                MinItemSpacing = 0.33f;
                CurveRadius = 0.5f;
                Console.WriteLine($"[Belt] Reset zu optimalen Werten");
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
                {
                    if (AddItem(miner.ResourceType, taken))
                    {
                        Console.WriteLine($"[Belt] Picked up {taken}x {miner.ResourceType} from Miner");
                    }
                }
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
        
        public List<ConveyorItem> GetItems()
        {
            return new List<ConveyorItem>(items);
        }
        
        // ===== SAVEABLE OVERRIDE =====
        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["BeltType"] = Type.ToString();
            data["Direction"] = new Dictionary<string, float>
            {
                { "X", Direction.X },
                { "Y", Direction.Y },
                { "Z", Direction.Z }
            };
            data["BeltSpeed"] = BeltSpeed;
            data["CurveRadius"] = CurveRadius;
            
            // Items serialisieren
            var itemsList = new List<Dictionary<string, object>>();
            foreach (var item in items)
            {
                itemsList.Add(new Dictionary<string, object>
                {
                    { "ResourceType", item.ResourceType },
                    { "Amount", item.Amount },
                    { "Progress", item.Progress }
                });
            }
            data["Items"] = itemsList;
            
            return data;
        }
        
        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("BeltSpeed"))
                BeltSpeed = Convert.ToSingle(data["BeltSpeed"]);
            
            if (data.ContainsKey("CurveRadius"))
                CurveRadius = Convert.ToSingle(data["CurveRadius"]);
            
            // Items deserialisieren
            if (data.ContainsKey("Items") && data["Items"] is List<object> itemsList)
            {
                items.Clear();
                foreach (var itemObj in itemsList)
                {
                    if (itemObj is Dictionary<string, object> itemData)
                    {
                        var item = new ConveyorItem(
                            itemData["ResourceType"].ToString() ?? "",
                            Convert.ToInt32(itemData["Amount"])
                        );
                        item.Progress = Convert.ToSingle(itemData["Progress"]);
                        items.Add(item);
                    }
                }
            }
        }
        
        public override void Draw()
        {
            DrawBeltModel();
            DrawItems();
            
            if (Data.GlobalData.ShowDebugInfo)
                DrawConnections();
            
            DrawButton(Data.GlobalData.camera);
        }
        
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
            
            // Zeichne Model (jetzt mit richtigen Models!)
            Raylib.DrawModelEx(Model, Position, rotationAxis, rotationAngle, Vector3.One, Color.White);
            
            // ===== ZUSÄTZLICHE VISUELLE PFAD-MARKER FÜR KURVEN =====
            if ((Type == BeltType.CurveLeft || Type == BeltType.CurveRight) && Data.GlobalData.ShowDebugInfo)
            {
                DrawCurvePath();
            }
        }
        
        // ===== NEU: ZEICHNE KURVEN-PFAD (OPTIONAL, NUR IM DEBUG) =====
        private void DrawCurvePath()
        {
            int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;
                
                Vector3 pos1 = CalculateCurvePosition(t1);
                Vector3 pos2 = CalculateCurvePosition(t2);
                
                pos1.Y = Position.Y + 0.6f;  // Über dem Belt
                pos2.Y = Position.Y + 0.6f;
                
                Color pathColor = Type == BeltType.CurveLeft ? 
                    new Color(255, 100, 100, 180) : 
                    new Color(100, 255, 100, 180);
                
                Raylib.DrawLine3D(pos1, pos2, pathColor);
                Raylib.DrawSphere(pos1, 0.03f, pathColor);
            }
        }
        
        private float CalculateRotationAngle()
        {
            float baseAngle = (float)(Math.Atan2(Direction.X, Direction.Z) * (180.0 / Math.PI));
            float modelOffset = 90f;
            return baseAngle + modelOffset;
        }
        
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
        
        private Vector3 CalculateStraightPosition(float progress)
        {
            float normalizedPosition = (progress - 0.5f);
            Vector3 pos = Position + Direction * normalizedPosition * BeltLength;
            pos.Y += ItemHeight + 1;
            return pos;
        }
        
        // ===== KURVEN POSITION (MIT EINSTELLBAREM RADIUS) =====
        private Vector3 CalculateCurvePosition(float progress)
        {
            float angle = progress * 90f * (float)(Math.PI / 180.0);
            
            // NUTZE EINSTELLBAREN RADIUS!
            float radius = CurveRadius;
            
            Vector3 center = Position;
            
            float x, z;
            if (Type == BeltType.CurveLeft)
            {
                x = radius * (float)Math.Sin(angle);
                z = radius * (1f - (float)Math.Cos(angle));
            }
            else
            {
                x = radius * (float)Math.Sin(angle);
                z = -radius * (1f - (float)Math.Cos(angle));
            }
            
            Vector3 localPos = new Vector3(x, 0, z);
            Vector3 worldPos = TransformToWorld(localPos, center, Direction);
            worldPos.Y += ItemHeight + 1;
            
            return worldPos;
        }
        
        private Vector3 CalculateRampPosition(float progress, bool goingUp)
        {
            float normalizedPosition = (progress - 0.5f);
            Vector3 pos = Position + Direction * normalizedPosition * BeltLength;
            
            float heightChange = 1.0f;
            float currentHeight = progress * heightChange;
            
            if (!goingUp)
                currentHeight = heightChange - currentHeight;
            
            pos.Y += ItemHeight + 1 + currentHeight;
            return pos;
        }
        
        private Vector3 TransformToWorld(Vector3 localPos, Vector3 center, Vector3 forward)
        {
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);
            
            Vector3 worldPos = center + 
                               right * localPos.X + 
                               new Vector3(0, localPos.Y, 0) + 
                               forward * localPos.Z;
            
            return worldPos;
        }
        
        // ===== VERBINDUNGEN (ERWEITERT) =====
        private void DrawConnections()
        {
            // ===== VERBINDUNGS-LINIEN ZEICHNEN =====
            if (InputMachine != null)
            {
                Vector3 inputPos = CalculateItemPosition(SpawnPoint);
                Raylib.DrawLine3D(InputMachine.Position + new Vector3(0, 1, 0), inputPos, Color.Green);
                Raylib.DrawSphere(inputPos, 0.15f, Color.Green);
                
                if (Data.GlobalData.ShowDebugInfo)
                {
                    Vector2 screenPos = Raylib.GetWorldToScreen(inputPos, Data.GlobalData.camera);
                    Raylib.DrawText($"IN: {InputMachine.MachineType}", (int)screenPos.X - 50, (int)screenPos.Y, 12, Color.Green);
                }
            }
            
            if (OutputMachine != null)
            {
                Vector3 outputPos = CalculateItemPosition(EndPoint);
                Raylib.DrawLine3D(outputPos, OutputMachine.Position + new Vector3(0, 1, 0), Color.Blue);
                Raylib.DrawSphere(outputPos, 0.15f, Color.Blue);
                
                if (Data.GlobalData.ShowDebugInfo)
                {
                    Vector2 screenPos = Raylib.GetWorldToScreen(outputPos, Data.GlobalData.camera);
                    Raylib.DrawText($"OUT: {OutputMachine.MachineType}", (int)screenPos.X - 50, (int)screenPos.Y, 12, Color.Blue);
                }
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
                
                // ===== NEU: ZEIGE RADIUS-CONTROLS FÜR KURVEN =====
                if (Type == BeltType.CurveLeft || Type == BeltType.CurveRight)
                {
                    Raylib.DrawText("Numpad 1/2: Curve Radius", (int)screenPos.X - 80, y, 12, Color.SkyBlue);
                    y += 15;
                }
                
                Raylib.DrawText("Numpad 0: Reset", (int)screenPos.X - 80, y, 12, Color.White);
                y += 20;
                Raylib.DrawText($"Spawn: {SpawnPoint:F2}", (int)screenPos.X - 80, y, 12, Color.Lime);
                y += 15;
                Raylib.DrawText($"End: {EndPoint:F2}", (int)screenPos.X - 80, y, 12, Color.SkyBlue);
                y += 15;
                Raylib.DrawText($"Spacing: {MinItemSpacing:F2}", (int)screenPos.X - 80, y, 12, Color.White);
                y += 15;
                
                // ===== NEU: ZEIGE RADIUS FÜR KURVEN =====
                if (Type == BeltType.CurveLeft || Type == BeltType.CurveRight)
                {
                    Raylib.DrawText($"Radius: {CurveRadius:F2}", (int)screenPos.X - 80, y, 12, Color.SkyBlue);
                    y += 15;
                    
                    // ===== BONUS: VISUALISIERE DEN RADIUS =====
                    Vector3 centerPos = Position;
                    centerPos.Y += ItemHeight + 1.3f;
                    
                    Vector3 radiusEnd = centerPos + Direction * CurveRadius;
                    Raylib.DrawLine3D(centerPos, radiusEnd, Color.SkyBlue);
                    Raylib.DrawSphere(radiusEnd, 0.05f, Color.SkyBlue);
                }
            }
        }
        
        public override string GetDebugInfo()
        {
            string info = base.GetDebugInfo();
            info += $" | Type: {Type}";
            info += $" | Items: {items.Count}/{MaxItemsOnBelt}";
            info += $" | Speed: {BeltSpeed:F1}";
            
            // ===== NEU: ZEIGE RADIUS FÜR KURVEN =====
            if (Type == BeltType.CurveLeft || Type == BeltType.CurveRight)
            {
                info += $" | Radius: {CurveRadius:F2}";
            }
            
            if (InputMachine != null)
                info += $" | In: {InputMachine.MachineType}";
            if (OutputMachine != null)
                info += $" | Out: {OutputMachine.MachineType}";
            
            return info;
        }
        
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Straight)",
                    "Resources/Models/belt_straight.glb",
                    BeltType.Straight,
                    defaultModel
                ));
                
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Curve Left)",
                    "Resources/Models/belt_curve_left.glb",
                    BeltType.CurveLeft,
                    defaultModel
                ));
                
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Curve Right)",
                    "Resources/Models/belt_curve_right.glb",
                    BeltType.CurveRight,
                    defaultModel
                ));
                
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Ramp Up)",
                    "Resources/Models/belt_ramp_up.glb",
                    BeltType.RampUp,
                    defaultModel
                ));
                
                definitions.Add(CreateBeltDefinition(
                    "Conveyor Belt (Ramp Down)",
                    "Resources/Models/belt_ramp_down.glb",
                    BeltType.RampDown,
                    defaultModel
                ));
                
                return definitions;
            }
            
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