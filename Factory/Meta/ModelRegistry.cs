using System;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    /// <summary>
    /// Zentrale Verwaltung aller 3D-Modelle
    /// Hier werden alle Models definiert und geladen
    /// </summary>
    public static class ModelRegistry
    {
        private static Dictionary<string, Model> models = new Dictionary<string, Model>();
        
        /// <summary>
        /// Definiert alle zu ladenden Models
        /// HIER NEUE MODELS HINZUFÜGEN!
        /// </summary>
        private static readonly List<ModelDefinition> modelDefinitions = new List<ModelDefinition>
        {
            // ===== BASIS-MODELS =====
            new ModelDefinition("default", "", ModelType.Procedural),
            
            // ===== UMGEBUNG =====
            new ModelDefinition("Wand", @"..\..\..\Resources\Wand.obj"),
            new ModelDefinition("Boden", @"..\..\..\Resources\Boden.obj"),
            
            // ===== MASCHINEN =====
            new ModelDefinition("IronDrill", @"..\..\..\Resources\Maschiene.obj"),
            new ModelDefinition("CopperDrill", @"..\..\..\Resources\Maschiene.obj"),
            new ModelDefinition("Iron_Furnace", @"..\..\..\Resources\Iron_Furnace.obj"),
            
            // ===== FÖRDERBAND-SYSTEM =====
            new ModelDefinition("ConveyorBelt", @"..\..\..\Resources\Belt.obj"),
            new ModelDefinition("ConveyorBelt_Straight", @"..\..\..\Resources\Belt.obj"),
            new ModelDefinition("ConveyorBelt_CurveLeft", @"..\..\..\Resources\Belt.obj"),   // TODO: Eigenes Model
            new ModelDefinition("ConveyorBelt_CurveRight", @"..\..\..\Resources\Belt.obj"),  // TODO: Eigenes Model
            new ModelDefinition("ConveyorBelt_RampUp", @"..\..\..\Resources\Belt.obj"),
            new ModelDefinition("ConveyorBelt_RampDown", @"..\..\..\Resources\Belt.obj"),
            
            // ===== STRUKTUREN & DEKO =====
            new ModelDefinition("T_Traeger_Vertikal", @"..\..\..\Resources\T_Träger_Vertikal.obj"),
            
            // ===== NEUE MODELS HIER HINZUFÜGEN =====
            // new ModelDefinition("MeinNeuesModel", @"..\..\..\Resources\MeinModel.obj"),
        };
        
        /// <summary>
        /// Lädt alle definierten Models
        /// </summary>
        public static void LoadAllModels(Shader? shader = null)
        {
            Console.WriteLine("[ModelRegistry] Loading models...");
            
            Model defaultModel = Raylib.LoadModelFromMesh(Raylib.GenMeshCube(1.0f, 1.0f, 1.0f));
            models["default"] = defaultModel;
            
            foreach (var modelDef in modelDefinitions)
            {
                if (modelDef.Key == "default") continue; // Bereits geladen
                
                Model loadedModel = LoadModel(modelDef, defaultModel);
                models[modelDef.Key] = loadedModel;
                
                // Shader anwenden falls vorhanden
                if (shader.HasValue && shader.Value.Id != 0)
                {
                    unsafe
                    {
                        loadedModel.Materials[0].Shader = shader.Value;
                    }
                }
            }
            
            Console.WriteLine($"[ModelRegistry] ✓ {models.Count} models loaded");
        }
        
        private static Model LoadModel(ModelDefinition def, Model fallback)
        {
            if (def.Type == ModelType.Procedural)
            {
                return fallback;
            }
            
            if (System.IO.File.Exists(def.Path))
            {
                try
                {
                    Model model = Raylib.LoadModel(def.Path);
                    Console.WriteLine($"[ModelRegistry] ✓ Loaded: {def.Key}");
                    return model;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModelRegistry] ✗ Error loading {def.Key}: {ex.Message}");
                    return fallback;
                }
            }
            else
            {
                Console.WriteLine($"[ModelRegistry] ⚠️ Not found: {def.Key} ({def.Path})");
                return fallback;
            }
        }
        
        /// <summary>
        /// Gibt ein geladenes Model zurück
        /// </summary>
        public static Model GetModel(string key)
        {
            if (models.ContainsKey(key))
            {
                return models[key];
            }
            
            Console.WriteLine($"[ModelRegistry] ⚠️ Model '{key}' not found, using default");
            return models.GetValueOrDefault("default");
        }
        
        /// <summary>
        /// Gibt alle Models als Dictionary zurück (für Kompatibilität)
        /// </summary>
        public static Dictionary<string, Model> GetAllModels()
        {
            return new Dictionary<string, Model>(models);
        }
        
        /// <summary>
        /// Entlädt alle Models
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var model in models.Values)
            {
                Raylib.UnloadModel(model);
            }
            models.Clear();
            Console.WriteLine("[ModelRegistry] All models unloaded");
        }
    }
    
    // ===== HELPER-KLASSEN =====
    
    public enum ModelType
    {
        File,         // Aus Datei laden
        Procedural    // Prozedural generiert (Cube, Sphere, etc.)
    }
    
    public class ModelDefinition
    {
        public string Key { get; set; }
        public string Path { get; set; }
        public ModelType Type { get; set; }
        
        public ModelDefinition(string key, string path, ModelType type = ModelType.File)
        {
            Key = key;
            Path = path;
            Type = type;
        }
    }
}