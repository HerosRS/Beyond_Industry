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
        
        // ===== NEU: Output Buffer =====
        public int OutputBuffer { get; private set; }
        public int MaxBufferSize { get; set; }
        
        public MiningMachine(Vector3 position, Model model, string resourceType = "Iron Ore") 
            : base(position, model)
        {
            MachineType = "Mining Drill";
            ResourceType = resourceType;
            OutputPerCycle = 1;
            ProductionCycleTime = 2.0f;
            PowerConsumption = 10f;
            TotalExtracted = 0;
            
            // NEU
            OutputBuffer = 0;
            MaxBufferSize = 10;  // Kann 10 Items speichern
        }
        
        protected override void Process()
        {
            // Nur produzieren wenn Buffer nicht voll
            if (OutputBuffer < MaxBufferSize)
            {
                OutputBuffer += OutputPerCycle;
                TotalExtracted += OutputPerCycle;
                Console.WriteLine($"[Mining Drill] Gefördert: {OutputPerCycle}x {ResourceType} (Buffer: {OutputBuffer}/{MaxBufferSize})");
            }
            else
            {
                Console.WriteLine($"[Mining Drill] Buffer voll! ({OutputBuffer}/{MaxBufferSize})");
            }
        }
        
        // ===== NEU: Methode zum Entnehmen =====
        public int TakeOutput(int maxAmount)
        {
            int amountToTake = Math.Min(maxAmount, OutputBuffer);
            OutputBuffer -= amountToTake;
            return amountToTake;
        }
        
        public override void Draw()
        {
            // Farbe basierend auf Buffer-Status
            Color drawColor = IsRunning && OutputBuffer < MaxBufferSize ? Color.Green : 
                             IsRunning && OutputBuffer >= MaxBufferSize ? Color.Yellow :  // Buffer voll
                             Color.Gray;
            Raylib.DrawModel(Model, Position, 1.0f, drawColor);
            base.Draw();  // Zeigt Fortschrittsbalken
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | Buffer: {OutputBuffer}/{MaxBufferSize} | Total: {TotalExtracted}x {ResourceType}";
        }
        
        // ===== PROVIDER FÜR MASCHINEN-DEFINITIONEN =====
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                // Iron Drill
                var ironDef = new MachineDefinition
                {
                    Name = "Iron Mining Drill",
                    MachineType = "MiningDrill_Iron",
                    Model = defaultModel,
                    PreviewColor = new Color(70, 130, 180, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    OutputResource = "Iron Ore",
                    ProductionTime = 2.0f,
                    PowerConsumption = 10f
                };
                
                ironDef.CreateMachineFunc = (pos) => new MiningMachine(pos, ironDef.Model, "Iron Ore")
                {
                    ProductionCycleTime = 2.0f,
                    PowerConsumption = 10f
                };
                
                definitions.Add(ironDef);
                
                // Copper Drill
                var copperDef = new MachineDefinition
                {
                    Name = "Copper Mining Drill",
                    MachineType = "MiningDrill_Copper",
                    Model = defaultModel,
                    PreviewColor = new Color(184, 115, 51, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    OutputResource = "Copper Ore",
                    ProductionTime = 1.5f,
                    PowerConsumption = 12f
                };
                
                copperDef.CreateMachineFunc = (pos) => new MiningMachine(pos, copperDef.Model, "Copper Ore")
                {
                    ProductionCycleTime = 1.5f,
                    PowerConsumption = 12f
                };
                
                definitions.Add(copperDef);
                
                return definitions;
            }
        }
    }
}