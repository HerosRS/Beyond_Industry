using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Data;

namespace BeyondIndustry.Factory
{
    // ===== DEKOELEMENT: T-TRÄGER VERTIKAL =====
    public class T_Traeger_Gerade : FactoryMachine
    {
        public float Height { get; set; } = 1.0f;
        public Color TintColor { get; set; } = Color.White;
        
        public T_Traeger_Gerade(Vector3 position, Model model, float height = 3.0f) 
            : base(position, model)
        {
            MachineType = "T_Traeger_Gerade";
            Height = height;
            PowerConsumption = 0f;
            ProductionCycleTime = 0f;
        }
        
        // ===== IMPLEMENTIERE ABSTRACT METHODS =====
        public override void Update(float deltaTime)
        {
            IsRunning = true;
        }
        
        protected override void Process()
        {
            // Keine Produktion
        }
        
        // ===== SAVEABLE OVERRIDE =====
        public override Dictionary<string, object> Serialize()
        {
            var data = base.Serialize();
            data["Height"] = Height;
            return data;
        }
        
        public override void Deserialize(Dictionary<string, object> data)
        {
            base.Deserialize(data);
            
            if (data.ContainsKey("Height"))
                Height = Convert.ToSingle(data["Height"]);
        }
        
        public override void Draw()
        {
            Raylib.DrawModel(Model, Position, 1.0f, TintColor);
            
            DrawButton(Data.GlobalData.camera);
        }
        
        public override string GetDebugInfo()
        {
            return base.GetDebugInfo() + $" | Height: {Height:F1}m";
        }
        
        // ===== PROVIDER FÜR MASCHINEN-DEFINITIONEN =====
        public class Provider : IMachineProvider
        {
            public List<MachineDefinition> GetDefinitions(Model defaultModel)
            {
                var definitions = new List<MachineDefinition>();
                
                var beamDef = new MachineDefinition
                {
                    Name = "T-Träger Gerade",
                    MachineType = "T_Traeger_Gerade",
                    Model = defaultModel,
                    PreviewColor = new Color(80, 80, 80, 128),
                    Size = new Vector3(1, 2, 1),
                    YOffset = 0.5f,
                    PowerConsumption = 0f
                };
                
                beamDef.CreateMachineFunc = (pos) => new T_Traeger_Gerade(pos, beamDef.Model, 3.0f);
                definitions.Add(beamDef);
                
                return definitions;
            }
        }
    }
}