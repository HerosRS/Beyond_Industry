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
            GlobalData.camera.Position = new Vector3(22.0f, 20.0f, 22.0f);  // Diagonal über der Szene
            GlobalData.camera.Target = new Vector3(0.0f, 0.0f, 0.0f);       // Schaut auf Ursprung
            GlobalData.camera.Up = new Vector3(0.0f, 1.0f, 0.0f);           // Y ist OBEN!
            GlobalData.camera.FovY = 15.0f;
            GlobalData.camera.Projection = CameraProjection.Perspective;     // Perspective statt Orthographic
            
            // ===== BELEUCHTUNG SETUP =====
            Shader shader = Raylib.LoadShader(@"..\..\..\Resources\lighting.vs", @"..\..\..\Resources\lighting.fs");

            Model Wand = Raylib.LoadModel(@"..\..\..\Resources\Wand.obj");
            Model Boden = Raylib.LoadModel(@"..\..\..\Resources\Boden.obj");
            Model cubeModel = Raylib.LoadModelFromMesh(Raylib.GenMeshCube(1.0f, 1.0f, 1.0f));

            // Shader auf Modelle anwenden
            unsafe
            {
                Boden.Materials[0].Shader = shader;
                Wand.Materials[0].Shader = shader;
                cubeModel.Materials[0].Shader = shader;
            }

            // WICHTIG: Licht-Werte MANUELL setzen!
            Vector3 lightPosition = new Vector3(0.0f, 10.0f, 0.0f);

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
                new float[] { 0.5f, 0.5f, 0.5f, 1.0f },
                ShaderUniformDataType.Vec4
            );

            // Grid
            Grid grid = new Grid();
            
            // Kamera-Bewegungsgeschwindigkeit
            float cameraMoveSpeed = 0.003f;
            float cameraRotationSpeed = 1.0f;
            
            while (!Raylib.WindowShouldClose())
            {
                // ===== UPDATE =====
                DebugConsole.Update();
                
                if (!DebugConsole.IsOpen())
                {
                    // Kamera-Bewegung mit Pfeiltasten (bewegt Position relativ zum Target)
                    Vector3 forward = Vector3.Normalize(GlobalData.camera.Target - GlobalData.camera.Position);
                    Vector3 right = Vector3.Normalize(Vector3.Cross(forward, GlobalData.camera.Up));
                    
                    // Vorwärts/Rückwärts
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
                    
                    // Links/Rechts
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
                    
                    // Hoch/Runter
                    if (Raylib.IsKeyDown(KeyboardKey.Q))
                    {
                        GlobalData.camera.Position.Y += cameraMoveSpeed;
                        GlobalData.camera.Target.Y += cameraMoveSpeed;
                    }
                    if (Raylib.IsKeyDown(KeyboardKey.E))
                    {
                        GlobalData.camera.Position.Y -= cameraMoveSpeed;
                        GlobalData.camera.Target.Y -= cameraMoveSpeed;
                    }
                    
                    // Kamera rotieren mit Pfeiltasten
                    if (Raylib.IsKeyDown(KeyboardKey.Left))
                    {
                        // Rotiere um Y-Achse (links)
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
                        // Rotiere um Y-Achse (rechts)
                        Vector3 direction = GlobalData.camera.Position - GlobalData.camera.Target;
                        float angle = -cameraRotationSpeed * Raylib.GetFrameTime();
                        
                        float cosAngle = MathF.Cos(angle);
                        float sinAngle = MathF.Sin(angle);
                        
                        float newX = direction.X * cosAngle - direction.Z * sinAngle;
                        float newZ = direction.X * sinAngle + direction.Z * cosAngle;
                        
                        GlobalData.camera.Position = GlobalData.camera.Target + new Vector3(newX, direction.Y, newZ);
                    }
                    
                    // Zoom mit Up/Down
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
                    
                    // Lichtquelle (fix)
                    Raylib.DrawSphere(lightPosition, 0.3f, Color.Yellow);
                    
                    // Cube zeichnen
                    //Raylib.DrawModel(cubeModel, Vector3.Zero, 1.0f, Color.Red);

                    Building.DrawBorderWallWithModel(Wand, 9, 1.0f);
                    
                    Vector3 bodenPos = new Vector3(0, 0, 0);
                    Vector3 rotationAxis = new Vector3(0, 1, 0);  // Y-Achse (vertikal)
                    float rotationAngle = 90.0f;                   // 90 Grad
                    Raylib.DrawModelEx(Boden, bodenPos, rotationAxis, rotationAngle, new Vector3(1, 1, 1), Color.White);
                

                Raylib.EndMode3D();

                // UI
                Raylib.DrawText("WASD: Move | Q/E: Up/Down | Arrows: Rotate/Zoom", 10, 10, 20, Color.White);
                
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