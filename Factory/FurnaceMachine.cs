using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory.Resources;
using BeyondIndustry.Data;

namespace BeyondIndustry.Factory
{
    public class FurnaceMachine : FactoryMachine
    {
        public string InputResource { get; private set; }
        public string OutputResource { get; private set; }
        public int InputPerCycle { get; private set; }
        public int OutputPerCycle { get; private set; }
        public int InputBuffer { get; set; }
        public int OutputBuffer { get; set; }
        public int MaxBufferSize { get; set; }
        public int TotalProcessed { get; private set; }
        
        public FurnaceMachine(Vector3 position, Model model, 
                             string inputResource = "IronOre", 
                             string outputResource = "IronPlate") 
            : base(position, model)
        {
            InputResource = inputResource;
            OutputResource = outputResource;
            MachineType = "Iron_Furnace";
            InputPerCycle = 1;
            OutputPerCycle = 1;
            ProductionCycleTime = 3.0f;
            PowerConsumption = 15f;
            InputBuffer = 0;
            OutputBuffer = 0;
            MaxBufferSize = 10;
            TotalProcessed = 0;
        }
        
        // ===== IMPLEMENTIERE ABSTRACT METHODS =====
        public override void Update(float deltaTime)
        {
            IsRunning = IsManuallyEnabled && HasPower() && InputBuffer >= InputPerCycle;
            
            if (!IsRunning) return;
            
            productionTimer += deltaTime;
            
            if (productionTimer >= ProductionCycleTime)
            {
                Process();
                productionTimer = 0f;
            }
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
        
        // ===== SAVEABLE OVERRIDE =====
        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["InputResource"] = InputResource;
            data["OutputResource"] = OutputResource;
            data["InputBuffer"] = InputBuffer;
            data["OutputBuffer"] = OutputBuffer;
            data["TotalProcessed"] = TotalProcessed;
            return data;
        }
        
        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("InputBuffer"))
                InputBuffer = Convert.ToInt32(data["InputBuffer"]);
            
            if (data.ContainsKey("OutputBuffer"))
                OutputBuffer = Convert.ToInt32(data["OutputBuffer"]);
            
            if (data.ContainsKey("TotalProcessed"))
                TotalProcessed = Convert.ToInt32(data["TotalProcessed"]);
        }
        
        public override void Draw()
        {
            Color drawColor = IsRunning && InputBuffer >= InputPerCycle ? Color.Orange : 
                            IsRunning ? Color.Yellow : 
                            Color.Gray;
            
            Raylib.DrawModel(Model, Position, 1.0f, drawColor);
            
            // Input-Anzeige (links)
            if (InputBuffer > 0)
            {
                Color inputColor = ResourceRegistry.GetColor(InputResource);
                Vector3 inputPos = Position + new Vector3(-0.7f, 1.0f, 0);
                Raylib.DrawCube(inputPos, 0.2f, 0.2f, 0.2f, inputColor);
            }
            
            // Output-Anzeige (rechts)
            if (OutputBuffer > 0)
            {
                Color outputColor = ResourceRegistry.GetColor(OutputResource);
                Vector3 outputPos = Position + new Vector3(0.7f, 1.0f, 0);
                Raylib.DrawCube(outputPos, 0.2f, 0.2f, 0.2f, outputColor);
            }
            
            DrawButton(Data.GlobalData.camera);
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | In:{InputBuffer} Out:{OutputBuffer} | Total:{TotalProcessed}x {OutputResource}";
        }
        
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                var ironDef = new MachineDefinition
                {
                    Name = "Iron Furnace",
                    MachineType = "Iron_Furnace",
                    Model = defaultModel,
                    PreviewColor = new Color(255, 140, 0, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    InputResource = "IronOre",
                    OutputResource = "IronPlate",
                    ProductionTime = 3.0f,
                    PowerConsumption = 15f,
                    BufferSize = 10
                };
                
                ironDef.CreateMachineFunc = (pos) => new FurnaceMachine(pos, ironDef.Model, "IronOre", "IronPlate")
                {
                    ProductionCycleTime = 3.0f,
                    PowerConsumption = 15f,
                    MaxBufferSize = 10
                };
                
                definitions.Add(ironDef);
                
                var copperDef = new MachineDefinition
                {
                    Name = "Copper Furnace",
                    MachineType = "Copper_Furnace",
                    Model = defaultModel,
                    PreviewColor = new Color(255, 100, 0, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    InputResource = "CopperOre",
                    OutputResource = "CopperPlate",
                    ProductionTime = 2.5f,
                    PowerConsumption = 15f,
                    BufferSize = 10
                };
                
                copperDef.CreateMachineFunc = (pos) => new FurnaceMachine(pos, copperDef.Model, "CopperOre", "CopperPlate")
                {
                    ProductionCycleTime = 2.5f,
                    PowerConsumption = 15f,
                    MaxBufferSize = 10
                };
                
                definitions.Add(copperDef);
                
                return definitions;
            }
        }
    }
}