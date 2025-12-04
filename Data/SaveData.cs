using System;
using System.Numerics;
using System.Collections.Generic;

namespace BeyondIndustry.Data
{
    [Serializable]
    public class GameSaveData
    {
        public string SaveName { get; set; } = "Unnamed Save";
        public DateTime SaveTime { get; set; }
        public string GameVersion { get; set; } = "0.1.0";
        
        public List<MachineData> Machines { get; set; } = new List<MachineData>();
        public List<BeltConnectionData> BeltConnections { get; set; } = new List<BeltConnectionData>();
        
        public CameraData Camera { get; set; } = new CameraData();
        public FactoryStatsData Stats { get; set; } = new FactoryStatsData();
    }
    
    // ===== UNIVERSAL MACHINE DATA =====
    [Serializable]
    public class MachineData
    {
        public string MachineType { get; set; } = "";
        public Vector3Data Position { get; set; } = new Vector3Data();
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class BeltConnectionData
    {
        public int BeltIndex { get; set; }
        public int? InputMachineIndex { get; set; }
        public int? OutputMachineIndex { get; set; }
    }
    
    [Serializable]
    public class CameraData
    {
        public Vector3Data Target { get; set; } = new Vector3Data();
        public float HorizontalAngle { get; set; } = 45f;
        public float VerticalAngle { get; set; } = 45f;
        public float Distance { get; set; } = 30f;
    }
    
    [Serializable]
    public class FactoryStatsData
    {
        public float TotalPowerGeneration { get; set; } = 200f;
        public int TotalMachines { get; set; } = 0;
    }
    
    [Serializable]
    public class Vector3Data
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
        public Vector3Data() { }
        
        public Vector3Data(Vector3 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = vector.Z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }
}