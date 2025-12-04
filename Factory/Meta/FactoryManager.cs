using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    public class FactoryManager
    {
        private List<FactoryMachine> machines = new List<FactoryMachine>();
        
        public float TotalPowerGeneration { get; set; }
        public float CurrentPowerConsumption { get; private set; }
        
        public void AddMachine(FactoryMachine machine)
        {
            machines.Add(machine);
            Console.WriteLine($"[Factory] Maschine platziert: {machine.MachineType} bei {machine.Position}");
        }
        
        public void RemoveMachine(FactoryMachine machine)
        {
            machines.Remove(machine);
            Console.WriteLine($"[Factory] Maschine entfernt: {machine.MachineType}");
        }
        
        public void Update(float deltaTime)
        {
            CurrentPowerConsumption = 0f;
            
            foreach (var machine in machines)
            {
                machine.Update(deltaTime);
                
                if (machine.IsRunning)
                {
                    CurrentPowerConsumption += machine.PowerConsumption;
                }
            }
        }
        
        // ===== BUTTON CLICKS VERARBEITEN =====
        public void HandleMachineClicks(Vector2 mousePosition)
        {
            for (int i = machines.Count - 1; i >= 0; i--)
            {
                machines[i].CheckButtonClick(mousePosition, Data.GlobalData.camera);
            }
        }
        
        public float GetTotalPowerConsumption()
        {
            float total = 0f;
            foreach (var machine in machines)
            {
                if (machine.IsRunning)
                {
                    total += machine.PowerConsumption;
                }
            }
            return total;
        }
        
        public void DrawAll()
        {
            foreach (var machine in machines)
            {
                machine.Draw();
            }
        }
        
        public void Clear()
        {
            machines.Clear();
            Console.WriteLine("[Factory] Cleared all machines");
        }

        public List<FactoryMachine> GetAllMachines()
        {
            return new List<FactoryMachine>(machines);
        }
        
        public void DrawDebugInfo(int startY)
        {
            int y = startY;
            Raylib.DrawText($"=== FACTORY STATUS ===", 10, y, 16, Color.White);
            y += 20;
            
            Raylib.DrawText($"Machines: {machines.Count}", 10, y, 14, Color.LightGray);
            y += 18;
            
            Raylib.DrawText($"Power: {CurrentPowerConsumption:F1}W / {TotalPowerGeneration:F1}W", 10, y, 14, 
                CurrentPowerConsumption > TotalPowerGeneration ? Color.Red : Color.Green);
            y += 18;
            
            int active = 0, disabled = 0;
            foreach (var machine in machines)
            {
                if (machine.IsManuallyEnabled)
                    active++;
                else
                    disabled++;
            }
            
            Raylib.DrawText($"Active: {active} | Disabled: {disabled}", 10, y, 14, Color.Yellow);
            y += 25;
            
            foreach (var machine in machines)
            {
                Color statusColor = machine.IsManuallyEnabled ?
                    (machine.IsRunning ? Color.Green : Color.Yellow) :
                    Color.Red;
                
                Raylib.DrawText(machine.GetDebugInfo(), 10, y, 12, statusColor);
                y += 16;
            }
        }
        
        public List<ConveyorBelt> GetAllBelts()
        {
            List<ConveyorBelt> belts = new List<ConveyorBelt>();
            foreach (var machine in machines)
            {
                if (machine is ConveyorBelt belt)
                {
                    belts.Add(belt);
                }
            }
            return belts;
        }
    }
}