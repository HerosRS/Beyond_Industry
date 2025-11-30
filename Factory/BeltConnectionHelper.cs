using System;
using System.Numerics;
using System.Collections.Generic;

namespace BeyondIndustry.Factory
{
    // ===== HELPER FÜR FÖRDERBAND-VERBINDUNGEN =====
    // Automatisches Verbinden von Bändern mit Maschinen
    public static class BeltConnectionHelper
    {
        // ===== VERBINDE BAND MIT MASCHINEN =====
        // Sucht automatisch nach Maschinen in Richtung des Bands
        public static void ConnectBelt(ConveyorBelt belt, List<FactoryMachine> allMachines, float connectionDistance = 1.1f)
        {
            // Suche nach Input-Maschine (hinter dem Band)
            Vector3 inputSearchPos = belt.Position - belt.Direction * connectionDistance;
            FactoryMachine inputMachine = FindMachineAtPosition(allMachines, inputSearchPos, 0.5f);
            
            if (inputMachine != null && inputMachine != belt)
            {
                belt.InputMachine = inputMachine;
                Console.WriteLine($"[Belt] Input verbunden: {inputMachine.MachineType}");
            }
            
            // Suche nach Output-Maschine (vor dem Band)
            Vector3 outputSearchPos = belt.Position + belt.Direction * connectionDistance;
            FactoryMachine outputMachine = FindMachineAtPosition(allMachines, outputSearchPos, 0.5f);
            
            if (outputMachine != null && outputMachine != belt)
            {
                belt.OutputMachine = outputMachine;
                Console.WriteLine($"[Belt] Output verbunden: {outputMachine.MachineType}");
            }
        }
        
        // ===== FINDE MASCHINE AN POSITION =====
      // Ändere diese Methode:
        private static FactoryMachine? FindMachineAtPosition(List<FactoryMachine> machines, Vector3 position, float tolerance)
        {
            foreach (var machine in machines)
            {
                if (Vector3.Distance(machine.Position, position) < tolerance)
                {
                    return machine;
                }
            }
            return null;
        }
        
        // ===== AKTUALISIERE ALLE VERBINDUNGEN =====
        // Rufe das auf wenn Maschinen platziert/entfernt werden
        public static void UpdateAllConnections(FactoryManager factoryManager)
        {
            List<ConveyorBelt> belts = factoryManager.GetMachinesOfType<ConveyorBelt>();
            List<FactoryMachine> allMachines = factoryManager.GetAllMachines();
            
            foreach (var belt in belts)
            {
                ConnectBelt(belt, allMachines);
            }
            
            Console.WriteLine($"[Belt] {belts.Count} Bänder verbunden");
        }
    }
}