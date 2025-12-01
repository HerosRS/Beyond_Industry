using System;
using System.Numerics;
using System.Collections.Generic;

namespace BeyondIndustry.Factory
{
    // ===== HELPER FÜR FÖRDERBAND-VERBINDUNGEN =====
    public static class BeltConnectionHelper
    {
        public static void ConnectBelt(ConveyorBelt belt, List<FactoryMachine> allMachines, float connectionDistance = 1.1f)
        {
            Console.WriteLine($"\n=== Verbinde Belt @ {belt.Position}, Richtung: {belt.Direction} ===");
            
            // Input suchen
            Vector3 inputSearchPos = belt.Position - belt.Direction * connectionDistance;
            Console.WriteLine($"Suche Input bei: {inputSearchPos}");
            
            FactoryMachine? inputMachine = FindMachineAtPosition(allMachines, inputSearchPos, 1f);
            
            if (inputMachine != null && inputMachine != belt)
            {
                belt.InputMachine = inputMachine;
                Console.WriteLine($"✓ Input verbunden: {inputMachine.MachineType} @ {inputMachine.Position}");
            }
            else
            {
                belt.InputMachine = null;
                Console.WriteLine($"✗ Kein Input gefunden");
            }
            
            // Output suchen
            Vector3 outputSearchPos = belt.Position + belt.Direction * connectionDistance;
            Console.WriteLine($"Suche Output bei: {outputSearchPos}");
            
            FactoryMachine? outputMachine = FindMachineAtPosition(allMachines, outputSearchPos, 1f);
            
            if (outputMachine != null && outputMachine != belt)
            {
                belt.OutputMachine = outputMachine;
                Console.WriteLine($"✓ Output verbunden: {outputMachine.MachineType} @ {outputMachine.Position}");
            }
            else
            {
                belt.OutputMachine = null;
                Console.WriteLine($"✗ Kein Output gefunden");
            }
            
            Console.WriteLine($"=== Verbindung abgeschlossen ===\n");
        }
        
        private static FactoryMachine? FindMachineAtPosition(List<FactoryMachine> machines, Vector3 position, float tolerance)
        {
            Console.WriteLine($"  Suche Maschine bei {position} (Toleranz: {tolerance})");
            
            foreach (var machine in machines)
            {
                // NUR X und Z vergleichen, Y ignorieren
                float distanceXZ = MathF.Sqrt(
                    MathF.Pow(machine.Position.X - position.X, 2) + 
                    MathF.Pow(machine.Position.Z - position.Z, 2)
                );
                
                Console.WriteLine($"    - {machine.MachineType} @ {machine.Position}, Distanz (XZ): {distanceXZ:F2}");
                
                if (distanceXZ < tolerance)
                {
                    Console.WriteLine($"    → GEFUNDEN!");
                    return machine;
                }
            }
            
            Console.WriteLine($"    → Nichts gefunden");
            return null;
        }
        
        public static void UpdateAllConnections(FactoryManager factoryManager)
        {
            // GEÄNDERT: Verwende GetAllBelts() statt GetMachinesOfType<T>()
            List<ConveyorBelt> belts = factoryManager.GetAllBelts();
            List<FactoryMachine> allMachines = factoryManager.GetAllMachines();
            
            Console.WriteLine($"\n========== UPDATE ALL BELT CONNECTIONS ==========");
            Console.WriteLine($"Anzahl Bänder: {belts.Count}");
            Console.WriteLine($"Anzahl Maschinen gesamt: {allMachines.Count}");
            
            foreach (var belt in belts)
            {
                ConnectBelt(belt, allMachines);
            }
            
            Console.WriteLine($"========== VERBINDUNGEN ABGESCHLOSSEN ==========\n");
        }
    }
}