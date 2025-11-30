using System;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== FACTORY MANAGER =====
    // Verwaltet alle Factory-Maschinen zentral
    public class FactoryManager
    {
        // Liste aller Maschinen
        private List<FactoryMachine> machines;
        
        public List<FactoryMachine> GetAllMachines()
        {
            return new List<FactoryMachine>(machines);
        }
        // Gesamt-Energie-System
        public float TotalPowerGeneration { get; set; }     // Wie viel Strom wird erzeugt
        public float TotalPowerDemand { get; private set; } // Wie viel wird benötigt
        public float PowerEfficiency => TotalPowerGeneration > 0 ? 
                                       Math.Min(1f, TotalPowerGeneration / TotalPowerDemand) : 
                                       0f;
        
        public FactoryManager()
        {
            machines = new List<FactoryMachine>();
            TotalPowerGeneration = 100f;  // Start mit 100 Einheiten Strom
        }
        
        // ===== MASCHINE HINZUFÜGEN =====
        public void AddMachine(FactoryMachine machine)
        {
            machines.Add(machine);
            Console.WriteLine($"[Factory] Maschine platziert: {machine.MachineType} bei {machine.Position}");
        }
        
        // ===== MASCHINE ENTFERNEN =====
        public void RemoveMachine(FactoryMachine machine)
        {
            machines.Remove(machine);
            Console.WriteLine($"[Factory] Maschine entfernt: {machine.MachineType}");
        }
        
        // ===== UPDATE ALLE MASCHINEN =====
        public void Update(float deltaTime)
        {
            // 1. Berechne Gesamt-Strombedarf
            TotalPowerDemand = 0f;
            foreach (var machine in machines)
            {
                TotalPowerDemand += machine.PowerConsumption;
            }
            
            // 2. Verteile Strom an Maschinen
            // Wenn nicht genug Strom, wird proportional verteilt
            float powerRatio = TotalPowerDemand > 0 ? 
                              Math.Min(1f, TotalPowerGeneration / TotalPowerDemand) : 
                              1f;
            
            foreach (var machine in machines)
            {
                machine.CurrentPower = machine.PowerConsumption * powerRatio;
            }
            
            // 3. Update alle Maschinen
            foreach (var machine in machines)
            {
                machine.Update(deltaTime);
            }
        }
        
        // ===== ZEICHNE ALLE MASCHINEN =====
        public void DrawAll()
        {
            foreach (var machine in machines)
            {
                machine.Draw();
            }
        }
        
        // ===== DEBUG INFO =====
        public void DrawDebugInfo(int startY = 150)
        {
            Raylib.DrawText($"=== Factory Status ===", 10, startY, 20, Color.White);
            Raylib.DrawText($"Power: {TotalPowerDemand:F0}/{TotalPowerGeneration:F0} ({PowerEfficiency:P0})", 10, startY + 25, 20, Color.Yellow);
            Raylib.DrawText($"Machines: {machines.Count}", 10, startY + 50, 20, Color.White);
            
            int y = startY + 80;
            foreach (var machine in machines)
            {
                string info = machine.GetDebugInfo();
                Color infoColor = machine.IsRunning ? Color.Green : Color.Gray;
                Raylib.DrawText(info, 10, y, 16, infoColor);
                y += 20;
            }
        }
        
        // ===== HILFSMETHODEN =====
        
        // Finde Maschine an Position
// Ändere diese Methode:
public FactoryMachine? GetMachineAtPosition(System.Numerics.Vector3 position, float tolerance = 0.1f)
{
    foreach (var machine in machines)
    {
        if (System.Numerics.Vector3.Distance(machine.Position, position) < tolerance)
        {
            return machine;
        }
    }
    return null;
}
        
        // Hole alle Maschinen eines bestimmten Typs
        public List<T> GetMachinesOfType<T>() where T : FactoryMachine
        {
            List<T> result = new List<T>();
            foreach (var machine in machines)
            {
                if (machine is T typedMachine)
                {
                    result.Add(typedMachine);
                }
            }
            return result;
        }
        
        // Statistiken
        public int GetTotalMachinesRunning()
        {
            int count = 0;
            foreach (var machine in machines)
            {
                if (machine.IsRunning) count++;
            }
            return count;
        }
    }
}