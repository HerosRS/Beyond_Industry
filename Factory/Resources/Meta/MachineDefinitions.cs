using System.Numerics;
using Raylib_cs;
using System.Collections.Generic;

namespace BeyondIndustry.Factory
{
    // ===== ZENTRALE MASCHINEN-DEFINITION =====
    public class MachineDefinition
    {
        // Visuelle Eigenschaften
        public string Name { get; set; } = "";
        public string MachineType { get; set; } = "";
        public Model Model { get; set; }
        public Color PreviewColor { get; set; }
        public Vector3 Size { get; set; }
        public float YOffset { get; set; }
        
        // Factory-Eigenschaften
        public string? InputResource { get; set; }
        public string? OutputResource { get; set; }
        public float ProductionTime { get; set; }
        public float PowerConsumption { get; set; }
        public int BufferSize { get; set; }
        
        // ===== FACTORY-METHODE FÜR ERSTELLUNG =====
        public System.Func<Vector3, FactoryMachine>? CreateMachineFunc { get; set; }
        
        public MachineDefinition()
        {
            Size = new Vector3(1, 1, 1);
            YOffset = 0.5f;
            PreviewColor = new Color(255, 255, 255, 128);
            ProductionTime = 1.0f;
            PowerConsumption = 10f;
            BufferSize = 10;
        }
        
        // ===== ERSTELLE MASCHINE =====
        public FactoryMachine? CreateMachine(Vector3 position)
        {
            if (CreateMachineFunc != null)
            {
                return CreateMachineFunc(position);
            }
            return null;
        }
    }
    
    // ===== MASCHINEN-REGISTRY MIT AUTO-REGISTRATION =====
    public static class MachineRegistry
    {
        // Liste aller registrierten Maschinen-Provider
        private static List<IMachineProvider> providers = new List<IMachineProvider>();
        
        // ===== REGISTRIERE EINE MASCHINEN-KLASSE =====
        public static void RegisterProvider(IMachineProvider provider)
        {
            providers.Add(provider);
        }
        
        // ===== LADE ALLE DEFINITIONEN VON ALLEN REGISTRIERTEN MASCHINEN =====
        public static List<MachineDefinition> LoadAllDefinitions(Dictionary<string, Model> models)
        {
            List<MachineDefinition> allDefinitions = new List<MachineDefinition>();
            
            foreach (var provider in providers)
            {
                // Hole Definitionen von diesem Provider
                List<MachineDefinition> definitions = provider.GetDefinitions(models.GetValueOrDefault("default"));
                
                // Weise Modelle zu basierend auf MachineType
                foreach (var def in definitions)
                {
                    // Versuche ein spezifisches Modell zu finden, sonst Default
                    if (models.ContainsKey(def.MachineType))
                    {
                        def.Model = models[def.MachineType];
                    }
                    else if (models.ContainsKey("default"))
                    {
                        def.Model = models["default"];
                    }
                }
                
                allDefinitions.AddRange(definitions);
            }
            
            return allDefinitions;
        }
        
        // ===== INITIALISIERE ALLE MASCHINEN =====
        // Diese Methode wird einmal beim Start aufgerufen
        public static void Initialize()
        {
            providers.Clear();
            
            // ===== HIER NUR DIE KLASSEN-NAMEN EINTRAGEN =====
            RegisterProvider(new MiningMachine.Provider());
            RegisterProvider(new FurnaceMachine.Provider());
            RegisterProvider(new ConveyorBelt.Provider());
            // Weitere Maschinen hier hinzufügen...
        }
    }
}