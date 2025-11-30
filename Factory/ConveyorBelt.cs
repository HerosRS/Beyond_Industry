using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== ITEM AUF DEM FÖRDERBAND =====
    public class ConveyorItem
    {
        public string ResourceType;         // Was wird transportiert
        public float Progress;              // Position auf dem Band (0.0 - 1.0)
        public int Amount;                  // Menge
        
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
        // Band-Eigenschaften
        public Vector3 Direction { get; private set; }      // In welche Richtung läuft das Band
        public float Speed { get; private set; }            // Wie schnell (Einheiten pro Sekunde)
        public int MaxItems { get; private set; }           // Max Items gleichzeitig auf dem Band
        
        // Items auf dem Band
        private List<ConveyorItem> items;
        
        // Verbindungen
        public FactoryMachine? InputMachine { get; set; }    // Von wo Items kommen
        public FactoryMachine? OutputMachine { get; set; }   // Wohin Items gehen
        
        // Konstruktor
        public ConveyorBelt(Vector3 position, Model model, Vector3 direction) 
            : base(position, model)
        {
            MachineType = "Conveyor Belt";
            Direction = Vector3.Normalize(direction);   // Normalisiere Richtung
            Speed = 1.0f;                               // 1 Einheit pro Sekunde
            MaxItems = 4;                               // Max 4 Items auf einem Band
            ProductionCycleTime = 0.5f;                 // Verarbeite alle 0.5 Sekunden
            PowerConsumption = 2f;                      // Braucht wenig Strom
            
            items = new List<ConveyorItem>();
        }
        
        // ===== ITEM HINZUFÜGEN =====
        // Versuche ein Item auf das Band zu legen
        public bool AddItem(string resourceType, int amount = 1)
        {
            if (items.Count >= MaxItems)
            {
                return false;  // Band ist voll
            }
            
            items.Add(new ConveyorItem(resourceType, amount));
            Console.WriteLine($"[Belt] Item hinzugefügt: {amount}x {resourceType}");
            return true;
        }
        
        // ===== VERARBEITUNG =====
        protected override void Process()
        {
            if (!IsRunning) return;
            
            // 1. Bewege alle Items vorwärts
            float deltaProgress = Speed * ProductionCycleTime;
            
            List<ConveyorItem> itemsToRemove = new List<ConveyorItem>();
            
            foreach (var item in items)
            {
                item.Progress += deltaProgress;
                
                // 2. Wenn Item am Ende angekommen
                if (item.Progress >= 1.0f)
                {
                    // Versuche Item an Output-Maschine zu übergeben
                    if (OutputMachine != null)
                    {
                        // Wenn Output eine Furnace ist
                        if (OutputMachine is FurnaceMachine furnace)
                        {
                            if (furnace.AddInput(item.ResourceType, item.Amount))
                            {
                                Console.WriteLine($"[Belt] Item übergeben an {OutputMachine.MachineType}: {item.Amount}x {item.ResourceType}");
                                itemsToRemove.Add(item);
                            }
                            // Wenn Furnace voll, bleibt Item am Ende des Bands
                        }
                        // Wenn Output ein anderes Band ist
                        else if (OutputMachine is ConveyorBelt nextBelt)
                        {
                            if (nextBelt.AddItem(item.ResourceType, item.Amount))
                            {
                                Console.WriteLine($"[Belt] Item weitergeleitet an nächstes Band");
                                itemsToRemove.Add(item);
                            }
                        }
                    }
                    else
                    {
                        // Kein Output - Item verschwindet (oder bleibt am Ende)
                        Console.WriteLine($"[Belt] Item am Ende: {item.Amount}x {item.ResourceType} (kein Output)");
                        itemsToRemove.Add(item);
                    }
                }
            }
            
            // 3. Entferne übergebene Items
            foreach (var item in itemsToRemove)
            {
                items.Remove(item);
            }
            
            // 4. Hole neue Items von Input-Maschine
            if (InputMachine != null && items.Count < MaxItems)
            {
                // Wenn Input eine Mining Machine ist
                if (InputMachine is MiningMachine miner && miner.TotalExtracted > 0)
                {
                    // Hole 1 Item (vereinfacht - später mit Output Buffer arbeiten)
                    AddItem(miner.ResourceType, 1);
                }
                // Wenn Input eine Furnace ist
                else if (InputMachine is FurnaceMachine furnace && furnace.OutputBuffer > 0)
                {
                    int taken = furnace.TakeOutput(1);
                    if (taken > 0)
                    {
                        AddItem(furnace.OutputResource, taken);
                    }
                }
            }
        }
        
        // ===== ZEICHNEN =====
        public override void Draw()
        {
            // Zeichne das Band-Modell
            Color bandColor = IsRunning ? new Color(60, 60, 60, 255) : Color.Gray;
            Raylib.DrawModel(Model, Position, 1.0f, bandColor);
            
            // Zeichne Richtungspfeil auf dem Band
            Vector3 arrowStart = Position + new Vector3(0, 0.6f, 0);
            Vector3 arrowEnd = arrowStart + Direction * 0.4f;
            Raylib.DrawLine3D(arrowStart, arrowEnd, Color.Yellow);
            
            // Zeichne kleine Kugel am Ende als Pfeilspitze
            Raylib.DrawSphere(arrowEnd, 0.05f, Color.Yellow);
            
            // Zeichne Items auf dem Band
            foreach (var item in items)
            {
                // Berechne Position des Items basierend auf Progress
                Vector3 itemPos = Position + Direction * (item.Progress - 0.5f) * 0.8f;
                itemPos.Y += 0.7f;  // Über dem Band
                
                // Item als kleine Box oder Kugel
                Color itemColor = GetResourceColor(item.ResourceType);
                Raylib.DrawCube(itemPos, 0.15f, 0.15f, 0.15f, itemColor);
            }
            
            // Kein Fortschrittsbalken nötig für Bänder
        }
        
        // ===== HILFSMETHODE - FARBE FÜR RESSOURCE =====
        private Color GetResourceColor(string resourceType)
        {
            return resourceType switch
            {
                "Iron Ore" => new Color(139, 69, 19, 255),      // Braun
                "Iron Plate" => new Color(192, 192, 192, 255),   // Silber
                "Copper Ore" => new Color(184, 115, 51, 255),    // Kupfer
                "Copper Plate" => new Color(255, 140, 0, 255),   // Orange
                _ => Color.White
            };
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | Items: {items.Count}/{MaxItems} | Dir: {Direction}";
        }
        
        // ===== PROVIDER FÜR MASCHINEN-DEFINITIONEN =====
        // WICHTIG: Diese Klasse muss INNERHALB von ConveyorBelt sein!
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                definitions.Add(new MachineDefinition
                {
                    Name = "Conveyor Belt →",
                    MachineType = "ConveyorBelt",
                    Model = defaultModel,
                    PreviewColor = new Color(60, 60, 60, 128),
                    Size = new Vector3(1, 1, 1),
                    YOffset = 0.3f,
                    
                    PowerConsumption = 2f,
                    
                    CreateMachineFunc = (pos) => new ConveyorBelt(pos, defaultModel, new Vector3(1, 0, 0))
                    {
                        PowerConsumption = 2f
                    }
                });
                
                return definitions;
            }
        }
    }  // <- ConveyorBelt Klasse endet HIER
}  // <- Namespace endet hier