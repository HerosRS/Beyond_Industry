using Raylib_cs;
using BeyondIndustry.Utils;
using BeyondIndustry.Data;
using BeyondIndustry.DebugView;
using BeyondIndustry.Debug;
using BeyondIndustry.Factory;
using System.Numerics;
using System;
using System.Collections.Generic;

namespace BeyondIndustry
{
    class Program
    {
        // ===== KLASSE FÜR PLATZIERTE OBJEKTE =====
        public class PlacedObject
        {
            public Vector3 Position;
            public Model Model;
            public string Type;
            public Vector3 Size;
            public FactoryMachine Machine;
            
            public PlacedObject(Vector3 pos, Model model, string type, Vector3 size, FactoryMachine? machine = null)
            {
                Position = pos;
                Model = model;
                Type = type;
                Size = size;
                Machine = machine;
            }
        }
        
        static void Main()
        {
            Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");

            // ===== KAMERA SETUP =====
            GlobalData.camera.Position = new Vector3(22.0f, 20.0f, 22.0f);
            GlobalData.camera.Target = new Vector3(0.0f, 0.0f, 0.0f);
            GlobalData.camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
            GlobalData.camera.FovY = 15.0f;
            GlobalData.camera.Projection = CameraProjection.Perspective;
            
            // ===== KALIBRIERBARE KAMERA-EINSTELLUNGEN =====
            float cameraMoveSpeed = 0.1f;
            float cameraRotationSpeed = 0.8f;
            
            // ===== BELEUCHTUNG SETUP =====
            Shader shader = Raylib.LoadShader(@"..\..\..\Resources\lighting.vs", @"..\..\..\Resources\lighting.fs");
            Vector3 lightPosition = new Vector3(0.0f, 10.0f, 0.0f);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "lightPos"),
                new float[] { lightPosition.X, lightPosition.Y, lightPosition.Z }, ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "lightColor"),
                new float[] { 1.0f, 1.0f, 1.0f, 1.0f }, ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "ambient"),
                new float[] { 0.5f, 0.5f, 0.5f, 1.0f }, ShaderUniformDataType.Vec4);

            // ===== LADE 3D MODELLE =====
            Model Wand = Raylib.LoadModel(@"..\..\..\Resources\Wand.obj");
            Model Boden = Raylib.LoadModel(@"..\..\..\Resources\Boden.obj");
            Model MaschineModel = Raylib.LoadModel(@"..\..\..\Resources\Maschiene.obj");
            Model cubeModel = Raylib.LoadModelFromMesh(Raylib.GenMeshCube(1.0f, 1.0f, 1.0f));

            // Shader auf Modelle anwenden
            unsafe
            {
                Boden.Materials[0].Shader = shader;
                Wand.Materials[0].Shader = shader;
                cubeModel.Materials[0].Shader = shader;
                MaschineModel.Materials[0].Shader = shader;
            }

            // ===== INITIALISIERE MASCHINEN-REGISTRY =====
            MachineRegistry.Initialize();

            // ===== MODELL-ZUORDNUNG =====
            var modelMap = new Dictionary<string, Model>
            {
                { "default", cubeModel },
                { "MiningDrill", MaschineModel },
                { "Furnace", cubeModel },
                { "ConveyorBelt", cubeModel }
            };

            // ===== LADE ALLE MASCHINEN-DEFINITIONEN =====
            List<MachineDefinition> machineDefinitions = MachineRegistry.LoadAllDefinitions(modelMap);
         
            // ===== GRID SETUP =====
            Grid grid = new Grid();
            
            // ===== FACTORY MANAGER =====
            FactoryManager factoryManager = new FactoryManager();
            factoryManager.TotalPowerGeneration = 200f;
            
            // ===== PLATZIERUNGS-SYSTEM =====
            List<PlacedObject> placedObjects = new List<PlacedObject>();
            int gridSize = 9;
            float cellSize = 1.0f;
            bool showPreview = false;
            Vector3 previewPosition = Vector3.Zero;
            int selectedMachineIndex = 0;
            
            while (!Raylib.WindowShouldClose())
            {
                // ===== UPDATE =====
                DebugConsole.Update();
                factoryManager.Update(Raylib.GetFrameTime());
                
                if (!DebugConsole.IsOpen())
                {
                    // ===== KAMERA-STEUERUNG =====
                    Vector3 forward = Vector3.Normalize(GlobalData.camera.Target - GlobalData.camera.Position);
                    Vector3 right = Vector3.Normalize(Vector3.Cross(forward, GlobalData.camera.Up));
                    
                    if (Raylib.IsKeyDown(KeyboardKey.W))
                    {
                        GlobalData.camera.Position += forward * cameraMoveSpeed;
                        GlobalData.camera.Target += forward * cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.S))
                    {
                        GlobalData.camera.Position -= forward * cameraMoveSpeed;
                        GlobalData.camera.Target -= forward * cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.A))
                    {
                        GlobalData.camera.Position -= right * cameraMoveSpeed;
                        GlobalData.camera.Target -= right * cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.D))
                    {
                        GlobalData.camera.Position += right * cameraMoveSpeed;
                        GlobalData.camera.Target += right * cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.Space))
                    {
                        GlobalData.camera.Position.Y += cameraMoveSpeed;
                        GlobalData.camera.Target.Y += cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.LeftShift))
                    {
                        GlobalData.camera.Position.Y -= cameraMoveSpeed;
                        GlobalData.camera.Target.Y -= cameraMoveSpeed;
                    }
                    
                    if (Raylib.IsKeyDown(KeyboardKey.Left))
                    {
                        Vector3 direction = GlobalData.camera.Position - GlobalData.camera.Target;
                        float angle = cameraRotationSpeed * Raylib.GetFrameTime();
                        float cosAngle = MathF.Cos(angle);
                        float sinAngle = MathF.Sin(angle);
                        float newX = direction.X * cosAngle - direction.Z * sinAngle;
                        float newZ = direction.X * sinAngle + direction.Z * cosAngle;
                        GlobalData.camera.Position = GlobalData.camera.Target + new Vector3(newX, direction.Y, newZ);
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.Right))
                    {
                        Vector3 direction = GlobalData.camera.Position - GlobalData.camera.Target;
                        float angle = -cameraRotationSpeed * Raylib.GetFrameTime();
                        float cosAngle = MathF.Cos(angle);
                        float sinAngle = MathF.Sin(angle);
                        float newX = direction.X * cosAngle - direction.Z * sinAngle;
                        float newZ = direction.X * sinAngle + direction.Z * cosAngle;
                        GlobalData.camera.Position = GlobalData.camera.Target + new Vector3(newX, direction.Y, newZ);
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.Up))
                    {
                        Vector3 direction = GlobalData.camera.Target - GlobalData.camera.Position;
                        GlobalData.camera.Position += Vector3.Normalize(direction) * cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.Down))
                    {
                        Vector3 direction = GlobalData.camera.Target - GlobalData.camera.Position;
                        GlobalData.camera.Position -= Vector3.Normalize(direction) * cameraMoveSpeed;
                    }

                    // ===== MASCHINEN-AUSWAHL =====
                    if (Raylib.IsKeyPressed(KeyboardKey.Tab))
                    {
                        selectedMachineIndex = (selectedMachineIndex + 1) % machineDefinitions.Count;
                        Console.WriteLine($"Ausgewählt: {machineDefinitions[selectedMachineIndex].Name}");
                    }
                    
                    // Dynamische Zahlentasten basierend auf Anzahl der Definitionen
                    if (Raylib.IsKeyPressed(KeyboardKey.One) && machineDefinitions.Count > 0)
                        selectedMachineIndex = 0;
                    if (Raylib.IsKeyPressed(KeyboardKey.Two) && machineDefinitions.Count > 1)
                        selectedMachineIndex = 1;
                    if (Raylib.IsKeyPressed(KeyboardKey.Three) && machineDefinitions.Count > 2)
                        selectedMachineIndex = 2;
                    if (Raylib.IsKeyPressed(KeyboardKey.Four) && machineDefinitions.Count > 3)
                        selectedMachineIndex = 3;
                    if (Raylib.IsKeyPressed(KeyboardKey.Five) && machineDefinitions.Count > 4)
                        selectedMachineIndex = 4;
                    
                    // ===== MAUS-POSITION AUF GRID =====
                    Ray mouseRay = Raylib.GetMouseRay(Raylib.GetMousePosition(), GlobalData.camera);
                    RayCollision groundCollision = Raylib.GetRayCollisionQuad(
                        mouseRay,
                        new Vector3(-50, 0, -50),
                        new Vector3(-50, 0, 50),
                        new Vector3(50, 0, 50),
                        new Vector3(50, 0, -50)
                    );
                    
                    if (groundCollision.Hit)
                    {
                        showPreview = true;
                        Vector3 hitPoint = groundCollision.Point;
                        MachineDefinition currentDef = machineDefinitions[selectedMachineIndex];
                        
                        int gridX = (int)MathF.Round(hitPoint.X / cellSize);
                        int gridZ = (int)MathF.Round(hitPoint.Z / cellSize);
                        int halfGrid = gridSize / 2;
                        
                        if (gridX >= -halfGrid && gridX <= halfGrid && 
                            gridZ >= -halfGrid && gridZ <= halfGrid)
                        {
                            previewPosition = new Vector3(gridX * cellSize, currentDef.YOffset, gridZ * cellSize);
                        }
                        else
                        {
                            showPreview = false;
                        }
                    }
                    else
                    {
                        showPreview = false;
                    }
                    
                    // ===== PLATZIEREN =====
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left) && showPreview)
                    {
                        MachineDefinition currentDef = machineDefinitions[selectedMachineIndex];
                        
                        // Kollisionsprüfung
                        bool positionOccupied = false;
                        foreach (var obj in placedObjects)
                        {
                            float distanceX = MathF.Abs(obj.Position.X - previewPosition.X);
                            float distanceZ = MathF.Abs(obj.Position.Z - previewPosition.Z);
                            
                            if (distanceX < (obj.Size.X + currentDef.Size.X) / 2 * cellSize &&
                                distanceZ < (obj.Size.Z + currentDef.Size.Z) / 2 * cellSize)
                            {
                                positionOccupied = true;
                                break;
                            }
                        }
                        
                        if (!positionOccupied)
                        {
                            // ===== MASCHINE ERSTELLEN ÜBER DEFINITION =====
                            FactoryMachine machine = currentDef.CreateMachine(previewPosition);
                            
                            if (machine != null)
                            {
                                factoryManager.AddMachine(machine);
                                
                                // Belt-Verbindungen aktualisieren
                                if (machine is ConveyorBelt)
                                {
                                    BeltConnectionHelper.UpdateAllConnections(factoryManager);
                                }
                            }
                            
                            PlacedObject newObject = new PlacedObject(
                                previewPosition,
                                currentDef.Model,
                                currentDef.Name,
                                currentDef.Size,
                                machine
                            );
                            
                            placedObjects.Add(newObject);
                            Console.WriteLine($"{currentDef.Name} platziert ({placedObjects.Count} gesamt)");
                        }
                    }
                    
                    // ===== ENTFERNEN =====
                    if (Raylib.IsMouseButtonPressed(MouseButton.Right) && showPreview)
                    {
                        for (int i = placedObjects.Count - 1; i >= 0; i--)
                        {
                            float distanceX = MathF.Abs(placedObjects[i].Position.X - previewPosition.X);
                            float distanceZ = MathF.Abs(placedObjects[i].Position.Z - previewPosition.Z);
                            
                            if (distanceX < placedObjects[i].Size.X * cellSize / 2 &&
                                distanceZ < placedObjects[i].Size.Z * cellSize / 2)
                            {
                                if (placedObjects[i].Machine != null)
                                {
                                    factoryManager.RemoveMachine(placedObjects[i].Machine);
                                    BeltConnectionHelper.UpdateAllConnections(factoryManager);
                                }
                                
                                Console.WriteLine($"{placedObjects[i].Type} entfernt");
                                placedObjects.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                
                // ===== DRAW =====
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(135, 206, 235, 255));
                
                Raylib.BeginMode3D(GlobalData.camera);
                    UI.Draw3DElements();
                    Raylib.DrawSphere(lightPosition, 0.3f, Color.Yellow);
                    
                    //Building.DrawBorderWallWithModel(Wand, 9, 1.0f); //<---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------Wand
                    Vector3 bodenPos = new Vector3(0, 0, 0);
                    Raylib.DrawModelEx(Boden, bodenPos, new Vector3(0, 1, 0), 90.0f, new Vector3(1, 1, 1), Color.White);
                    
                    factoryManager.DrawAll();
                    
                    if (showPreview && selectedMachineIndex < machineDefinitions.Count)
                    {
                        MachineDefinition currentDef = machineDefinitions[selectedMachineIndex];
                        Raylib.DrawModel(currentDef.Model, previewPosition, 1.0f, currentDef.PreviewColor);
                        Raylib.DrawCubeWires(previewPosition, currentDef.Size.X, currentDef.Size.Y, currentDef.Size.Z, Color.White);
                    }

                Raylib.EndMode3D();

                // ===== UI =====
                Raylib.DrawText("WASD: Move | Space/Shift: Up/Down | Arrows: Rotate/Zoom", 10, 10, 18, Color.Black);
                Raylib.DrawText($"TAB/1-{machineDefinitions.Count}: Select | LClick: Place | RClick: Remove", 10, 32, 18, Color.Black);
                
                if (selectedMachineIndex < machineDefinitions.Count)
                {
                    string info = $"Selected: {machineDefinitions[selectedMachineIndex].Name} | Placed: {placedObjects.Count}";
                    Raylib.DrawText(info, 10, 54, 18, Color.DarkGreen);
                }
                
                factoryManager.DrawDebugInfo(90);
                UI.DebugDataUI();
                DebugConsole.Draw();

                Raylib.EndDrawing();
            }

            // Cleanup
            Raylib.UnloadShader(shader);
            Raylib.UnloadModel(Wand);
            Raylib.UnloadModel(Boden);
            Raylib.UnloadModel(MaschineModel);
            Raylib.UnloadModel(cubeModel);
            Raylib.CloseWindow();
        }
    }
}