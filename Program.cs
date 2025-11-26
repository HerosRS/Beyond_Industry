using Raylib_cs;
using BeyondIndustry.Utils;
using BeyondIndustry.Data;
using BeyondIndustry.DebugView;
using BeyondIndustry.Debug;
using System.Numerics;
using System;
using System.Threading.Tasks.Dataflow;
using System.Runtime.CompilerServices;

namespace BeyondIndustry
{
    class Program
    {
        static void Main()
        {
            Raylib.InitWindow(GlobalData.SCREEN_WIDTH, GlobalData.SCREEN_HEIGHT, "Beyond Industry");

            // ===== KORRIGIERTE KAMERA (wie im Raylib Beispiel!) =====
            GlobalData.camera.Position = new Vector3(20.0f, 20.0f, 20.0f);  // Diagonal über der Szene
            GlobalData.camera.Target = new Vector3(0.0f, 0.0f, 0.0f);       // Schaut auf Ursprung
            GlobalData.camera.Up = new Vector3(0.0f, 1.0f, 0.0f);           // Y ist OBEN!
            GlobalData.camera.FovY = 50.0f;
            GlobalData.camera.Projection = CameraProjection.Perspective;     // Perspective statt Orthographic
            
            // ===== BELEUCHTUNG SETUP =====
            Shader shader = Raylib.LoadShader((string)null, (string)null);

            // Modelle laden
            Model Wand = Raylib.LoadModel(@"D:\Beyond_Industry\Resources\Wand.glb");
            Model Boden = Raylib.LoadModel(@"D:\Beyond_Industry\Resources\Boden.glb");

            // Shader auf Modelle anwenden
            unsafe
            {
                if (Wand.MeshCount > 0)
                    Wand.Materials[0].Shader = shader;
                if (Boden.MeshCount > 0)
                    Boden.Materials[0].Shader = shader;
            }

            // Licht erstellen
            Light mainLight = Rlights.CreateLight(
                LightType.Directional,
                new Vector3(10, 20, 10),
                Vector3.Zero,
                Color.White,
                shader
            );

            // Umgebungslicht
            Raylib.SetShaderValue(
                shader,
                Raylib.GetShaderLocation(shader, "ambient"),
                new float[] { 0.4f, 0.4f, 0.4f, 1.0f },
                ShaderUniformDataType.Vec4
            );

            // Grid
            Grid grid = new Grid();

            while (!Raylib.WindowShouldClose())
            {
                // ===== UPDATE =====
                DebugConsole.Update();
                
                if (!DebugConsole.IsOpen())
                {
                    // Kamera mit UpdateCamera (wie im Raylib Beispiel)
                    Raylib.UpdateCamera(ref GlobalData.camera, CameraMode.Free);
                    
                    // ODER manuelle Steuerung:
                    /*
                    float cameraSpeed = 0.5f;
                    if (Raylib.IsKeyDown(KeyboardKey.W))
                        GlobalData.camera.Position.Z -= cameraSpeed;
                    if (Raylib.IsKeyDown(KeyboardKey.S))
                        GlobalData.camera.Position.Z += cameraSpeed;
                    */
                }
                
                // ===== DRAW =====
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.RayWhite);  // Hellerer Hintergrund zum Testen
                
                Raylib.BeginMode3D(GlobalData.camera);
                    UI.Draw3DElements();
                    // Referenz-Grid am Boden (Y=0)
                    //Raylib.DrawGrid(10, 1.0f);

                    // Wenn Modelle geladen, zeichne sie
                    if (Wand.MeshCount > 0)
                    {
                        Building.DrawBorderWallWithModel(Wand, 9, 1.0f);
                        
                        Vector3 BodenPos = new Vector3(1, 0, 0);
                        Raylib.DrawModel(Boden, BodenPos, 1.0f, Color.White);
                        
                      
                        // Mit Wire-Overlay zum Debuggen
                        //Raylib.DrawModelWires(Wand, wandPos, 1.0f, Color.Black);
                    }
                    
                  
                Raylib.EndMode3D();

                // UI
                Raylib.DrawText("Use Mouse to rotate camera", 10, 10, 20, Color.Black);
                
                UI.DebugDataUI();
                DebugConsole.Draw();

                Raylib.EndDrawing();
            }

            // Cleanup
            Raylib.UnloadShader(shader);
            Raylib.UnloadModel(Wand);
            Raylib.UnloadModel(Boden);
            Raylib.CloseWindow();
        }
    }
}