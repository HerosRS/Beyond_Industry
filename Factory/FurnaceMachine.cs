using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== SCHMELZOFEN-MASCHINE =====
    public class FurnaceMachine : FactoryMachine
    {
        public string InputResource { get; private set; }
        public string OutputResource { get; private set; }
        public int InputPerCycle { get; private set; }
        public int OutputPerCycle { get; private set; }
        public int InputBuffer { get; set; }
        public int OutputBuffer { get; private set; }
        public int MaxBufferSize { get; set; }
        public int TotalProcessed { get; private set; }
        
        public FurnaceMachine(Vector3 position, Model model, 
                             string inputResource = "Iron Ore", 
                             string outputResource = "Iron Plate") 
            : base(position, model)
        {
            MachineType = "Furnace";
            InputResource = inputResource;
            OutputResource = outputResource;
            InputPerCycle = 1;
            OutputPerCycle = 1;
            ProductionCycleTime = 3.0f;
            PowerConsumption = 15f;
            InputBuffer = 0;
            OutputBuffer = 0;
            MaxBufferSize = 10;
            TotalProcessed = 0;
        }
        
        public bool AddInput(string resourceType, int amount)
        {
            if (resourceType == InputResource && InputBuffer + amount <= MaxBufferSize)
            {
                InputBuffer += amount;
                Console.WriteLine($"[Furnace] Empfangen: {amount}x {resourceType} (Puffer: {InputBuffer}/{MaxBufferSize})");
                return true;
            }
            return false;
        }
        
        public int TakeOutput(int maxAmount)
        {
            int amountToTake = Math.Min(maxAmount, OutputBuffer);
            OutputBuffer -= amountToTake;
            return amountToTake;
        }
        
        protected override void Process()
        {
            if (InputBuffer >= InputPerCycle && OutputBuffer + OutputPerCycle <= MaxBufferSize)
            {
                InputBuffer -= InputPerCycle;
                OutputBuffer += OutputPerCycle;
                TotalProcessed += OutputPerCycle;
                Console.WriteLine($"[Furnace] {InputPerCycle}x {InputResource} → {OutputPerCycle}x {OutputResource}");
            }
        }
        
        public override void Draw()
        {
            Color drawColor = IsRunning && InputBuffer >= InputPerCycle ? Color.Orange : 
                             IsRunning ? Color.Yellow : 
                             Color.Gray;
            Raylib.DrawModel(Model, Position, 1.0f, drawColor);
            base.Draw();
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | In:{InputBuffer} Out:{OutputBuffer} | Total:{TotalProcessed}x {OutputResource}";
        }
        
        // ===== PROVIDER FÜR MASCHINEN-DEFINITIONEN =====
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                // ===== IRON FURNACE =====
                var ironDef = new MachineDefinition
                {
                    Name = "Iron Furnace",
                    MachineType = "Iron_Furnace",
                    Model = defaultModel,
                    PreviewColor = new Color(255, 140, 0, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    
                    InputResource = "Iron Ore",
                    OutputResource = "Iron Plate",
                    ProductionTime = 3.0f,
                    PowerConsumption = 15f,
                    BufferSize = 10
                };
                
                ironDef.CreateMachineFunc = (pos) => new FurnaceMachine(pos, ironDef.Model, "Iron Ore", "Iron Plate")
                {
                    ProductionCycleTime = 3.0f,
                    PowerConsumption = 15f,
                    MaxBufferSize = 10
                };
                
                definitions.Add(ironDef);
                
                // ===== COPPER FURNACE =====
                var copperDef = new MachineDefinition
                {
                    Name = "Copper Furnace",
                    MachineType = "Copper_Furnace",
                    Model = defaultModel,
                    PreviewColor = new Color(255, 100, 0, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    
                    InputResource = "Copper Ore",
                    OutputResource = "Copper Plate",
                    ProductionTime = 2.5f,
                    PowerConsumption = 15f,
                    BufferSize = 10
                };
                
                copperDef.CreateMachineFunc = (pos) => new FurnaceMachine(pos, copperDef.Model, "Copper Ore", "Copper Plate")
                {
                    ProductionCycleTime = 2.5f,
                    PowerConsumption = 15f,
                    MaxBufferSize = 10  // <- DAS HAT GEFEHLT!
                };
                
                definitions.Add(copperDef);
                
                return definitions;
            }
        }
    }
}