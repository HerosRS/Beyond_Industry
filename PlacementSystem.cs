using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory;

namespace BeyondIndustry
{
    public class PlacedObject
    {
        public Vector3 Position;
        public Model Model;
        public string Type;
        public Vector3 Size;
        public FactoryMachine? Machine;
        
        public PlacedObject(Vector3 pos, Model model, string type, Vector3 size, FactoryMachine? machine = null)
        {
            Position = pos;
            Model = model;
            Type = type;
            Size = size;
            Machine = machine;
        }
    }
    
    public class PlacementSystem
    {
        // Grid-Einstellungen
        public int GridSize { get; set; } = 9;
        public float CellSize { get; set; } = 1.0f;
        
        // Platzierte Objekte
        public List<PlacedObject> PlacedObjects { get; private set; }
        
        // Preview
        public bool ShowPreview { get; set; }
        public Vector3 PreviewPosition { get; set; }
        public int SelectedMachineIndex { get; set; }
        
        // Belt-Rotation
        public int BeltRotation { get; set; }
        public Vector3[] BeltDirections { get; private set; }
        public string[] BeltDirectionNames { get; private set; }
        
        private List<MachineDefinition> machineDefinitions;
        private FactoryManager factoryManager;
        private Camera3D camera;
        
        public PlacementSystem(List<MachineDefinition> definitions, FactoryManager manager, Camera3D cam)
        {
            PlacedObjects = new List<PlacedObject>();
            machineDefinitions = definitions;
            factoryManager = manager;
            camera = cam;
            
            ShowPreview = false;
            PreviewPosition = Vector3.Zero;
            SelectedMachineIndex = 0;
            
            // Belt-Richtungen
            BeltRotation = 0;
            BeltDirections = new Vector3[]
            {
                new Vector3(1, 0, 0),   // Rechts (→)
                new Vector3(0, 0, 1),   // Unten (↓)
                new Vector3(-1, 0, 0),  // Links (←)
                new Vector3(0, 0, -1)   // Oben (↑)
            };
            BeltDirectionNames = new string[] { "→", "↓", "←", "↑" };
        }
        
        // ===== AUSWAHL =====
        public void SelectNext()
        {
            SelectedMachineIndex = (SelectedMachineIndex + 1) % machineDefinitions.Count;
            Console.WriteLine($"Ausgewählt: {machineDefinitions[SelectedMachineIndex].Name}");
        }
        
        public void SelectIndex(int index)
        {
            if (index >= 0 && index < machineDefinitions.Count)
            {
                SelectedMachineIndex = index;
            }
        }
        
        // ===== BELT-ROTATION =====
        public void RotateBelt()
        {
            BeltRotation = (BeltRotation + 1) % 4;
            Console.WriteLine($"Belt-Richtung: {BeltDirectionNames[BeltRotation]}");
        }
        
        // ===== PREVIEW BERECHNEN =====
        public void UpdatePreview(Vector2 mousePosition)
        {
            Ray mouseRay = Raylib.GetScreenToWorldRay(mousePosition, camera);
            RayCollision groundCollision = Raylib.GetRayCollisionQuad(
                mouseRay,
                new Vector3(-50, 0, -50),
                new Vector3(-50, 0, 50),
                new Vector3(50, 0, 50),
                new Vector3(50, 0, -50)
            );
            
            if (groundCollision.Hit)
            {
                ShowPreview = true;
                Vector3 hitPoint = groundCollision.Point;
                MachineDefinition currentDef = machineDefinitions[SelectedMachineIndex];
                
                int gridX = (int)MathF.Round(hitPoint.X / CellSize);
                int gridZ = (int)MathF.Round(hitPoint.Z / CellSize);
                int halfGrid = GridSize / 2;
                
                if (gridX >= -halfGrid && gridX <= halfGrid && 
                    gridZ >= -halfGrid && gridZ <= halfGrid)
                {
                    PreviewPosition = new Vector3(gridX * CellSize, currentDef.YOffset, gridZ * CellSize);
                }
                else
                {
                    ShowPreview = false;
                }
            }
            else
            {
                ShowPreview = false;
            }
        }
        
        // ===== PLATZIEREN =====
        public void PlaceObject()
        {
            if (!ShowPreview) return;
            
            MachineDefinition currentDef = machineDefinitions[SelectedMachineIndex];
            
            // Kollisionsprüfung
            bool positionOccupied = false;
            foreach (var obj in PlacedObjects)
            {
                float distanceX = MathF.Abs(obj.Position.X - PreviewPosition.X);
                float distanceZ = MathF.Abs(obj.Position.Z - PreviewPosition.Z);
                
                if (distanceX < (obj.Size.X + currentDef.Size.X) / 2 * CellSize &&
                    distanceZ < (obj.Size.Z + currentDef.Size.Z) / 2 * CellSize)
                {
                    positionOccupied = true;
                    break;
                }
            }
            
            if (!positionOccupied)
            {
                FactoryMachine? machine = null;
                
                // Spezielle Behandlung für Conveyor Belts
                if (currentDef.MachineType == "ConveyorBelt")
                {
                    // Hole Belt-Typ aus CustomData
                    BeltType type = BeltType.Straight;
                    if (currentDef.CustomData != null && currentDef.CustomData.ContainsKey("BeltType"))
                    {
                        type = (BeltType)currentDef.CustomData["BeltType"];
                    }
                    
                    // Erstelle Belt mit richtigem Typ
                    machine = new ConveyorBelt(
                        PreviewPosition, 
                        currentDef.Model, 
                        BeltDirections[BeltRotation],
                        type
                    )
                    {
                        PowerConsumption = currentDef.PowerConsumption
                    };
                }
                else
                {
                    machine = currentDef.CreateMachine(PreviewPosition);
                }
                
                if (machine != null)
                {
                    factoryManager.AddMachine(machine);
                    BeltConnectionHelper.UpdateAllConnections(factoryManager);
                }
                
                PlacedObject newObject = new PlacedObject(
                    PreviewPosition,
                    currentDef.Model,
                    currentDef.Name,
                    currentDef.Size,
                    machine
                );
                
                PlacedObjects.Add(newObject);
                Console.WriteLine($"{currentDef.Name} platziert ({PlacedObjects.Count} gesamt)");
            }
        }
        
        // ===== ENTFERNEN =====
        public void RemoveObject()
        {
            if (!ShowPreview) return;
            
            for (int i = PlacedObjects.Count - 1; i >= 0; i--)
            {
                float distanceX = MathF.Abs(PlacedObjects[i].Position.X - PreviewPosition.X);
                float distanceZ = MathF.Abs(PlacedObjects[i].Position.Z - PreviewPosition.Z);
                
                if (distanceX < PlacedObjects[i].Size.X * CellSize / 2 &&
                    distanceZ < PlacedObjects[i].Size.Z * CellSize / 2)
                {
                    if (PlacedObjects[i].Machine != null)
                    {
                        factoryManager.RemoveMachine(PlacedObjects[i].Machine);
                        BeltConnectionHelper.UpdateAllConnections(factoryManager);
                    }
                    
                    Console.WriteLine($"{PlacedObjects[i].Type} entfernt");
                    PlacedObjects.RemoveAt(i);
                    break;
                }
            }
        }
        
        // ===== PREVIEW ZEICHNEN =====
        public void DrawPreview()
        {
            if (ShowPreview && SelectedMachineIndex < machineDefinitions.Count)
            {
                MachineDefinition currentDef = machineDefinitions[SelectedMachineIndex];
                Raylib.DrawModel(currentDef.Model, PreviewPosition, 1.0f, currentDef.PreviewColor);
                Raylib.DrawCubeWires(PreviewPosition, currentDef.Size.X, currentDef.Size.Y, currentDef.Size.Z, Color.White);
                
                // Belt-Richtungspfeil
                if (currentDef.MachineType == "ConveyorBelt")
                {
                    Vector3 arrowStart = PreviewPosition + new Vector3(0, 0.6f, 0);
                    Vector3 arrowEnd = arrowStart + BeltDirections[BeltRotation] * 0.6f;
                    Raylib.DrawLine3D(arrowStart, arrowEnd, Color.Lime);
                    Raylib.DrawSphere(arrowEnd, 0.1f, Color.Lime);
                }
            }
        }
        
        // ===== UI INFO =====
        public string GetSelectedInfo()
        {
            if (SelectedMachineIndex < machineDefinitions.Count)
            {
                string info = $"Selected: {machineDefinitions[SelectedMachineIndex].Name} | Placed: {PlacedObjects.Count}";
                
                if (machineDefinitions[SelectedMachineIndex].MachineType == "ConveyorBelt")
                {
                    info += $" | Direction: {BeltDirectionNames[BeltRotation]}";
                }
                
                return info;
            }
            return "";
        }
    }
}