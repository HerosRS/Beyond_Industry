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
        
        // ===== NEU: CUSTOM DATA FÜR ZUSÄTZLICHE INFORMATIONEN =====
        public Dictionary<string, object>? CustomData { get; set; }
        
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
            CustomData = null;  // Optional
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
                
                // Weise Modelle zu basierend auf MachineType und CustomData
                foreach (var def in definitions)
                {
                    // ===== NEU: SPEZIELLE BEHANDLUNG FÜR BELT-TYPEN =====
                    if (def.MachineType == "ConveyorBelt" && def.CustomData != null)
                    {
                        if (def.CustomData.ContainsKey("BeltType"))
                        {
                            BeltType type = (BeltType)def.CustomData["BeltType"];
                            
                            // Versuche spezifisches Belt-Model zu laden
                            string modelKey = $"ConveyorBelt_{type}";
                            if (models.ContainsKey(modelKey))
                            {
                                def.Model = models[modelKey];
                                System.Console.WriteLine($"[MachineRegistry] Loaded model for {modelKey}");
                            }
                            else if (models.ContainsKey("ConveyorBelt"))
                            {
                                def.Model = models["ConveyorBelt"];
                            }
                            else if (models.ContainsKey("default"))
                            {
                                def.Model = models["default"];
                            }
                        }
                    }
                    else
                    {
                        // ===== STANDARD: VERSUCHE SPEZIFISCHES MODELL ZU FINDEN =====
                        // 1. Versuche mit vollständigem Namen
                        if (models.ContainsKey(def.Name))
                        {
                            def.Model = models[def.Name];
                        }
                        // 2. Versuche mit MachineType
                        else if (models.ContainsKey(def.MachineType))
                        {
                            def.Model = models[def.MachineType];
                        }
                        // 3. Fallback zu Default
                        else if (models.ContainsKey("default"))
                        {
                            def.Model = models["default"];
                        }
                    }
                }
                
                allDefinitions.AddRange(definitions);
            }
            
            System.Console.WriteLine($"[MachineRegistry] {allDefinitions.Count} Maschinen-Definitionen geladen");
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
            
            System.Console.WriteLine($"[MachineRegistry] {providers.Count} Provider registriert");
        }
    }
}