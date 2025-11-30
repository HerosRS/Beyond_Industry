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
        static void Main()
        {
            Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");

            // ===== KAMERA =====
            CameraController cameraController = new CameraController(0.1f, 0.8f);
            GlobalData.camera = cameraController.Camera;
            
            // ===== BELEUCHTUNG =====
            Shader shader = Raylib.LoadShader(@"..\..\..\Resources\lighting.vs", @"..\..\..\Resources\lighting.fs");
            Vector3 lightPosition = new Vector3(0.0f, 10.0f, 0.0f);
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
            var modelMap = new Dictionary<string, Model>
            {
                { "default", cubeModel },
                { "MiningDrill_Iron", IronDrillModel },
                { "MiningDrill_Copper", CopperDrillModel },
                { "Iron_Furnace", Iron_Furnace },
                { "ConveyorBelt", BeltModel }
            };
            List<MachineDefinition> machineDefinitions = MachineRegistry.LoadAllDefinitions(modelMap);
         
            // ===== SYSTEME =====
            Grid grid = new Grid();
            FactoryManager factoryManager = new FactoryManager();
            factoryManager.TotalPowerGeneration = 200f;
            PlacementSystem placementSystem = new PlacementSystem(machineDefinitions, factoryManager, GlobalData.camera);
            
            while (!Raylib.WindowShouldClose())
            {
                // ===== UPDATE =====
                DebugConsole.Update();
                factoryManager.Update(Raylib.GetFrameTime());
                
                if (!DebugConsole.IsOpen())
                {
                    // Kamera
                    cameraController.Update();
                    GlobalData.camera = cameraController.Camera;
                    
                    // Auswahl
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
                    
                    // Belt-Rotation
                    if (Raylib.IsKeyPressed(KeyboardKey.R))
                        placementSystem.RotateBelt();
                    
                    // Preview
                    placementSystem.UpdatePreview(Raylib.GetMousePosition());
                    
                    // Platzieren/Entfernen
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                        placementSystem.PlaceObject();
                    if (Raylib.IsMouseButtonPressed(MouseButton.Right))
                        placementSystem.RemoveObject();
                }
                
                // ===== DRAW =====
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(135, 206, 235, 255));
                
                Raylib.BeginMode3D(GlobalData.camera);
                    UI.Draw3DElements();
                    Raylib.DrawSphere(lightPosition, 0.3f, Color.Yellow);
                    
                    Building.DrawBorderWallWithModel(Wand, 9, 1.0f);
                    Raylib.DrawModelEx(Boden, Vector3.Zero, new Vector3(0, 1, 0), 90.0f, Vector3.One, Color.White);
                    
                    factoryManager.DrawAll();
                    placementSystem.DrawPreview();
                Raylib.EndMode3D();

                // ===== UI =====
                Raylib.DrawText("WASD: Move | Space/Shift: Up/Down | Arrows: Rotate/Zoom", 10, 10, 18, Color.Black);
                Raylib.DrawText($"TAB/1-{machineDefinitions.Count}: Select | R: Rotate Belt | LClick: Place | RClick: Remove", 10, 32, 18, Color.Black);
                Raylib.DrawText(placementSystem.GetSelectedInfo(), 10, 54, 18, Color.DarkGreen);
                
                factoryManager.DrawDebugInfo(90);
                UI.DebugDataUI();
                DebugConsole.Draw();

                Raylib.EndDrawing();
            }

            // Cleanup
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