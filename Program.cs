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
            GlobalData.camera.FovY = 15.0f;
            GlobalData.camera.Projection = CameraProjection.Perspective;     // Perspective statt Orthographic
            
            // ===== BELEUCHTUNG SETUP =====
            Shader shader = Raylib.LoadShader(@"C:\Users\ltheis\Documents\GitHub\Beyond_Industry\Resources\lighting.vs", @"C:\Users\ltheis\Documents\GitHub\Beyond_Industry\Resources\lighting.fs");

// Modelle laden
Model Wand = Raylib.LoadModel(@"Resources\Wand.glb");
Model Boden = Raylib.LoadModel(@"Resources\Boden.glb");
Model cubeModel = Raylib.LoadModelFromMesh(Raylib.GenMeshCube(1.0f, 1.0f, 1.0f));

// Shader auf Modelle anwenden
unsafe
{
    cubeModel.Materials[0].Shader = shader;
    
    if (Wand.MeshCount > 0)
        Wand.Materials[0].Shader = shader;
    if (Boden.MeshCount > 0)
        Boden.Materials[0].Shader = shader;
}

// WICHTIG: Licht-Werte MANUELL setzen!
Vector3 lightPosition = new Vector3(0.0f, 5.0f, 0.0f);

// Lichtposition setzen
Raylib.SetShaderValue(
    shader,
    Raylib.GetShaderLocation(shader, "lightPos"),
    new float[] { lightPosition.X, lightPosition.Y, lightPosition.Z },
    ShaderUniformDataType.Vec3
);

// Lichtfarbe setzen (Weiß)
Raylib.SetShaderValue(
    shader,
    Raylib.GetShaderLocation(shader, "lightColor"),
    new float[] { 1.0f, 1.0f, 1.0f, 1.0f },
    ShaderUniformDataType.Vec4
);

// Umgebungslicht setzen
Raylib.SetShaderValue(
    shader,
    Raylib.GetShaderLocation(shader, "ambient"),
    new float[] { 0.2f, 0.2f, 0.2f, 1.0f },
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
                    // Bewege Licht mit Pfeiltasten
                    if (Raylib.IsKeyDown(KeyboardKey.Up))
                        lightPosition.Z -= 0.1f;
                    if (Raylib.IsKeyDown(KeyboardKey.Down))
                        lightPosition.Z += 0.1f;
                    if (Raylib.IsKeyDown(KeyboardKey.Left))
                        lightPosition.X -= 0.1f;
                    if (Raylib.IsKeyDown(KeyboardKey.Right))
                        lightPosition.X += 0.1f;
                    if (Raylib.IsKeyDown(KeyboardKey.Q))
                        lightPosition.Y += 0.1f;
                    if (Raylib.IsKeyDown(KeyboardKey.E))
                        lightPosition.Y -= 0.1f;
                    
                    // Update Shader mit neuer Licht-Position
                    Raylib.SetShaderValue(
                        shader,
                        Raylib.GetShaderLocation(shader, "lightPos"),
                        new float[] { lightPosition.X, lightPosition.Y, lightPosition.Z },
                        ShaderUniformDataType.Vec3
                    );

                    // Maus-Klick Detection
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        // Ray von Mausposition in 3D-Welt
                        Ray ray = Raylib.GetMouseRay(Raylib.GetMousePosition(), GlobalData.camera);
                        
                        // Cube ist bei Vector3.Zero mit Größe 1.0f
                        BoundingBox cubeBox = new BoundingBox(
                            new Vector3(-0.5f, -0.5f, -0.5f),  // min
                            new Vector3(0.5f, 0.5f, 0.5f)      // max
                        );
                        
                        // Prüfe ob Ray den Cube trifft
                        RayCollision collision = Raylib.GetRayCollisionBox(ray, cubeBox);
                        
                        if (collision.Hit)
                        {
                            Console.WriteLine("Cube wurde angeklickt!");
                        }
                    }
                }
                
                // ===== DRAW =====
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color(38, 40, 43, 255));
                
                Raylib.BeginMode3D(GlobalData.camera);
                    UI.Draw3DElements();
                    
                    // Lichtquelle an dynamischer Position zeichnen
                    Raylib.DrawSphere(lightPosition, 0.3f, Color.Yellow);
                    
                    // Cube zeichnen
                    Raylib.DrawModel(cubeModel, Vector3.Zero, 1.0f, Color.Red);

                    // Wenn Modelle geladen, zeichne sie
                    if (Wand.MeshCount > 0)
                    {
                        Building.DrawBorderWallWithModel(Wand, 9, 1.0f);
                        
                        Vector3 BodenPos = new Vector3(1, 0, 0);
                        Raylib.DrawModel(Boden, BodenPos, 1.0f, Color.White);
                    }
                    
                Raylib.EndMode3D();

                // UI
                Raylib.DrawText("Use Mouse to rotate camera", 10, 10, 20, Color.Black);
                Raylib.DrawText("Arrow Keys + Q/E to move light", 10, 35, 20, Color.Black);
                
                UI.DebugDataUI();
                DebugConsole.Draw();

                Raylib.EndDrawing();
            }

            // Cleanup
            Raylib.UnloadShader(shader);
            Raylib.UnloadModel(Wand);
            Raylib.UnloadModel(Boden);
            Raylib.UnloadModel(cubeModel);
            Raylib.CloseWindow();
        }
    }
}