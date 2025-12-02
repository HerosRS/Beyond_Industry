using Raylib_cs;
using BeyondIndustry.Utils;
using BeyondIndustry.Data;
using BeyondIndustry.UI;
using BeyondIndustry.Debug;
using BeyondIndustry.Factory;
using BeyondIndustry.Factory.Resources;
using System.Numerics;
using System;
using System.Collections.Generic;

namespace BeyondIndustry
{
    class Program
    {
        static void Main()
        {
            // ===== FENSTER MIT RESIZE-SUPPORT =====
            Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
            Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");
            Raylib.SetTargetFPS(60);

            // ===== KAMERA =====
            CameraController cameraController = new CameraController(0.1f, 0.8f);
            GlobalData.camera = cameraController.Camera;
            
            // ===== BELEUCHTUNG =====
            Shader shader = Raylib.LoadShader(@"..\..\..\Resources\lighting.vs", @"..\..\..\Resources\lighting.fs");
            Vector3 lightPosition = new Vector3(0.0f, 6.0f, 0.0f);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "lightPos"),
                new float[] { lightPosition.X, lightPosition.Y, lightPosition.Z }, ShaderUniformDataType.Vec3);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "lightColor"),
                new float[] { 1.0f, 1.0f, 1.0f, 1.0f }, ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "ambient"),
                new float[] { 0.5f, 0.5f, 0.5f, 1.0f }, ShaderUniformDataType.Vec4);

            // ===== MODELLE LADEN =====
            Model Wand = Raylib.LoadModel(@"..\..\..\Resources\Wand.obj");
            Model Boden = Raylib.LoadModel(@"..\..\..\Resources\Boden.obj");
            Model Iron_Furnace = Raylib.LoadModel(@"..\..\..\Resources\Iron_Furnace.obj");
            Model IronDrillModel = Raylib.LoadModel(@"..\..\..\Resources\Maschiene.obj");
            Model CopperDrillModel = Raylib.LoadModel(@"..\..\..\Resources\Maschiene.obj");
            Model BeltModel = Raylib.LoadModel(@"..\..\..\Resources\Belt.obj");
            Model cubeModel = Raylib.LoadModelFromMesh(Raylib.GenMeshCube(1.0f, 1.0f, 1.0f));

            // Shader anwenden
            unsafe
            {
                Boden.Materials[0].Shader = shader;
                Wand.Materials[0].Shader = shader;
                cubeModel.Materials[0].Shader = shader;
                IronDrillModel.Materials[0].Shader = shader;
                CopperDrillModel.Materials[0].Shader = shader;
                BeltModel.Materials[0].Shader = shader;
                Iron_Furnace.Materials[0].Shader = shader;
            }

            // ===== MASCHINEN-SYSTEM =====
            MachineRegistry.Initialize();
            ResourceRegistry.Initialize();
            ResourceRegistry.PrintAll();
            
            var modelMap = new Dictionary<string, Model>
            {
                { "default", cubeModel },
                { "MiningDrill_Iron", IronDrillModel },
                { "MiningDrill_Copper", CopperDrillModel },
                { "Iron_Furnace", Iron_Furnace },
                { "ConveyorBelt", BeltModel },
                
                // Belt-Typen (nutze vorerst gleiches Model)
                { "ConveyorBelt_Straight", BeltModel },
                { "ConveyorBelt_CurveLeft", BeltModel },
                { "ConveyorBelt_CurveRight", BeltModel },
                { "ConveyorBelt_RampUp", BeltModel },
                { "ConveyorBelt_RampDown", BeltModel },
            };
            
            List<MachineDefinition> machineDefinitions = MachineRegistry.LoadAllDefinitions(modelMap);
         
            // ===== SYSTEME =====
            Grid grid = new Grid();
            FactoryManager factoryManager = new FactoryManager();
            factoryManager.TotalPowerGeneration = 200f;
            PlacementSystem placementSystem = new PlacementSystem(machineDefinitions, factoryManager, GlobalData.camera);
            
            // ===== NEU: BUILD MENU UI =====
            BuildMenuUI buildMenu = new BuildMenuUI(machineDefinitions, placementSystem);
            
            while (!Raylib.WindowShouldClose())
            {
                // ===== FENSTER-RESIZE HANDLING =====
                if (Raylib.IsWindowResized())
                {
                    GlobalData.UpdateScreenSize();
                    buildMenu.UpdateLayout(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT);
                    Console.WriteLine($"[Window] Resized to {GlobalData.SCREEN_WIDTH}x{GlobalData.SCREEN_HEIGHT}");
                }
                
                // ===== UPDATE =====
                DebugConsole.Update();
                factoryManager.Update(Raylib.GetFrameTime());
                buildMenu.Update();
                
                if (!DebugConsole.IsOpen())
                {
                    // Kamera
                    cameraController.Update();
                    GlobalData.camera = cameraController.Camera;
                    
                    // Auswahl mit Hotkeys
                    if (Raylib.IsKeyPressed(KeyboardKey.Tab))
                        placementSystem.SelectNext();
                    if (Raylib.IsKeyPressed(KeyboardKey.One))
                        placementSystem.SelectIndex(0);
                    if (Raylib.IsKeyPressed(KeyboardKey.Two))
                        placementSystem.SelectIndex(1);
                    if (Raylib.IsKeyPressed(KeyboardKey.Three))
                        placementSystem.SelectIndex(2);
                    if (Raylib.IsKeyPressed(KeyboardKey.Four))
                        placementSystem.SelectIndex(3);
                    if (Raylib.IsKeyPressed(KeyboardKey.Five))
                        placementSystem.SelectIndex(4);
                    if (Raylib.IsKeyPressed(KeyboardKey.Six))
                        placementSystem.SelectIndex(5);
                    if (Raylib.IsKeyPressed(KeyboardKey.Seven))
                        placementSystem.SelectIndex(6);
                    if (Raylib.IsKeyPressed(KeyboardKey.Eight))
                        placementSystem.SelectIndex(7);
                    if (Raylib.IsKeyPressed(KeyboardKey.Nine))
                        placementSystem.SelectIndex(8);
                    
                    // Belt-Rotation
                    if (Raylib.IsKeyPressed(KeyboardKey.R))
                        placementSystem.RotateBelt();
                    
                    // Preview
                    placementSystem.UpdatePreview(Raylib.GetMousePosition());
                    
                    // ===== CLICK HANDLING (NUR WENN NICHT ÜBER UI) =====
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        // Prüfe zuerst ob über Build-Menu geklickt wurde
                        if (!buildMenu.IsMouseOverUI())
                        {
                            // Dann prüfe ob Maschinen-Button geklickt wurde
                            if (!IsAnyButtonHovered(factoryManager))
                            {
                                placementSystem.PlaceObject();
                            }
                            else
                            {
                                factoryManager.HandleMachineClicks();
                            }
                        }
                    }
                    
                    // Entfernen
                    if (Raylib.IsMouseButtonPressed(MouseButton.Right))
                    {
                        if (!buildMenu.IsMouseOverUI())
                        {
                            placementSystem.RemoveObject();
                        }
                    }
                }
                
                // ===== DRAW =====
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(135, 206, 235, 255));
                
                Raylib.BeginMode3D(GlobalData.camera);
                    UI.MainUI.Draw3DElements();
                    Raylib.DrawSphere(lightPosition, 0.3f, Color.Yellow);
                    
                    //Building.DrawBorderWallWithModel(Wand, 9, 1.0f);
                    Raylib.DrawModelEx(Boden, Vector3.Zero, new Vector3(0, 1, 0), 90.0f, Vector3.One, Color.White);
                    
                    factoryManager.DrawAll();
                    placementSystem.DrawPreview();
                Raylib.EndMode3D();

                // ===== UI =====
                Raylib.DrawText("WASD: Move | Space/Shift: Up/Down | Arrows: Rotate/Zoom", 10, 10, 18, Color.Black);
                Raylib.DrawText("LClick: Place/Toggle | RClick: Remove | R: Rotate Belt", 10, 32, 18, Color.Black);
                
                factoryManager.DrawDebugInfo(60);
                UI.MainUI.DebugDataUI();
                
                // ===== NEU: BUILD MENU ZEICHNEN =====
                buildMenu.Draw(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT);
                
                DebugConsole.Draw();

                Raylib.EndDrawing();
            }

            // ===== HELPER FUNKTION =====
            static bool IsAnyButtonHovered(FactoryManager manager)
            {
                foreach (var machine in manager.GetAllMachines())
                {
                    if (machine.IsButtonHovered())
                        return true;
                }
                return false;
            }

            // Cleanup
            buildMenu.Unload();
            Raylib.UnloadShader(shader);
            Raylib.UnloadModel(Wand);
            Raylib.UnloadModel(Boden);
            Raylib.UnloadModel(Iron_Furnace);
            Raylib.UnloadModel(IronDrillModel);
            Raylib.UnloadModel(CopperDrillModel);
            Raylib.UnloadModel(BeltModel);
            Raylib.UnloadModel(cubeModel);
            Raylib.CloseWindow();
        }
    }
}