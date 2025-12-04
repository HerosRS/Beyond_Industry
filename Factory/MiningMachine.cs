using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory.Resources;
using BeyondIndustry.Data;

namespace BeyondIndustry.Factory
{
    public class MiningMachine : FactoryMachine
    {
        public string ResourceType { get; private set; }
        public int OutputPerCycle { get; private set; }
        public int TotalExtracted { get; private set; }
        
        public int OutputBuffer { get; set; }
        public int MaxBufferSize { get; set; }
        
        public MiningMachine(Vector3 position, Model model, string resourceType = "IronOre") 
            : base(position, model)
        {
            ResourceType = resourceType;
            
            if (resourceType == "IronOre")
                MachineType = "MiningDrill_Iron";
            else if (resourceType == "CopperOre")
                MachineType = "MiningDrill_Copper";
            else
                MachineType = "MiningDrill_" + resourceType;
            
            OutputPerCycle = 1;
            ProductionCycleTime = 2.0f;
            PowerConsumption = 10f;
            TotalExtracted = 0;
            OutputBuffer = 0;
            MaxBufferSize = 10;
        }
        
        // ===== IMPLEMENTIERE ABSTRACT METHODS =====
        public override void Update(float deltaTime)
        {
            IsRunning = IsManuallyEnabled && HasPower();
            
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
            data["ResourceType"] = ResourceType;
            data["OutputBuffer"] = OutputBuffer;
            data["TotalExtracted"] = TotalExtracted;
            return data;
        }
        
        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("OutputBuffer"))
                OutputBuffer = Convert.ToInt32(data["OutputBuffer"]);
            
            if (data.ContainsKey("TotalExtracted"))
                TotalExtracted = Convert.ToInt32(data["TotalExtracted"]);
        }
        
        public override void Draw()
        {
            Color drawColor = IsRunning && OutputBuffer < MaxBufferSize ? Color.Green : 
                            IsRunning ? Color.Yellow : 
                            Color.Gray;
            
            Raylib.DrawModel(Model, Position, 1.0f, drawColor);
            
            // Zeige Ressourcen-Farbe als Indikator
            if (OutputBuffer > 0)
            {
                Color resourceColor = ResourceRegistry.GetColor(ResourceType);
                Vector3 indicatorPos = Position + new Vector3(0, 1.5f, 0);
                Raylib.DrawCube(indicatorPos, 0.3f, 0.3f, 0.3f, resourceColor);
            }
            
            DrawButton(Data.GlobalData.camera);
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | Buffer: {OutputBuffer}/{MaxBufferSize} | Total: {TotalExtracted}x {ResourceType}";
        }
        
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                var ironDef = new MachineDefinition
                {
                    Name = "Iron Mining Drill",
                    MachineType = "MiningDrill_Iron",
                    Model = defaultModel,
                    PreviewColor = new Color(70, 130, 180, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    OutputResource = "IronOre",
                    ProductionTime = 2.0f,
                    PowerConsumption = 10f
                };
                
                ironDef.CreateMachineFunc = (pos) => new MiningMachine(pos, ironDef.Model, "IronOre")
                {
                    ProductionCycleTime = 2.0f,
                    PowerConsumption = 10f
                };
                
                definitions.Add(ironDef);
                
                var copperDef = new MachineDefinition
                {
                    Name = "Copper Mining Drill",
                    MachineType = "MiningDrill_Copper",
                    Model = defaultModel,
                    PreviewColor = new Color(184, 115, 51, 128),
                    Size = new Vector3(2, 2, 2),
                    YOffset = 0.5f,
                    OutputResource = "CopperOre",
                    ProductionTime = 1.5f,
                    PowerConsumption = 12f
                };
                
                copperDef.CreateMachineFunc = (pos) => new MiningMachine(pos, copperDef.Model, "CopperOre")
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