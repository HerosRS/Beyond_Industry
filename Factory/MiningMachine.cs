using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== BERGBAU-MASCHINE =====
    public class MiningMachine : FactoryMachine
    {
        public string ResourceType { get; private set; }
        public int OutputPerCycle { get; private set; }
        public int TotalExtracted { get; private set; }
        
        public MiningMachine(Vector3 position, Model model, string resourceType = "Iron Ore") 
            : base(position, model)
        {
            MachineType = "Mining Drill";
            ResourceType = resourceType;
            OutputPerCycle = 1;
            ProductionCycleTime = 2.0f;
            PowerConsumption = 10f;
            TotalExtracted = 0;
        }
        
        protected override void Process()
        {
            TotalExtracted += OutputPerCycle;
            Console.WriteLine($"[Mining Drill] Gefördert: {OutputPerCycle}x {ResourceType} (Gesamt: {TotalExtracted})");
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | Extracted: {TotalExtracted}x {ResourceType}";
        }
        
        // ===== PROVIDER FÜR MASCHINEN-DEFINITIONEN =====
        // Hier definierst du alle Varianten dieser Maschine!
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                // ===== IRON MINING DRILL =====
                definitions.Add(new MachineDefinition
                {
                    Name = "Iron Mining Drill",
                    MachineType = "MiningDrill",
                    Model = defaultModel,
                    PreviewColor = new Color(70, 130, 180, 128),    // Stahlblau
                    Size = new Vector3(1, 1, 1),
                    YOffset = 0.5f,
                    
                    OutputResource = "Iron Ore",
                    ProductionTime = 2.0f,
                    PowerConsumption = 10f,
                    
                    // Factory-Funktion
                    CreateMachineFunc = (pos) => new MiningMachine(pos, defaultModel, "Iron Ore")
                    {
                        ProductionCycleTime = 2.0f,
                        PowerConsumption = 10f
                    }
                });
                
                // ===== COPPER MINING DRILL =====
                definitions.Add(new MachineDefinition
                {
                    Name = "Copper Mining Drill",
                    MachineType = "MiningDrill",
                    Model = defaultModel,
                    PreviewColor = new Color(184, 115, 51, 128),    // Kupferfarbe
                    Size = new Vector3(1, 1, 1),
                    YOffset = 0.5f,
                    
                    OutputResource = "Copper Ore",
                    ProductionTime = 1.5f,
                    PowerConsumption = 12f,
                    
                    CreateMachineFunc = (pos) => new MiningMachine(pos, defaultModel, "Copper Ore")
                    {
                        ProductionCycleTime = 1.5f,
                        PowerConsumption = 12f
                    }
                });
                
                // ===== WEITERE MINING DRILLS HIER HINZUFÜGEN =====
                /*
                definitions.Add(new MachineDefinition
                {
                    Name = "Coal Mining Drill",
                    MachineType = "MiningDrill",
                    Model = defaultModel,
                    PreviewColor = new Color(50, 50, 50, 128),
                    OutputResource = "Coal",
                    ProductionTime = 1.0f,
                    PowerConsumption = 8f,
                    CreateMachineFunc = (pos) => new MiningMachine(pos, defaultModel, "Coal")
                });
                */
                
                return definitions;
            }
        }
    }
}