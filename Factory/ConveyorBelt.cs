using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== ITEM AUF DEM FÖRDERBAND =====
    public class ConveyorItem
    {
        public string ResourceType;
        public float Progress;
        public int Amount;
        
        public ConveyorItem(string resourceType, int amount = 1)
        {
            ResourceType = resourceType;
            Amount = amount;
            Progress = 0f;
        }
    }
    
    // ===== FÖRDERBAND =====
    public class ConveyorBelt : FactoryMachine
    {
        public Vector3 Direction { get; private set; }
        public float Speed { get; private set; }
        public int MaxItems { get; private set; }
        private List<ConveyorItem> items;
        
        public FactoryMachine? InputMachine { get; set; }
        public FactoryMachine? OutputMachine { get; set; }
        
        public ConveyorBelt(Vector3 position, Model model, Vector3 direction) 
            : base(position, model)
        {
            MachineType = "Conveyor Belt";
            Direction = Vector3.Normalize(direction);
            Speed = 1.0f;
            MaxItems = 4;
            ProductionCycleTime = 0.5f;
            PowerConsumption = 2f;
            items = new List<ConveyorItem>();
        }
        
        public bool AddItem(string resourceType, int amount = 1)
        {
            if (items.Count >= MaxItems)
            {
                return false;
            }
            
            items.Add(new ConveyorItem(resourceType, amount));
            Console.WriteLine($"[Belt] Item hinzugefügt: {amount}x {resourceType}");
            return true;
        }
        
        protected override void Process()
        {
            if (!IsRunning)
            {
                Console.WriteLine($"[Belt @ {Position}] Nicht aktiv (kein Strom)");
                return;
            }
            
            Console.WriteLine($"[Belt @ {Position}] Process läuft - Items: {items.Count}, Input: {InputMachine?.MachineType ?? "none"}, Output: {OutputMachine?.MachineType ?? "none"}");
            
            float deltaProgress = Speed * ProductionCycleTime;
            List<ConveyorItem> itemsToRemove = new List<ConveyorItem>();
            
            foreach (var item in items)
            {
                item.Progress += deltaProgress;
                Console.WriteLine($"[Belt] Item {item.ResourceType} Progress: {item.Progress:F2}");
                
                if (item.Progress >= 1.0f)
                {
                    if (OutputMachine != null)
                    {
                        if (OutputMachine is FurnaceMachine furnace)
                        {
                            if (furnace.AddInput(item.ResourceType, item.Amount))
                            {
                                Console.WriteLine($"[Belt] Item übergeben an {OutputMachine.MachineType}: {item.Amount}x {item.ResourceType}");
                                itemsToRemove.Add(item);
                            }
                            else
                            {
                                Console.WriteLine($"[Belt] Furnace voll oder falscher Typ!");
                            }
                        }
                        else if (OutputMachine is ConveyorBelt nextBelt)
                        {
                            if (nextBelt.AddItem(item.ResourceType, item.Amount))
                            {
                                Console.WriteLine($"[Belt] Item weitergeleitet an nächstes Band");
                                itemsToRemove.Add(item);
                            }
                            else
                            {
                                Console.WriteLine($"[Belt] Nächstes Band voll!");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[Belt] Item am Ende: {item.Amount}x {item.ResourceType} (kein Output)");
                        itemsToRemove.Add(item);
                    }
                }
            }
            
            foreach (var item in itemsToRemove)
            {
                items.Remove(item);
            }
            
            // ===== ITEMS VON INPUT HOLEN =====
            if (InputMachine != null && items.Count < MaxItems)
            {
                Console.WriteLine($"[Belt] Versuche Items von {InputMachine.MachineType} zu holen...");
                
                if (InputMachine is MiningMachine miner)
                {
                    Console.WriteLine($"[Belt] Miner hat OutputBuffer: {miner.OutputBuffer}");
                    
                    if (miner.OutputBuffer > 0)
                    {
                        int taken = miner.TakeOutput(1);
                        if (taken > 0)
                        {
                            AddItem(miner.ResourceType, taken);
                            Console.WriteLine($"[Belt] Erfolgreich {taken}x {miner.ResourceType} von Miner genommen");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[Belt] Miner Buffer ist leer");
                    }
                }
                else if (InputMachine is FurnaceMachine furnace)
                {
                    Console.WriteLine($"[Belt] Furnace hat OutputBuffer: {furnace.OutputBuffer}");
                    
                    if (furnace.OutputBuffer > 0)
                    {
                        int taken = furnace.TakeOutput(1);
                        if (taken > 0)
                        {
                            AddItem(furnace.OutputResource, taken);
                            Console.WriteLine($"[Belt] Erfolgreich {taken}x {furnace.OutputResource} von Furnace genommen");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[Belt] Furnace Buffer ist leer");
                    }
                }
                else
                {
                    Console.WriteLine($"[Belt] Input ist weder Miner noch Furnace: {InputMachine.GetType().Name}");
                }
            }
            else
            {
                if (InputMachine == null)
                    Console.WriteLine($"[Belt @ {Position}] Kein Input verbunden!");
                if (items.Count >= MaxItems)
                    Console.WriteLine($"[Belt @ {Position}] Belt ist voll!");
            }
        }
        
        public override void Draw()
        {
            Color bandColor = IsRunning ? new Color(60, 60, 60, 255) : Color.Gray;
            Raylib.DrawModel(Model, Position, 1.0f, bandColor);
            
            Vector3 arrowStart = Position + new Vector3(0, 0.6f, 0);
            Vector3 arrowEnd = arrowStart + Direction * 0.4f;
            Raylib.DrawLine3D(arrowStart, arrowEnd, Color.Yellow);
            Raylib.DrawSphere(arrowEnd, 0.05f, Color.Yellow);
            
            foreach (var item in items)
            {
                Vector3 itemPos = Position + Direction * (item.Progress - 0.5f) * 0.8f;
                itemPos.Y += 0.7f;
                Color itemColor = GetResourceColor(item.ResourceType);
                Raylib.DrawCube(itemPos, 0.15f, 0.15f, 0.15f, itemColor);
            }
        }
        
        private Color GetResourceColor(string resourceType)
        {
            return resourceType switch
            {
                "Iron Ore" => new Color(139, 69, 19, 255),
                "Iron Plate" => new Color(192, 192, 192, 255),
                "Copper Ore" => new Color(184, 115, 51, 255),
                "Copper Plate" => new Color(255, 140, 0, 255),
                _ => Color.White
            };
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | Items: {items.Count}/{MaxItems} | Dir: {Direction}";
        }
        
        // ===== PROVIDER FÜR MASCHINEN-DEFINITIONEN =====
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                var def = new MachineDefinition
                {
                    Name = "Conveyor Belt →",
                    MachineType = "ConveyorBelt",
                    Model = defaultModel,
                    PreviewColor = new Color(60, 60, 60, 128),
                    Size = new Vector3(1, 1, 1),
                    YOffset = 0.5f,
                    PowerConsumption = 2f
                };
                
                def.CreateMachineFunc = (pos) => new ConveyorBelt(pos, def.Model, new Vector3(1, 0, 0))
                {
                    PowerConsumption = 2f
                };
                
                definitions.Add(def);
                
                return definitions;
            }
        }
    }
}