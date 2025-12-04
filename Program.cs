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
        cameraController.SetBounds(-50, 50, -50, 50);
        GlobalData.camera = cameraController.Camera;
        
        // ===== SHADER =====
        Shader? shader = LoadShader();
        
        // ===== MODELS - NUR EINE ZEILE! =====
        ModelRegistry.LoadAllModels(shader);
        
        // ===== MASCHINEN-SYSTEM =====
        ResourceRegistry.Initialize();
        ResourceRegistry.PrintAll();
        MachineRegistry.Initialize();
        SaveLoadManager.Initialize();
        
        // ===== HOLE MODELS FÜR FACTORIES =====
        var modelMap = ModelRegistry.GetAllModels();
        List<MachineDefinition> machineDefinitions = MachineRegistry.LoadAllDefinitions(modelMap);
        
        Grid grid = new Grid();
        FactoryManager factoryManager = new FactoryManager();
        factoryManager.TotalPowerGeneration = 200f;
        
        PlacementSystem placementSystem = new PlacementSystem(machineDefinitions, factoryManager);
        BuildMenuUI buildMenu = new BuildMenuUI(machineDefinitions, placementSystem);
        
        Console.WriteLine("[Program] Starting main loop...");
        
        bool requestRestart = false;
        
        while (!Raylib.WindowShouldClose() && !requestRestart)
        {
            float deltaTime = Raylib.GetFrameTime();
            
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
                
                if (Raylib.IsKeyPressed(KeyboardKey.Home))
                {
                    cameraController.ResetCamera();
                    Console.WriteLine("[Camera] Reset to origin");
                }
                
                if (Raylib.IsKeyPressed(KeyboardKey.F5))
                {
                    HotReloadSystem.Enabled = !HotReloadSystem.Enabled;
                    Console.WriteLine($"[HotReload] {(HotReloadSystem.Enabled ? "Enabled" : "Disabled")}");
                }
                
                if (Raylib.IsKeyPressed(KeyboardKey.F6) && HotReloadSystem.NeedsRestart)
                {
                    Console.WriteLine("[HotReload] Restarting to apply changes...");
                    requestRestart = true;
                    break;
                }
                
                // Layer Controls
                if (Raylib.IsKeyPressed(KeyboardKey.KpAdd) || Raylib.IsKeyPressed(KeyboardKey.Equal))
                    placementSystem.IncreaseLayer();
                if (Raylib.IsKeyPressed(KeyboardKey.KpSubtract) || Raylib.IsKeyPressed(KeyboardKey.Minus))
                    placementSystem.DecreaseLayer();
                if (Raylib.IsKeyPressed(KeyboardKey.KpMultiply) || Raylib.IsKeyPressed(KeyboardKey.Zero))
                    placementSystem.ResetLayer();
                
                if (Raylib.IsKeyPressed(KeyboardKey.H))
                {
                    placementSystem.AutoDetectHeight = !placementSystem.AutoDetectHeight;
                    Console.WriteLine($"[Placement] Auto-Detect Height: {placementSystem.AutoDetectHeight}");
                }
                
                // Machine Selection
                if (Raylib.IsKeyPressed(KeyboardKey.One)) placementSystem.SelectIndex(0);
                if (Raylib.IsKeyPressed(KeyboardKey.Two)) placementSystem.SelectIndex(1);
                if (Raylib.IsKeyPressed(KeyboardKey.Three)) placementSystem.SelectIndex(2);
                if (Raylib.IsKeyPressed(KeyboardKey.Four)) placementSystem.SelectIndex(3);
                if (Raylib.IsKeyPressed(KeyboardKey.Five)) placementSystem.SelectIndex(4);
                if (Raylib.IsKeyPressed(KeyboardKey.Six)) placementSystem.SelectIndex(5);
                if (Raylib.IsKeyPressed(KeyboardKey.Seven)) placementSystem.SelectIndex(6);
                if (Raylib.IsKeyPressed(KeyboardKey.Eight)) placementSystem.SelectIndex(7);
                if (Raylib.IsKeyPressed(KeyboardKey.Nine)) placementSystem.SelectIndex(8);
                
                if (Raylib.IsKeyPressed(KeyboardKey.R))
                    placementSystem.RotateBelt();
                
                // Save/Load
                if (Raylib.IsKeyPressed(KeyboardKey.F8))
                {
                    SaveLoadManager.SaveGame("quicksave", factoryManager, cameraController);
                }
                
                if (Raylib.IsKeyPressed(KeyboardKey.F9))
                {
                    if (SaveLoadManager.LoadGame("quicksave", factoryManager, cameraController, modelMap))
                    {
                        Console.WriteLine($"[Game] Loaded from quicksave");
                        BeltConnectionHelper.UpdateAllConnections(factoryManager);
                    }
                    else
                    {
                        Console.WriteLine($"[Game] Load failed!");
                    }
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
                
                // Zeichne Boden
                Model bodenModel = ModelRegistry.GetModel("Boden");
                Raylib.DrawModelEx(bodenModel, Vector3.Zero, new Vector3(0, 1, 0), 90.0f, Vector3.One, Color.Green);
                
                factoryManager.DrawAll();
                placementSystem.DrawPreview();
            Raylib.EndMode3D();

            Raylib.DrawText("WASD: Move | F5: Hot Reload Toggle | F6: Apply Reload (Restart)", 10, 10, 18, Color.Black);
            Raylib.DrawText("LClick: Place | RClick: Remove | R: Rotate Belt", 10, 32, 18, Color.Black);
            Raylib.DrawText($"Layer: {placementSystem.CurrentLayer} | +/-: Change | 0: Reset | H: Auto-Detect", 10, 54, 18, Color.Black);
            
            string hotReloadStatus = HotReloadSystem.Enabled ? "ON" : "OFF";
            Color hotReloadColor = HotReloadSystem.Enabled ? Color.Green : Color.Red;
            Raylib.DrawText($"Hot Reload: {hotReloadStatus}", GlobalData.SCREEN_WIDTH - 150, 10, 18, hotReloadColor);
            
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
        
        if (shader.HasValue && shader.Value.Id != 0)
            Raylib.UnloadShader(shader.Value);
        
        ModelRegistry.UnloadAll();
        
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

static Shader? LoadShader()
{
    try
    {
        string vsPath = @"..\..\..\Resources\lighting.vs";
        string fsPath = @"..\..\..\Resources\lighting.fs";
        
        Shader shader = Raylib.LoadShader(vsPath, fsPath);
        
        HotReloadSystem.WatchFile("shader_vs", vsPath);
        HotReloadSystem.WatchFile("shader_fs", fsPath);
        
        Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "ambient"),
            new float[] { 0.5f, 0.5f, 0.5f, 1.0f }, ShaderUniformDataType.Vec4);
        
        Console.WriteLine("[Program] Shader loaded with hot reload");
        return shader;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Program] WARNING: Could not load shader: {ex.Message}");
        return null;
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