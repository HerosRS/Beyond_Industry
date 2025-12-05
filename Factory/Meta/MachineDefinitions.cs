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

        public bool CanBePlacedOn { get; set; } = true;         // Können andere darauf bauen?
        public bool RequiresFoundation { get; set; } = false;    // Braucht Unterbau?
        public List<string> AllowedBaseTypes { get; set; } = new List<string>(); // Welche Typen erlaubt als Base
        public float PlacementHeight { get; set; } = 1.0f;       // Höhe für nächste Ebene
        public bool IsFoundation { get; set; } = false;      
        
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
        List<MachineDefinition> definitions = provider.GetDefinitions(models.GetValueOrDefault("default"));
        
        foreach (var def in definitions)
        {
            // ===== HOLE MODEL AUS REGISTRY =====
            def.Model = ModelRegistry.GetModel(def.MachineType);
            
            // ===== SPEZIALFALL: BELT-TYPEN =====
            if (def.MachineType == "ConveyorBelt" && def.CustomData != null && def.CustomData.ContainsKey("BeltType"))
            {
                BeltType type = (BeltType)def.CustomData["BeltType"];
                string modelKey = $"ConveyorBelt_{type}";
                def.Model = ModelRegistry.GetModel(modelKey);
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
            RegisterProvider(new T_Traeger_Gerade.Provider());
            RegisterProvider(new T_Traeger_Ecke.Provider());
            // Weitere Maschinen hier hinzufügen...
            
            System.Console.WriteLine($"[MachineRegistry] {providers.Count} Provider registriert");
        }
    }
}