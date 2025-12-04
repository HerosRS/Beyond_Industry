using System;
using System.Numerics;
using System.Collections.Generic;
using Raylib_cs;
using BeyondIndustry.Factory;
using BeyondIndustry.Data;

namespace BeyondIndustry
{
    /// <summary>
    /// Verwaltung des Platzier-Systems: Auswahl von Maschinen, Vorschaupositionen, Layer-Management
    /// und tatsächliches Platzieren / Entfernen von Maschinen in der Fabrikwelt.
    /// </summary>
    public class PlacementSystem
    {
        private List<MachineDefinition> machineDefinitions;
        private FactoryManager factoryManager;
        
        public int SelectedMachineIndex { get; private set; } = 0;
        public bool ShowPreview { get; private set; } = false;
        public Vector3 PreviewPosition { get; private set; }
        
        public int GridSize { get; set; } = 100;
        public float CellSize { get; set; } = 1.0f;
        
        public int CurrentLayer { get; private set; } = 0;
        public float LayerHeight { get; set; } = 1.0f;
        public bool AutoDetectHeight { get; set; } = true;
        public bool SnapToSurface { get; set; } = true;
        
        private FactoryMachine? detectedBaseMachine = null;
        private float detectedHeight = 0f;
        private bool canPlace = false;
        
        private Vector3 currentBeltDirection = new Vector3(1, 0, 0);
        
        /// <summary>
        /// Konstruktor: Initialisiert das PlacementSystem mit verfügbaren Maschinendefinitionen
        /// und einer Referenz auf den FactoryManager zur Verwaltung der Maschinen.
        /// </summary>
        public PlacementSystem(List<MachineDefinition> definitions, FactoryManager manager)
        {
            machineDefinitions = definitions;
            factoryManager = manager;
        }
        
        /// <summary>
        /// Wählt die nächste Maschine in der Definitionsliste (cyclic).
        /// </summary>
        public void SelectNext()
        {
            SelectedMachineIndex = (SelectedMachineIndex + 1) % machineDefinitions.Count;
        }
        
        /// <summary>
        /// Wählt die Maschine an einem bestimmten Index, falls gültig.
        /// </summary>
        /// <param name="index">Index der zu wählenden Maschine.</param>
        public void SelectIndex(int index)
        {
            if (index >= 0 && index < machineDefinitions.Count)
                SelectedMachineIndex = index;
        }
        
        /// <summary>
        /// Dreht die aktuelle Förderband-Richtung um 90° (nützlich für Belt-Rotation bei Platzierung).
        /// </summary>
        public void RotateBelt()
        {
            float angle90 = MathF.PI / 2.0f;
            float currentAngle = MathF.Atan2(currentBeltDirection.X, currentBeltDirection.Z);
            float newAngle = currentAngle + angle90;
            
            currentBeltDirection = new Vector3(
                MathF.Sin(newAngle),
                0,
                MathF.Cos(newAngle)
            );
            
            currentBeltDirection = Vector3.Normalize(currentBeltDirection);
        }
        
        /// <summary>
        /// Erhöht den aktuellen Layer (Höhenebene) um 1 und gibt die neue Höhe in der Konsole aus.
        /// </summary>
        public void IncreaseLayer()
        {
            CurrentLayer++;
            Console.WriteLine($"[Placement] Layer: {CurrentLayer} (Height: {CurrentLayer * LayerHeight}m)");
        }
        
        /// <summary>
        /// Verringert den aktuellen Layer um 1 (falls größer als 0) und gibt die neue Höhe aus.
        /// </summary>
        public void DecreaseLayer()
        {
            if (CurrentLayer > 0)
            {
                CurrentLayer--;
                Console.WriteLine($"[Placement] Layer: {CurrentLayer} (Height: {CurrentLayer * LayerHeight}m)");
            }
        }
        
        /// <summary>
        /// Setzt den aktuellen Layer zurück auf 0.
        /// </summary>
        public void ResetLayer()
        {
            CurrentLayer = 0;
            Console.WriteLine("[Placement] Layer reset to 0");
        }
        
        /// <summary>
        /// Aktualisiert die Vorschauposition basierend auf der Bildschirm-Mausposition.
        /// Führt Raycasts gegen Maschinen und Boden aus, ermittelt Snapping, Layer-Höhe und
        /// setzt die Flags canPlace / ShowPreview entsprechend.
        /// </summary>
        /// <param name="mousePosition">aktuelle Mausposition in Bildschirmkoordinaten (Vector2)</param>
        public void UpdatePreview(Vector2 mousePosition)
        {
            Ray mouseRay = Raylib.GetScreenToWorldRay(mousePosition, GlobalData.camera);
            
            MachineDefinition currentDef = machineDefinitions[SelectedMachineIndex];
            
            ShowPreview = false;
            canPlace = false;
            detectedBaseMachine = null;
            detectedHeight = 0f;
            
            // ===== SCHRITT 1: SUCHE NACH MASCHINEN =====
            FactoryMachine? hitMachine = null;
            float closestDistance = float.MaxValue;
            
            if (AutoDetectHeight || SnapToSurface)
            {
                foreach (var machine in factoryManager.GetAllMachines())
                {
                    BoundingBox machineBox = new BoundingBox(
                        machine.Position - new Vector3(0.5f, 0, 0.5f),
                        machine.Position + new Vector3(0.5f, 2.0f, 0.5f)
                    );
                    
                    RayCollision collision = Raylib.GetRayCollisionBox(mouseRay, machineBox);
                    
                    if (collision.Hit && collision.Distance < closestDistance)
                    {
                        closestDistance = collision.Distance;
                        hitMachine = machine;
                    }
                }
            }
            
            // ===== SCHRITT 2: PLATZIERUNGS-HÖHE =====
            Vector3 placementPos;
            float targetHeight = CurrentLayer * LayerHeight;
            
            if (hitMachine != null && SnapToSurface)
            {
                // Wenn eine Maschine getroffen wurde und Snap enabled ist, platziere relativ zu dieser Maschine
                detectedBaseMachine = hitMachine;
                
                // Platziere 1 Block höher als die Basis-Maschine
                detectedHeight = hitMachine.Position.Y + 1.0f;
                
                int gridX = (int)MathF.Round(hitMachine.Position.X / CellSize);
                int gridZ = (int)MathF.Round(hitMachine.Position.Z / CellSize);
                
                placementPos = new Vector3(
                    gridX * CellSize,
                    detectedHeight + currentDef.YOffset,
                    gridZ * CellSize
                );
                
                canPlace = true;
            }
            else
            {
                // ===== GROUND PLACEMENT =====
                float planeSize = 500f;
                
                // Versuche direkten Quad-Collision-Check (schneller) und fallback auf plane intersection
                RayCollision groundCollision = Raylib.GetRayCollisionQuad(
                    mouseRay,
                    new Vector3(-planeSize, targetHeight, -planeSize),
                    new Vector3(-planeSize, targetHeight, planeSize),
                    new Vector3(planeSize, targetHeight, planeSize),
                    new Vector3(planeSize, targetHeight, -planeSize)
                );
                
                if (!groundCollision.Hit)
                {
                    // Falls Quad-Collision fehlschlägt, berechne die Schnittstelle mit Y-Ebene manuell
                    if (TryGetPlaneIntersection(mouseRay, targetHeight, out Vector3 intersection))
                    {
                        int gridX = (int)MathF.Round(intersection.X / CellSize);
                        int gridZ = (int)MathF.Round(intersection.Z / CellSize);
                        int halfGrid = GridSize / 2;
                        
                        if (gridX >= -halfGrid && gridX <= halfGrid && 
                            gridZ >= -halfGrid && gridZ <= halfGrid)
                        {
                            placementPos = new Vector3(
                                gridX * CellSize,
                                targetHeight + currentDef.YOffset,
                                gridZ * CellSize
                            );
                            
                            detectedHeight = targetHeight;
                            canPlace = true;
                            
                            ShowPreview = true;
                            PreviewPosition = placementPos;
                        }
                    }
                    return;
                }
                
                // Wenn Quad gehitten wurde, nimm den errechneten Punkt
                Vector3 hitPoint = groundCollision.Point;
                
                int gridX2 = (int)MathF.Round(hitPoint.X / CellSize);
                int gridZ2 = (int)MathF.Round(hitPoint.Z / CellSize);
                int halfGrid2 = GridSize / 2;
                
                if (gridX2 < -halfGrid2 || gridX2 > halfGrid2 || 
                    gridZ2 < -halfGrid2 || gridZ2 > halfGrid2)
                    return;
                
                placementPos = new Vector3(
                    gridX2 * CellSize,
                    targetHeight + currentDef.YOffset,
                    gridZ2 * CellSize
                );
                
                detectedHeight = targetHeight;
                canPlace = true;
            }
            
            // ===== SCHRITT 3: CHECK OB FREI =====
            if (canPlace)
            {
                foreach (var machine in factoryManager.GetAllMachines())
                {
                    float distance = Vector3.Distance(machine.Position, placementPos);
                    if (distance < 0.5f)
                    {
                        // Wenn bereits eine Maschine zu nah ist, Platzierung verbieten
                        canPlace = false;
                        break;
                    }
                }
            }
            
            ShowPreview = true;
            PreviewPosition = placementPos;
        }
        
        /// <summary>
        /// Berechnet die Schnittstelle eines Rays mit einer horizontalen Ebene auf Höhe planeY.
        /// Gibt true zurück und liefert die Schnittposition, wenn eine Schnittstelle vorliegt.
        /// </summary>
        /// <param name="ray">Der Ray im Welt-Raum</param>
        /// <param name="planeY">Y-Koordinate der Ebene</param>
        /// <param name="intersection">ausgabe: Schnittpunkt (falls vorhanden)</param>
        /// <returns>True, wenn Schnitt vorhanden und vor dem Ray-Start</returns>
        private bool TryGetPlaneIntersection(Ray ray, float planeY, out Vector3 intersection)
        {
            intersection = Vector3.Zero;
            
            if (MathF.Abs(ray.Direction.Y) < 0.0001f)
            {
                return false;
            }
            
            float t = (planeY - ray.Position.Y) / ray.Direction.Y;
            
            if (t < 0)
            {
                return false;
            }
            
            intersection = ray.Position + ray.Direction * t;
            return true;
        }
        
        /// <summary>
        /// Zeichnet die Vorschau (Model + Wireframe + Info-Text) an der aktuellen PreviewPosition,
        /// nutzt canPlace um Farbe/Status darzustellen. Zeichnet zusätzlich Debug-Ray wenn aktiviert.
        /// </summary>
        public void DrawPreview()
        {
            if (!ShowPreview) return;
            
            MachineDefinition currentDef = machineDefinitions[SelectedMachineIndex];
            
            Color previewColor = canPlace ? 
                new Color(0, 255, 0, 100) : 
                new Color(255, 0, 0, 100);
            
            Raylib.DrawModelEx(
                currentDef.Model,
                PreviewPosition,
                new Vector3(0, 1, 0),
                0,
                Vector3.One,
                previewColor
            );
            
            Raylib.DrawCubeWires(
                PreviewPosition,
                currentDef.Size.X,
                currentDef.Size.Y,
                currentDef.Size.Z,
                canPlace ? Color.Green : Color.Red
            );
            
            Vector2 screenPos = Raylib.GetWorldToScreen(
                PreviewPosition + new Vector3(0, 2, 0),
                GlobalData.camera
            );
            
            string info = $"{currentDef.Name}";
            if (detectedBaseMachine != null)
            {
                info += $"\nOn: {detectedBaseMachine.MachineType}";
            }
            info += $"\nLayer: {CurrentLayer} ({detectedHeight:F1}m)";
            info += canPlace ? "\n[Can Place]" : "\n[Cannot Place]";
            
            Raylib.DrawText(info, (int)screenPos.X - 50, (int)screenPos.Y, 14, 
                canPlace ? Color.Green : Color.Red);
            
            if (detectedBaseMachine != null)
            {
                Raylib.DrawLine3D(
                    detectedBaseMachine.Position + new Vector3(0, 1, 0),
                    PreviewPosition,
                    new Color(255, 255, 0, 100)
                );
            }
            
            if (GlobalData.ShowDebugInfo)
            {
                Ray mouseRay = Raylib.GetScreenToWorldRay(Raylib.GetMousePosition(), GlobalData.camera);
                Vector3 rayEnd = mouseRay.Position + mouseRay.Direction * 100f;
                Raylib.DrawLine3D(mouseRay.Position, rayEnd, new Color(255, 0, 255, 100));
                Raylib.DrawSphere(mouseRay.Position, 0.2f, Color.Magenta);
            }
        }
        
        /// <summary>
        /// Platziert die aktuell ausgewählte Maschine an der PreviewPosition (falls gültig).
        /// Erzeugt den passenden Maschinen-Typ (z. B. ConveyorBelt, MiningMachine, Furnace) und
        /// fügt die Maschine dem FactoryManager hinzu.
        /// </summary>
        public void PlaceObject()
        {
            if (!ShowPreview || !canPlace) return;
            
            MachineDefinition currentDef = machineDefinitions[SelectedMachineIndex];
            FactoryMachine? newMachine = null;
            
            if (currentDef.MachineType == "ConveyorBelt")
            {
                BeltType type = BeltType.Straight;
                if (currentDef.CustomData != null && currentDef.CustomData.ContainsKey("BeltType"))
                {
                    type = (BeltType)currentDef.CustomData["BeltType"];
                }
                
                newMachine = new ConveyorBelt(
                    PreviewPosition,
                    currentDef.Model,
                    currentBeltDirection,
                    type
                );
                
                if (newMachine is ConveyorBelt newBelt)
                {
                    BeltConnectionHelper.ConnectBelt(newBelt, factoryManager.GetAllMachines());
                }
                
                BeltConnectionHelper.UpdateAllConnections(factoryManager);
            }
            // ===== NEU: T-TRÄGER =====
            else if (currentDef.MachineType == "T_Traeger_Vertikal")
            {
                newMachine = new T_Traeger_Vertikal(PreviewPosition, currentDef.Model, 3.0f);
            }
            else
            {
                // Normale Maschinen (Miner, Furnace, etc.)
                if (currentDef.CreateMachineFunc != null)
                {
                    newMachine = currentDef.CreateMachineFunc(PreviewPosition);
                }
                else
                {
                    Console.WriteLine($"[Placement] ERROR: No CreateMachineFunc for {currentDef.MachineType}");
                    return;
                }
            }
            
            if (newMachine != null)
            {
                factoryManager.AddMachine(newMachine);
                Console.WriteLine($"[Placement] Placed {currentDef.Name} at {PreviewPosition} (Layer {CurrentLayer})");
            }
            else
            {
                Console.WriteLine($"[Placement] ERROR: Failed to create machine {currentDef.MachineType}");
            }
        }
        
        /// <summary>
        /// Entfernt die Maschine, auf die mit der Maus gezeigt wird (Raycast gegen Maschinen-BoundingBoxen),
        /// und entfernt diese aus dem FactoryManager, falls gefunden.
        /// </summary>
        public void RemoveObject()
        {
            Ray mouseRay = Raylib.GetScreenToWorldRay(Raylib.GetMousePosition(), GlobalData.camera);
            
            FactoryMachine? closestMachine = null;
            float closestDistance = float.MaxValue;
            
            foreach (var machine in factoryManager.GetAllMachines())
            {
                BoundingBox machineBox = new BoundingBox(
                    machine.Position - new Vector3(0.5f, 0, 0.5f),
                    machine.Position + new Vector3(0.5f, 2.0f, 0.5f)
                );
                
                RayCollision collision = Raylib.GetRayCollisionBox(mouseRay, machineBox);
                
                if (collision.Hit && collision.Distance < closestDistance)
                {
                    closestDistance = collision.Distance;
                    closestMachine = machine;
                }
            }
            
            if (closestMachine != null)
            {
                factoryManager.RemoveMachine(closestMachine);
                Console.WriteLine($"[Placement] Removed {closestMachine.MachineType}");
            }
        }
        
        /// <summary>
        /// Hilfsmethode: Erzeugt eine konkrete FactoryMachine-Instanz anhand der Maschinendefinition.
        /// Unterstützt aktuell MiningDrill_Iron, MiningDrill_Copper und Iron_Furnace.
        /// </summary>
        /// <param name="def">MachineDefinition mit Typ & Model</param>
        /// <param name="position">Platzierungsposition</param>
        /// <returns>Neue FactoryMachine-Instanz oder null, wenn Typ unbekannt ist.</returns>
        private FactoryMachine? CreateMachineFromDefinition(MachineDefinition def, Vector3 position)
        {
            switch (def.MachineType)
            {
                case "MiningDrill_Iron":
                    return new MiningMachine(position, def.Model, "IronOre");
                
                case "MiningDrill_Copper":
                    return new MiningMachine(position, def.Model, "CopperOre");
                
                case "Iron_Furnace":
                    return new FurnaceMachine(position, def.Model, "IronOre", "IronPlate");
                
                default:
                    Console.WriteLine($"[Placement] Unknown machine type: {def.MachineType}");
                    return null;
            }
        }
    }
}
