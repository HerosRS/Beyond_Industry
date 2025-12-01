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
        
        // ===== ANPASSBARE EINSTELLUNGEN =====
        public float MinItemSpacing { get; set; } = 0.2f;
        public float BeltLength { get; set; } = 1.0f;           // Logische Länge für Berechnungen
        public float RenderLength { get; set; } = 1f;        // Physische Länge für Rendering (bis Rand)
        
        private List<ConveyorItem> items = new List<ConveyorItem>();
        private float updateAccumulator = 0f;
        private const float UPDATE_RATE = 1f / 60f;
        
        public ConveyorBelt(Vector3 position, Model model, Vector3 direction) 
            : base(position, model)
        {
            MachineType = "ConveyorBelt";
            Direction = Vector3.Normalize(direction);
            ProductionCycleTime = 0.5f;
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
        
        private void UpdateItems(float deltaTime)
        {
            if (items.Count == 0) return;
            
            float deltaProgress = BeltSpeed * deltaTime / BeltLength;
            
            items.Sort((a, b) => a.Progress.CompareTo(b.Progress));
            
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ConveyorItem item = items[i];
                float newProgress = item.Progress + deltaProgress;
                
                // Collision
                if (i < items.Count - 1)
                {
                    ConveyorItem itemAhead = items[i + 1];
                    float normalizedSpacing = MinItemSpacing / BeltLength;
                    float maxAllowedProgress = itemAhead.Progress - normalizedSpacing;
                    
                    if (newProgress > maxAllowedProgress)
                    {
                        newProgress = maxAllowedProgress;
                    }
                }
                
                item.Progress = newProgress;
                
                // Item am Ende (Progress >= 1.0)
                if (item.Progress >= 0.99f)
                {
                    if (TryTransferToOutput(item))
                    {
                        items.RemoveAt(i);
                    }
                    else
                    {
                        item.Progress = 1.0f;
                    }
                }
            }
        }
        
        protected override void Process()
        {
            TryPickupFromInput();
        }
        
        // ===== ITEM HINZUFÜGEN - DIREKT AM ANFANG =====
        public bool AddItem(string resourceType, int amount)
        {
            if (items.Count >= MaxItemsOnBelt)
                return false;
            
            // Neues Item startet bei Progress 0.0 (direkt am Anfang)
            float startProgress = 0.0f;
            
            if (items.Count > 0)
            {
                // Finde vorderstes Item
                float minProgress = 1.0f;
                foreach (var item in items)
                {
                    if (item.Progress < minProgress)
                        minProgress = item.Progress;
                }
                
                // Normalisiere Spacing
                float normalizedSpacing = MinItemSpacing / BeltLength;
                
                // Prüfe ob genug Platz ist
                if (minProgress < normalizedSpacing)
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
                if (lastItem.Progress >= 0.95f)
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
        
        // ===== ITEMS ZEICHNEN - BIS ZUM RAND =====
        private void DrawItems()
        {
            foreach (var item in items)
            {
                // Progress 0.0 = linke Kante, 1.0 = rechte Kante
                // Nutze RenderLength für physische Position
                float relativePosition = (item.Progress - 0.2f) * RenderLength;
                Vector3 itemPosition = Position + Direction * relativePosition;
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
        
        // ===== VERBINDUNGEN AN DEN KANTEN =====
        private void DrawConnections()
        {
            float sphereSize = 0.1f;
            
            // Input an der vorderen physischen Kante (nutzt RenderLength)
            if (InputMachine != null)
            {
                Vector3 inputPos = Position + Direction * (-1f * RenderLength);
                inputPos.Y += ItemHeight + 1;
                Raylib.DrawSphere(inputPos, sphereSize, new Color(0, 255, 0, 200));
            }
            
            // Output an der hinteren physischen Kante (nutzt RenderLength)
            if (OutputMachine != null)
            {
                Vector3 outputPos = Position + Direction * (1f * RenderLength);
                outputPos.Y += ItemHeight + 1;
                Raylib.DrawSphere(outputPos, sphereSize, new Color(0, 100, 255, 200));
            }
            
            // Debug: Zeige Belt-Grenzen
            if (Data.GlobalData.ShowDebugInfo)
            {
                // Logische Grenzen (gelb/rot) - für Debugging
                Vector3 startPos = Position + Direction * (-1f * BeltLength);
                startPos.Y += ItemHeight + 1f;
                Raylib.DrawSphere(startPos, 0.05f, Color.Yellow);
                
                Vector3 endPos = Position + Direction * (1f * BeltLength);
                endPos.Y += ItemHeight + 1f;
                Raylib.DrawSphere(endPos, 0.05f, Color.Red);
                
                // Physische Grenzen (cyan/magenta) - wo Items tatsächlich sind
                Vector3 renderStart = Position + Direction * (-1f * RenderLength);
                renderStart.Y += ItemHeight + 1f;
                Raylib.DrawSphere(renderStart, 0.03f, Color.DarkBlue);
                
                Vector3 renderEnd = Position + Direction * (1f * RenderLength);
                renderEnd.Y += ItemHeight + 1f;
                Raylib.DrawSphere(renderEnd, 0.03f, Color.Magenta);
                
                // Mitte
                Vector3 midPos = Position;
                midPos.Y += ItemHeight + 1f;
                Raylib.DrawSphere(midPos, 0.03f, Color.White);
            }
        }
        
        public override string GetDebugInfo()
        {
            string info = base.GetDebugInfo();
            info += $" | Items: {items.Count}/{MaxItemsOnBelt}";
            info += $" | Speed: {BeltSpeed:F1}";
            info += $" | Spacing: {MinItemSpacing:F2}";
            info += $" | Render: {RenderLength:F2}";
            
            if (InputMachine != null)
                info += $" | In: {InputMachine.MachineType}";
            if (OutputMachine != null)
                info += $" | Out: {OutputMachine.MachineType}";
            
            if (items.Count > 0)
            {
                var firstItem = items[0];
                string displayName = ResourceRegistry.GetDisplayName(firstItem.ResourceType);
                info += $" | [{displayName}] Pos:{firstItem.Progress:F2}";
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