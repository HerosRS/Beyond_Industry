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
        public float Progress { get; set; }  // 0.0 = Anfang, 1.0 = Ende
        
        public ConveyorItem(string resourceType, int amount)
        {
            ResourceType = resourceType;
            Amount = amount;
            Progress = 0.0f;
        }
    }
    
    public class ConveyorBelt : FactoryMachine
    {
        public FactoryMachine? InputMachine { get; set; }
        public FactoryMachine? OutputMachine { get; set; }
        
        public Vector3 Direction { get; private set; }
        public float BeltSpeed { get; set; } = 1.0f;
        public int MaxItemsOnBelt { get; set; } = 6;
        public float ItemHeight { get; set; } = 0.3f;
        
        // ===== OPTIMALE WERTE (VON DIR GETESTET) =====
        public float SpawnPoint { get; set; } = -0.5f;       // Items spawnen AUSSERHALB am Anfang
        public float EndPoint { get; set; } = 1.5f;          // Items enden AUSSERHALB am Ende
        public float MinItemSpacing { get; set; } = 0.33f;   // Organischer Abstand
        public float BeltLength { get; set; } = 1.0f;        // Physische Belt-Länge
        
        private List<ConveyorItem> items = new List<ConveyorItem>();
        private float updateAccumulator = 0f;
        private const float UPDATE_RATE = 1f / 60f;
        
        public ConveyorBelt(Vector3 position, Model model, Vector3 direction) 
            : base(position, model)
        {
            MachineType = "ConveyorBelt";
            Direction = Vector3.Normalize(direction);
            ProductionCycleTime = 0.3f;
            PowerConsumption = 2f;
            InputMachine = null;
            OutputMachine = null;
        }
        
        protected override bool HasPower()
        {
            return true;
        }
        
        public override void Update(float deltaTime)
        {
            IsRunning = IsManuallyEnabled && HasPower();
            
            // ===== LIVE DEBUG CONTROLS =====
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
        
        // ===== LIVE DEBUG INPUT =====
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
            
            // Numpad 0 - Reset zu optimalen Werten
            if (Raylib.IsKeyPressed(KeyboardKey.Kp0))
            {
                SpawnPoint = -0.5f;
                EndPoint = 1.5f;
                MinItemSpacing = 0.33f;
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
                
                // Collision mit Item davor
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
                
                // Item erreicht EndPoint
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
        
        // ===== ITEM HINZUFÜGEN AM SPAWNPOINT =====
        public bool AddItem(string resourceType, int amount)
        {
            if (items.Count >= MaxItemsOnBelt)
                return false;
            
            // Items starten am SpawnPoint
            float startProgress = SpawnPoint;
            
            if (items.Count > 0)
            {
                // Finde vorderstes Item
                float minProgress = EndPoint;
                foreach (var item in items)
                {
                    if (item.Progress < minProgress)
                        minProgress = item.Progress;
                }
                
                // Ist genug Platz für neues Item?
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
                // Transfer wenn Item am EndPoint ist
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
            DrawBeltWithRotation();
            DrawItems();
            
            if (Data.GlobalData.ShowDebugInfo)
                DrawConnections();
            
            base.Draw();
        }
        
        private void DrawBeltWithRotation()
        {
            Color beltColor = IsRunning ? new Color(50, 50, 50, 255) : Color.Gray;
            if (items.Count > 0 && IsRunning)
                beltColor = new Color(70, 70, 70, 255);
            
            float rotationAngle = 0f;
            Vector3 rotationAxis = new Vector3(0, 1, 0);
            
            if (Direction.X > 0.5f) rotationAngle = 0f;
            else if (Direction.X < -0.5f) rotationAngle = 180f;
            else if (Direction.Z > 0.5f) rotationAngle = 90f;
            else if (Direction.Z < -0.5f) rotationAngle = 270f;
            
            Raylib.DrawModelEx(Model, Position, rotationAxis, rotationAngle, Vector3.One, beltColor);
        }
        
        // ===== ITEMS ZEICHNEN MIT OPTIMALEN WERTEN =====
        private void DrawItems()
        {
            foreach (var item in items)
            {
                // Progress direkt auf Belt-Länge mappen
                // SpawnPoint -0.5 bedeutet Items starten VOR dem Belt
                // EndPoint 1.5 bedeutet Items enden NACH dem Belt
                float normalizedPosition = (item.Progress - 0.5f);
                Vector3 itemPosition = Position + Direction * normalizedPosition * BeltLength;
                itemPosition.Y += ItemHeight + 1;
                
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
        
        // ===== VERBINDUNGEN + DEBUG VISUALISIERUNG =====
        private void DrawConnections()
        {
            float sphereSize = 0.1f;
            
            // Input am SpawnPoint (GRÜN)
            if (InputMachine != null)
            {
                float spawnPos = (SpawnPoint - 0.5f) * BeltLength;
                Vector3 inputPos = Position + Direction * spawnPos;
                inputPos.Y += ItemHeight + 1;
                Raylib.DrawSphere(inputPos, sphereSize, new Color(0, 255, 0, 255));
                
                // Label
                Vector2 screenPos = Raylib.GetWorldToScreen(inputPos, Data.GlobalData.camera);
                Raylib.DrawText($"SPAWN", (int)screenPos.X - 25, (int)screenPos.Y - 30, 12, Color.Green);
            }
            
            // Output am EndPoint (BLAU)
            if (OutputMachine != null)
            {
                float endPos = (EndPoint - 0.5f) * BeltLength;
                Vector3 outputPos = Position + Direction * endPos;
                outputPos.Y += ItemHeight + 1;
                Raylib.DrawSphere(outputPos, sphereSize, new Color(0, 100, 255, 255));
                
                // Label
                Vector2 screenPos = Raylib.GetWorldToScreen(outputPos, Data.GlobalData.camera);
                Raylib.DrawText($"END", (int)screenPos.X - 15, (int)screenPos.Y - 30, 12, Color.Blue);
            }
            
            // Debug: Belt-Grenzen
            if (Data.GlobalData.ShowDebugInfo)
            {
                // Absoluter Start (GELB)
                Vector3 startPos = Position + Direction * (-0.5f * BeltLength);
                startPos.Y += ItemHeight + 1.2f;
                Raylib.DrawSphere(startPos, 0.05f, Color.Yellow);
                
                // Absolutes Ende (ROT)
                Vector3 endPosAbs = Position + Direction * (0.5f * BeltLength);
                endPosAbs.Y += ItemHeight + 1.2f;
                Raylib.DrawSphere(endPosAbs, 0.05f, Color.Red);
                
                // SpawnPoint (HELLGRÜN)
                float spawnPosNorm = (SpawnPoint - 0.5f) * BeltLength;
                Vector3 spawnPosVec = Position + Direction * spawnPosNorm;
                spawnPosVec.Y += ItemHeight + 1.3f;
                Raylib.DrawSphere(spawnPosVec, 0.06f, Color.Lime);
                
                // EndPoint (HELLBLAU)
                float endPosNorm = (EndPoint - 0.5f) * BeltLength;
                Vector3 endPosVec = Position + Direction * endPosNorm;
                endPosVec.Y += ItemHeight + 1.3f;
                Raylib.DrawSphere(endPosVec, 0.06f, Color.SkyBlue);
                
                // Zeige Controls wenn Belt in Maus-Nähe
                if (IsNearMouse())
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
                    Raylib.DrawText("Numpad 0: Reset", (int)screenPos.X - 80, y, 12, Color.White);
                    y += 20;
                    Raylib.DrawText($"Current Values:", (int)screenPos.X - 80, y, 12, Color.Yellow);
                    y += 15;
                    Raylib.DrawText($"Spawn: {SpawnPoint:F2}", (int)screenPos.X - 80, y, 12, Color.Lime);
                    y += 15;
                    Raylib.DrawText($"End: {EndPoint:F2}", (int)screenPos.X - 80, y, 12, Color.SkyBlue);
                    y += 15;
                    Raylib.DrawText($"Spacing: {MinItemSpacing:F2}", (int)screenPos.X - 80, y, 12, Color.White);
                }
            }
        }
        
        public override string GetDebugInfo()
        {
            string info = base.GetDebugInfo();
            info += $" | Items: {items.Count}/{MaxItemsOnBelt}";
            info += $" | Speed: {BeltSpeed:F1}";
            info += $" | S:{SpawnPoint:F1}→E:{EndPoint:F1}";
            info += $" | Gap:{MinItemSpacing:F2}";
            
            if (InputMachine != null)
                info += $" | In: {InputMachine.MachineType}";
            if (OutputMachine != null)
                info += $" | Out: {OutputMachine.MachineType}";
            
            if (items.Count > 0)
            {
                var firstItem = items[0];
                string displayName = ResourceRegistry.GetDisplayName(firstItem.ResourceType);
                info += $" | [{displayName}] P:{firstItem.Progress:F2}";
            }
            
            return info;
        }
        
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                var beltDef = new MachineDefinition
                {
                    Name = "Conveyor Belt",
                    MachineType = "ConveyorBelt",
                    Model = defaultModel,
                    PreviewColor = new Color(100, 100, 100, 128),
                    Size = new Vector3(1, 1, 1),
                    YOffset = 0.5f,
                    PowerConsumption = 2f
                };
                
                definitions.Add(beltDef);
                
                return definitions;
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