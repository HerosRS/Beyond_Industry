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
            bool shouldRestart = true;
            
            while (shouldRestart)
            {
                shouldRestart = RunGame();
                
                if (shouldRestart)
                {
                    Console.WriteLine("[Program] Restarting for hot reload...");
                    System.Threading.Thread.Sleep(500);  // Kurze Pause
                }
            }
        }
        
        static bool RunGame()
        {
            try
            {
                Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.VSyncHint);
                Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");
                Raylib.SetTargetFPS(60);

                Console.WriteLine("[Program] Window initialized");

                CameraController cameraController = new CameraController();
                cameraController.SetBounds(-50, 50, -50, 50);  // Grid-Bounds
                

                GlobalData.camera = cameraController.Camera;
                
                // ===== SHADER =====
                Shader shader = default;
                try
                {
                    string vsPath = @"..\..\..\Resources\lighting.vs";
                    string fsPath = @"..\..\..\Resources\lighting.fs";
                    
                    shader = Raylib.LoadShader(vsPath, fsPath);
                    
                    // Watch shader files
                    HotReloadSystem.WatchFile("shader_vs", vsPath);
                    HotReloadSystem.WatchFile("shader_fs", fsPath);
                    
                    Console.WriteLine("[Program] Shader loaded with hot reload");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Program] WARNING: Could not load shader: {ex.Message}");
                }
                
                Vector3 lightPosition = new Vector3(0.0f, 6.0f, 0.0f);
                
                if (shader.Id != 0)
                {
                    Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "lightPos"),
                        new float[] { lightPosition.X, lightPosition.Y, lightPosition.Z }, ShaderUniformDataType.Vec3);
                    Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "lightColor"),
                        new float[] { 1.0f, 1.0f, 1.0f, 1.0f }, ShaderUniformDataType.Vec4);
                    Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "ambient"),
                        new float[] { 0.5f, 0.5f, 0.5f, 1.0f }, ShaderUniformDataType.Vec4);
                }

                // ===== MODELLE =====
                Console.WriteLine("[Program] Loading models...");
                
                Model cubeModel = Raylib.LoadModelFromMesh(Raylib.GenMeshCube(1.0f, 1.0f, 1.0f));
                
                // Lade Models und watch files
                Model Wand = LoadAndWatchModel("Wand", @"..\..\..\Resources\Wand.obj", cubeModel);
                Model Boden = LoadAndWatchModel("Boden", @"..\..\..\Resources\Boden.obj", cubeModel);
                Model Iron_Furnace = LoadAndWatchModel("Iron_Furnace", @"..\..\..\Resources\Iron_Furnace.obj", cubeModel);
                Model IronDrillModel = LoadAndWatchModel("IronDrill", @"..\..\..\Resources\Maschiene.obj", cubeModel);
                Model CopperDrillModel = LoadAndWatchModel("CopperDrill", @"..\..\..\Resources\Maschiene.obj", cubeModel);
                Model BeltModel = LoadAndWatchModel("Belt", @"..\..\..\Resources\Belt.obj", cubeModel);
                //Model BeltLinks = LoadAndWatchModel("BeltCurveLeft", @"..\..\..\Resources\belt_curve_left.obj", BeltModel);
                //Model BeltRechts = LoadAndWatchModel("BeltCurveRight", @"..\..\..\Resources\belt_curve_right.obj", BeltModel);

                Console.WriteLine("[Program] All models loaded");

                // Shader anwenden
                if (shader.Id != 0)
                {
                    unsafe
                    {
                        Boden.Materials[0].Shader = shader;
                        Wand.Materials[0].Shader = shader;
                        cubeModel.Materials[0].Shader = shader;
                        IronDrillModel.Materials[0].Shader = shader;
                        CopperDrillModel.Materials[0].Shader = shader;
                        BeltModel.Materials[0].Shader = shader;
                        //BeltRechts.Materials[0].Shader = shader;
                        //BeltLinks.Materials[0].Shader = shader;
                        Iron_Furnace.Materials[0].Shader = shader;
                    }
                }

                // ===== MASCHINEN-SYSTEM =====
                
                ResourceRegistry.Initialize();
                ResourceRegistry.PrintAll();
                MachineRegistry.Initialize();
                // ===== NACH DEM LADEN ALLER SYSTEME =====
                SaveLoadManager.Initialize();


                Console.WriteLine("[Program] === TESTING RESOURCE ACCESS ===");
var ironOre = ResourceRegistry.Get("IronOre");
if (ironOre != null)
{
    Console.WriteLine($"[Program] ✓ IronOre found: {ironOre.Name}, Color: RGB({ironOre.Color.R}, {ironOre.Color.G}, {ironOre.Color.B})");
}
else
{
    Console.WriteLine("[Program] ✗ ERROR: IronOre NOT FOUND!");
}

var copperOre = ResourceRegistry.Get("CopperOre");
if (copperOre != null)
{
    Console.WriteLine($"[Program] ✓ CopperOre found: {copperOre.Name}");
}
else
{
    Console.WriteLine("[Program] ✗ ERROR: CopperOre NOT FOUND!");
}

var ironPlate = ResourceRegistry.Get("IronPlate");
if (ironPlate != null)
{
    Console.WriteLine($"[Program] ✓ IronPlate found: {ironPlate.Name}");
}
else
{
    Console.WriteLine("[Program] ✗ ERROR: IronPlate NOT FOUND!");
}
Console.WriteLine("[Program] === END TEST ===");

                var modelMap = new Dictionary<string, Model>
                {
                    { "default", cubeModel },
                    { "MiningDrill_Iron", IronDrillModel },
                    { "MiningDrill_Copper", CopperDrillModel },
                    { "Iron_Furnace", Iron_Furnace },
                    { "ConveyorBelt", BeltModel },
                    { "ConveyorBelt_Straight", BeltModel },
                    //{ "ConveyorBelt_CurveLeft", BeltLinks },
                    //{ "ConveyorBelt_CurveRight", BeltRechts },
                    { "ConveyorBelt_RampUp", BeltModel },
                    { "ConveyorBelt_RampDown", BeltModel },
                };
                
                List<MachineDefinition> machineDefinitions = MachineRegistry.LoadAllDefinitions(modelMap);
             
                Grid grid = new Grid();
                FactoryManager factoryManager = new FactoryManager();
                factoryManager.TotalPowerGeneration = 200f;

                // ===== PLACEMENT SYSTEM (OHNE KAMERA PARAMETER) =====
                PlacementSystem placementSystem = new PlacementSystem(machineDefinitions, factoryManager);

                BuildMenuUI buildMenu = new BuildMenuUI(machineDefinitions, placementSystem);
                
                Console.WriteLine("[Program] Starting main loop...");
                
                bool requestRestart = false;
                
                while (!Raylib.WindowShouldClose() && !requestRestart)
                {
                    float deltaTime = Raylib.GetFrameTime();
                    
                    // ===== HOT RELOAD UPDATE =====
                    HotReloadSystem.Update(deltaTime);
                    
                    if (Raylib.IsWindowResized())
                    {
                        GlobalData.UpdateScreenSize();
                        buildMenu.UpdateLayout(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT);
                    }
                    
                    DebugConsole.Update();
                    factoryManager.Update(deltaTime);
                    buildMenu.Update();
                    
                    if (!DebugConsole.IsOpen())
                    {
                        cameraController.Update();
                        GlobalData.camera = cameraController.Camera;
                        
                        // ===== KAMERA RESET =====
                        if (Raylib.IsKeyPressed(KeyboardKey.Home))
                        {
                            cameraController.ResetCamera();
                            Console.WriteLine("[Camera] Reset to origin");
                        }
                        
                        // ===== SNAP TO CARDINAL =====
                       
                                            
                        // ===== HOT RELOAD TOGGLE =====
                        if (Raylib.IsKeyPressed(KeyboardKey.F5))
                        {
                            HotReloadSystem.Enabled = !HotReloadSystem.Enabled;
                            Console.WriteLine($"[HotReload] {(HotReloadSystem.Enabled ? "Enabled" : "Disabled")}");
                        }
                        
                        // ===== RESTART FÜR HOT RELOAD =====
                        if (Raylib.IsKeyPressed(KeyboardKey.F6) && HotReloadSystem.NeedsRestart)
                        {
                            Console.WriteLine("[HotReload] Restarting to apply changes...");
                            requestRestart = true;
                            break;
                        }

                    // ===== LAYER CONTROLS =====
                        if (Raylib.IsKeyPressed(KeyboardKey.KpAdd) || Raylib.IsKeyPressed(KeyboardKey.Equal))
                        {
                            placementSystem.IncreaseLayer();
                        }
                        if (Raylib.IsKeyPressed(KeyboardKey.KpSubtract) || Raylib.IsKeyPressed(KeyboardKey.Minus))
                        {
                            placementSystem.DecreaseLayer();
                        }
                        if (Raylib.IsKeyPressed(KeyboardKey.KpMultiply) || Raylib.IsKeyPressed(KeyboardKey.Zero))
                        {
                            placementSystem.ResetLayer();
                        }
                        
                        // ===== AUTO-DETECT TOGGLE =====
                        if (Raylib.IsKeyPressed(KeyboardKey.H))
                        {
                            placementSystem.AutoDetectHeight = !placementSystem.AutoDetectHeight;
                            Console.WriteLine($"[Placement] Auto-Detect Height: {placementSystem.AutoDetectHeight}");
                        }

                        // ===== PLACEMENT SYSTEM INPUT =====
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
                        
                        if (Raylib.IsKeyPressed(KeyboardKey.R))
                            placementSystem.RotateBelt();
                        
                        if (Raylib.IsKeyPressed(KeyboardKey.F8))
                            {
                                SaveLoadManager.SaveGame("quicksave", factoryManager, cameraController);
                            }
                            
                            if (Raylib.IsKeyPressed(KeyboardKey.F9))
                            {
                                SaveLoadManager.LoadGame("quicksave", factoryManager, cameraController, modelMap);
                            }


                        placementSystem.UpdatePreview(Raylib.GetMousePosition());
                        
                        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                        {
                            if (!buildMenu.IsMouseOverUI())
                            {
                                if (!IsAnyButtonHovered(factoryManager))
                                {
                                    placementSystem.PlaceObject();
                                }
                                else
                                {
                                    factoryManager.HandleMachineClicks(Raylib.GetMousePosition());
                                }
                            }
                        }
                        
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
                        Raylib.DrawModelEx(Boden, Vector3.Zero, new Vector3(0, 1, 0), 90.0f, Vector3.One, Color.White);
                        factoryManager.DrawAll();
                        placementSystem.DrawPreview();
                    Raylib.EndMode3D();

                    Raylib.DrawText("WASD: Move | F5: Hot Reload Toggle | F6: Apply Reload (Restart)", 10, 10, 18, Color.Black);
                    Raylib.DrawText("LClick: Place | RClick: Remove | R: Rotate Belt", 10, 32, 18, Color.Black);
                    
                    Raylib.DrawText($"Layer: {placementSystem.CurrentLayer} | +/-: Change | 0: Reset | H: Auto-Detect", 
    10, 54, 18, Color.Black);
    
                    // ===== HOT RELOAD STATUS =====
                    string hotReloadStatus = HotReloadSystem.Enabled ? "ON" : "OFF";
                    Color hotReloadColor = HotReloadSystem.Enabled ? Color.Green : Color.Red;
                    Raylib.DrawText($"Hot Reload: {hotReloadStatus}", GlobalData.SCREEN_WIDTH - 150, 10, 18, hotReloadColor);
                    
                    // ===== RELOAD NOTIFICATION =====
                    if (HotReloadSystem.NeedsRestart)
                    {
                        int centerX = GlobalData.SCREEN_WIDTH / 2;
                        int centerY = 100;
                        
                        string message = "CHANGES DETECTED! Press F6 to reload";
                        int textWidth = Raylib.MeasureText(message, 20);
                        
                        Raylib.DrawRectangle(centerX - textWidth/2 - 20, centerY - 15, textWidth + 40, 50, new Color(0, 0, 0, 200));
                        Raylib.DrawText(message, centerX - textWidth/2, centerY, 20, Color.Yellow);
                        
                        var changedFiles = HotReloadSystem.GetChangedFiles();
                        string filesText = $"Changed: {string.Join(", ", changedFiles)}";
                        int filesWidth = Raylib.MeasureText(filesText, 14);
                        Raylib.DrawText(filesText, centerX - filesWidth/2, centerY + 25, 14, Color.White);
                    }
                    
                    factoryManager.DrawDebugInfo(60);
                    UI.MainUI.DebugDataUI();
                    buildMenu.Draw(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT);
                    DebugConsole.Draw();

                    Raylib.EndDrawing();
                }

                Console.WriteLine("[Program] Cleaning up...");

                // Cleanup
                HotReloadSystem.Cleanup();
                buildMenu.Unload();
                if (shader.Id != 0)
                    Raylib.UnloadShader(shader);
                Raylib.UnloadModel(Wand);
                Raylib.UnloadModel(Boden);
                Raylib.UnloadModel(Iron_Furnace);
                Raylib.UnloadModel(IronDrillModel);
                Raylib.UnloadModel(CopperDrillModel);
                Raylib.UnloadModel(BeltModel);
                //Raylib.UnloadModel(BeltLinks);
                //Raylib.UnloadModel(BeltRechts);
                Raylib.UnloadModel(cubeModel);
                Raylib.CloseWindow();
                
                HotReloadSystem.ResetChanges();
                
                return requestRestart;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Program] FATAL ERROR: {ex.Message}");
                Console.WriteLine($"[Program] Stack Trace: {ex.StackTrace}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return false;
            }
        }

        static bool IsAnyButtonHovered(FactoryManager manager)
        {
            foreach (var machine in manager.GetAllMachines())
            {
                if (machine.IsButtonHovered())
                    return true;
            }
            return false;
        }
        
        // ===== HELPER: MODEL LADEN UND WATCHEN =====
        static Model LoadAndWatchModel(string key, string path, Model fallback)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var model = Raylib.LoadModel(path);
                    HotReloadSystem.WatchFile(key, path);
                    Console.WriteLine($"[Program] Loaded & watching: {key}");
                    return model;
                }
                else
                {
                    Console.WriteLine($"[Program] WARNING: File not found: {path}, using fallback");
                    return fallback;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Program] ERROR loading {path}: {ex.Message}");
                return fallback;
            }
        }
    }
}