using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry
{
    public class CameraController
    {
        public Camera3D Camera;
        public float MoveSpeed { get; set; }
        public float RotationSpeed { get; set; }
        
        public CameraController(float moveSpeed = 0.1f, float rotationSpeed = 0.8f)
        {
            Camera = new Camera3D();
            Camera.Position = new Vector3(22.0f, 20.0f, 22.0f);
            Camera.Target = new Vector3(0.0f, 0.0f, 0.0f);
            Camera.Up = new Vector3(0.0f, 1.0f, 0.0f);
            Camera.FovY = 15.0f;
            Camera.Projection = CameraProjection.Perspective;
            
            MoveSpeed = moveSpeed;
            RotationSpeed = rotationSpeed;
        }
        
        public void Update()
        {
            Vector3 forward = Vector3.Normalize(Camera.Target - Camera.Position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Camera.Up));
            
            // Bewegung
            if (Raylib.IsKeyDown(KeyboardKey.W))
            {
                Camera.Position += forward * MoveSpeed;
                Camera.Target += forward * MoveSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.S))
            {
                Camera.Position -= forward * MoveSpeed;
                Camera.Target -= forward * MoveSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                Camera.Position -= right * MoveSpeed;
                Camera.Target -= right * MoveSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                Camera.Position += right * MoveSpeed;
                Camera.Target += right * MoveSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.Space))
            {
                Camera.Position.Y += MoveSpeed;
                Camera.Target.Y += MoveSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.LeftShift))
            {
                Camera.Position.Y -= MoveSpeed;
                Camera.Target.Y -= MoveSpeed;
            }
            
            // Rotation
            if (Raylib.IsKeyDown(KeyboardKey.Left))
            {
                Vector3 direction = Camera.Position - Camera.Target;
                float angle = RotationSpeed * Raylib.GetFrameTime();
                float cosAngle = MathF.Cos(angle);
                float sinAngle = MathF.Sin(angle);
                float newX = direction.X * cosAngle - direction.Z * sinAngle;
                float newZ = direction.X * sinAngle + direction.Z * cosAngle;
                Camera.Position = Camera.Target + new Vector3(newX, direction.Y, newZ);
            }
            if (Raylib.IsKeyDown(KeyboardKey.Right))
            {
                Vector3 direction = Camera.Position - Camera.Target;
                float angle = -RotationSpeed * Raylib.GetFrameTime();
                float cosAngle = MathF.Cos(angle);
                float sinAngle = MathF.Sin(angle);
                float newX = direction.X * cosAngle - direction.Z * sinAngle;
                float newZ = direction.X * sinAngle + direction.Z * cosAngle;
                Camera.Position = Camera.Target + new Vector3(newX, direction.Y, newZ);
            }
            
            // Zoom
            if (Raylib.IsKeyDown(KeyboardKey.Up))
            {
                Vector3 direction = Camera.Target - Camera.Position;
                Camera.Position += Vector3.Normalize(direction) * MoveSpeed;
            }
            if (Raylib.IsKeyDown(KeyboardKey.Down))
            {
                Vector3 direction = Camera.Target - Camera.Position;
                Camera.Position -= Vector3.Normalize(direction) * MoveSpeed;
            }
        }
    }
}