using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Factory;

namespace BeyondIndustry.Data
{
    public static class SaveLoadManager
    {
        private static string SaveDirectory => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Beyond_Industry",
            "Saves"
        );
        
        private static JsonSerializerOptions JsonOptions => new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };
        
        public static void Initialize()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Console.WriteLine($"[SaveLoad] Created save directory: {SaveDirectory}");
            }
        }
        
        // ===== SAVE GAME - AUTOMATISCH! =====
        public static bool SaveGame(string saveName, FactoryManager factoryManager, CameraController cameraController)
        {
            try
            {
                Console.WriteLine($"[SaveLoad] Saving game: {saveName}");
                
                GameSaveData saveData = new GameSaveData
                {
                    SaveName = saveName,
                    SaveTime = DateTime.Now,
                    GameVersion = "0.1.0"
                };
                
                // Speichere Maschinen - AUTOMATISCH!
                var machines = factoryManager.GetAllMachines();
                foreach (var machine in machines)
                {
                    if (machine is ISaveable saveable)
                    {
                        saveData.Machines.Add(new MachineData
                        {
                            MachineType = saveable.GetSaveId(),
                            Position = new Vector3Data(machine.Position),
                            Properties = saveable.Serialize()
                        });
                    }
                }
                
                // Speichere Belt-Verbindungen
                saveData.BeltConnections = SerializeBeltConnections(machines);
                
                // Speichere Kamera
                saveData.Camera = new CameraData
                {
                    Target = new Vector3Data(cameraController.Camera.Target),
                    HorizontalAngle = cameraController.GetHorizontalAngle(),
                    VerticalAngle = cameraController.GetVerticalAngle(),
                    Distance = cameraController.GetDistance()
                };
                
                // Speichere Stats
                saveData.Stats = new FactoryStatsData
                {
                    TotalPowerGeneration = factoryManager.TotalPowerGeneration,
                    TotalMachines = machines.Count
                };
                
                // Schreibe JSON
                string filePath = Path.Combine(SaveDirectory, $"{saveName}.json");
                string json = JsonSerializer.Serialize(saveData, JsonOptions);
                File.WriteAllText(filePath, json);
                
                Console.WriteLine($"[SaveLoad] ✓ Saved {saveData.Machines.Count} machines to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveLoad] ✗ SAVE ERROR: {ex.Message}");
                Console.WriteLine($"[SaveLoad]   Stack: {ex.StackTrace}");
                return false;
            }
        }
        
        // ===== LOAD GAME - AUTOMATISCH MIT JsonElement FIX! =====
        public static bool LoadGame(string saveName, FactoryManager factoryManager, CameraController cameraController, Dictionary<string, Model> modelMap)
        {
            try
            {
                string filePath = Path.Combine(SaveDirectory, $"{saveName}.json");
                
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[SaveLoad] ✗ Save not found: {filePath}");
                    return false;
                }
                
                Console.WriteLine($"[SaveLoad] Loading: {saveName}");
                
                string json = File.ReadAllText(filePath);
                GameSaveData? saveData = JsonSerializer.Deserialize<GameSaveData>(json, JsonOptions);
                
                if (saveData == null)
                {
                    Console.WriteLine($"[SaveLoad] ✗ Failed to deserialize");
                    return false;
                }
                
                // Lösche aktuelle Factory
                factoryManager.Clear();
                
                // Lade Maschinen - AUTOMATISCH!
                List<FactoryMachine> loadedMachines = new List<FactoryMachine>();
                foreach (var machineData in saveData.Machines)
                {
                    FactoryMachine? machine = CreateMachineFromData(machineData, modelMap);
                    
                    if (machine != null && machine is ISaveable saveable)
                    {
                        // ===== FIX: KONVERTIERE JsonElement PROPERTIES! =====
                        var convertedProperties = ConvertJsonElementProperties(machineData.Properties);
                        saveable.Deserialize(convertedProperties);
                        
                        factoryManager.AddMachine(machine);
                        loadedMachines.Add(machine);
                    }
                }
                
                // Lade Belt-Verbindungen
                DeserializeBeltConnections(saveData.BeltConnections, loadedMachines);
                
                // Lade Kamera
                cameraController.SetPosition(
                    saveData.Camera.Target.ToVector3(),
                    saveData.Camera.HorizontalAngle,
                    saveData.Camera.VerticalAngle,
                    saveData.Camera.Distance
                );
                
                // Lade Stats
                factoryManager.TotalPowerGeneration = saveData.Stats.TotalPowerGeneration;
                
                Console.WriteLine($"[SaveLoad] ✓ Loaded {loadedMachines.Count} machines");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveLoad] ✗ LOAD ERROR: {ex.Message}");
                Console.WriteLine($"[SaveLoad]   Stack: {ex.StackTrace}");
                return false;
            }
        }
        
        // ===== NEU: JsonElement KONVERTER =====
        private static Dictionary<string, object> ConvertJsonElementProperties(Dictionary<string, object> properties)
        {
            var converted = new Dictionary<string, object>();
            
            foreach (var kvp in properties)
            {
                if (kvp.Value is JsonElement element)
                {
                    converted[kvp.Key] = ConvertJsonElement(element);
                }
                else
                {
                    converted[kvp.Key] = kvp.Value;
                }
            }
            
            return converted;
        }
        
        private static object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? "";
                
                case JsonValueKind.Number:
                    // Versuche Int zuerst, dann Float, dann Double
                    if (element.TryGetInt32(out int intValue))
                        return intValue;
                    if (element.TryGetSingle(out float floatValue))
                        return floatValue;
                    return element.GetDouble();
                
                case JsonValueKind.True:
                    return true;
                
                case JsonValueKind.False:
                    return false;
                
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElement(item));
                    }
                    return list;
                
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = ConvertJsonElement(prop.Value);
                    }
                    return dict;
                
                case JsonValueKind.Null:
                default:
                    return null!;
            }
        }
        
        // ===== MASCHINE ERSTELLEN (NUR CONSTRUCTOR!) =====
        private static FactoryMachine? CreateMachineFromData(MachineData data, Dictionary<string, Model> modelMap)
        {
            Vector3 position = data.Position.ToVector3();
            Model model = modelMap.GetValueOrDefault("default");
            
            // ===== FIX: KONVERTIERE PROPERTIES VOR VERWENDUNG =====
            var props = ConvertJsonElementProperties(data.Properties);
            
            switch (data.MachineType)
            {
                case "MiningDrill_Iron":
                    model = modelMap.GetValueOrDefault("MiningDrill_Iron", model);
                    string ironResource = props.ContainsKey("ResourceType") ? props["ResourceType"].ToString() ?? "IronOre" : "IronOre";
                    return new MiningMachine(position, model, ironResource);
                
                case "MiningDrill_Copper":
                    model = modelMap.GetValueOrDefault("MiningDrill_Copper", model);
                    string copperResource = props.ContainsKey("ResourceType") ? props["ResourceType"].ToString() ?? "CopperOre" : "CopperOre";
                    return new MiningMachine(position, model, copperResource);
                
                case "Iron_Furnace":
                    model = modelMap.GetValueOrDefault("Iron_Furnace", model);
                    string input = props.ContainsKey("InputResource") ? props["InputResource"].ToString() ?? "IronOre" : "IronOre";
                    string output = props.ContainsKey("OutputResource") ? props["OutputResource"].ToString() ?? "IronPlate" : "IronPlate";
                    return new FurnaceMachine(position, model, input, output);
                
                case "Copper_Furnace":
                    model = modelMap.GetValueOrDefault("Copper_Furnace", model);
                    string copperInput = props.ContainsKey("InputResource") ? props["InputResource"].ToString() ?? "CopperOre" : "CopperOre";
                    string copperOutput = props.ContainsKey("OutputResource") ? props["OutputResource"].ToString() ?? "CopperPlate" : "CopperPlate";
                    return new FurnaceMachine(position, model, copperInput, copperOutput);

                case "T_Traeger_Vertikal":
                    model = modelMap.GetValueOrDefault("T_Traeger_Vertikal", model);
                    return new T_Traeger_Vertikal(position, model, 3.0f);
                    
                case "ConveyorBelt":
                    BeltType type = BeltType.Straight;
                    if (props.ContainsKey("BeltType") && Enum.TryParse(props["BeltType"].ToString(), out BeltType parsedType))
                    {
                        type = parsedType;
                    }
                    
                    Vector3 direction = new Vector3(1, 0, 0);
                    if (props.ContainsKey("Direction") && props["Direction"] is Dictionary<string, object> dirDict)
                    {
                        float x = Convert.ToSingle(dirDict["X"]);
                        float y = Convert.ToSingle(dirDict["Y"]);
                        float z = Convert.ToSingle(dirDict["Z"]);
                        direction = new Vector3(x, y, z);
                    }
                    
                    string modelKey = type switch
                    {
                        BeltType.CurveLeft => "belt_curve_left",
                        BeltType.CurveRight => "belt_curve_right",
                        BeltType.RampUp => "belt_ramp_up",
                        BeltType.RampDown => "belt_ramp_down",
                        _ => "belt_straight"
                    };
                    
                    model = modelMap.GetValueOrDefault(modelKey, modelMap.GetValueOrDefault("default", model));
                    
                    return new ConveyorBelt(position, model, direction, type);
                
                default:
                    Console.WriteLine($"[SaveLoad] ✗ Unknown type: {data.MachineType}");
                    return null;
            }
        }
        
        private static List<BeltConnectionData> SerializeBeltConnections(List<FactoryMachine> machines)
        {
            List<BeltConnectionData> connections = new List<BeltConnectionData>();
            
            for (int i = 0; i < machines.Count; i++)
            {
                if (machines[i] is ConveyorBelt belt)
                {
                    connections.Add(new BeltConnectionData
                    {
                        BeltIndex = i,
                        InputMachineIndex = belt.InputMachine != null ? machines.IndexOf(belt.InputMachine) : null,
                        OutputMachineIndex = belt.OutputMachine != null ? machines.IndexOf(belt.OutputMachine) : null
                    });
                }
            }
            
            return connections;
        }
        
        private static void DeserializeBeltConnections(List<BeltConnectionData> connections, List<FactoryMachine> machines)
        {
            foreach (var connection in connections)
            {
                if (connection.BeltIndex >= 0 && connection.BeltIndex < machines.Count && machines[connection.BeltIndex] is ConveyorBelt belt)
                {
                    if (connection.InputMachineIndex.HasValue && connection.InputMachineIndex.Value >= 0 && connection.InputMachineIndex.Value < machines.Count)
                    {
                        belt.InputMachine = machines[connection.InputMachineIndex.Value];
                        Console.WriteLine($"[SaveLoad] Belt {connection.BeltIndex} → Input: {belt.InputMachine.MachineType}");
                    }
                    
                    if (connection.OutputMachineIndex.HasValue && connection.OutputMachineIndex.Value >= 0 && connection.OutputMachineIndex.Value < machines.Count)
                    {
                        belt.OutputMachine = machines[connection.OutputMachineIndex.Value];
                        Console.WriteLine($"[SaveLoad] Belt {connection.BeltIndex} → Output: {belt.OutputMachine.MachineType}");
                    }
                }
            }
        }
        
        public static List<string> GetSaveList()
        {
            List<string> saves = new List<string>();
            if (!Directory.Exists(SaveDirectory)) return saves;
            
            string[] files = Directory.GetFiles(SaveDirectory, "*.json");
            foreach (string file in files)
                saves.Add(Path.GetFileNameWithoutExtension(file));
            
            return saves;
        }
        
        public static bool DeleteSave(string saveName)
        {
            try
            {
                string filePath = Path.Combine(SaveDirectory, $"{saveName}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"[SaveLoad] Deleted: {saveName}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveLoad] Delete failed: {ex.Message}");
                return false;
            }
        }
    }
}
