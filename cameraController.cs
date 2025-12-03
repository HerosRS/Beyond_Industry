using System;
using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Data;

namespace BeyondIndustry
{
    public enum CameraMode
    {
        Free,
        Orbit
    }
    
    public class CameraController
    {
        public Camera3D Camera;
        
        // ===== MODUS =====
        public CameraMode Mode { get; private set; } = CameraMode.Free;
        
        // ===== BEWEGUNGS-EINSTELLUNGEN =====
        public float MoveSpeed { get; set; } = 20.0f;
        public float FastMoveMultiplier { get; set; } = 2.5f;
        public float SlowMoveMultiplier { get; set; } = 0.5f;
        
        // ===== ROTATIONS-EINSTELLUNGEN =====
        public float MouseRotationSpeed { get; set; } = 0.5f;
        public float MouseTiltSpeed { get; set; } = 0.5f;
        
        // ===== ZOOM-EINSTELLUNGEN =====
        public float ZoomSpeed { get; set; } = 2.0f;
        public float MinZoom { get; set; } = 5.0f;
        public float MaxZoom { get; set; } = 100.0f;
        
        // ===== KAMERA-WINKEL =====
        private float horizontalAngle = 45.0f;  // Y-Rotation
        private float verticalAngle = 45.0f;    // X-Rotation (Pitch/Tilt)
        private float distance = 30.0f;         // Distanz vom Target
        
        // ===== TARGET =====
        private Vector3 targetPosition;
        
        // ===== WINKEL-GRENZEN =====
        public float MinVerticalAngle { get; set; } = 10.0f;
        public float MaxVerticalAngle { get; set; } = 80.0f;
        
        // ===== ORBIT MODE =====
        private Vector3 orbitTarget;
        private float orbitDistance = 10.0f;
        
        // ===== BOUNDS =====
        public bool UseBounds { get; set; } = true;
        public float MinX { get; set; } = -50f;
        public float MaxX { get; set; } = 50f;
        public float MinZ { get; set; } = -50f;
        public float MaxZ { get; set; } = 50f;
        
        public CameraController(float moveSpeed = 20.0f, float v = 0)
        {
            MoveSpeed = moveSpeed;
            targetPosition = new Vector3(0.0f, 0.0f, 0.0f);
            orbitTarget = Vector3.Zero;
            
            Camera = new Camera3D
            {
                Position = CalculateCameraPosition(),
                Target = targetPosition,
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = GameConstants.DEFAULT_CAMERA_FOV,
                Projection = CameraProjection.Orthographic
            };
        }
        
        // ===== UPDATE =====
        public void Update()
        {
            float deltaTime = Raylib.GetFrameTime();
            
            // Mode Switch
            if (Raylib.IsKeyPressed(KeyboardKey.O) && Raylib.IsKeyDown(KeyboardKey.LeftAlt))
            {
                if (Mode == CameraMode.Free)
                    EnterOrbitMode(targetPosition);
                else
                    ExitOrbitMode();
            }
            
            if (Mode == CameraMode.Free)
            {
                UpdateFreeMode(deltaTime);
            }
            else if (Mode == CameraMode.Orbit)
            {
                UpdateOrbitMode(deltaTime);
            }
        }
        
        // ===== FREE MODE UPDATE =====
        private void UpdateFreeMode(float deltaTime)
        {
            // ===== WASD MOVEMENT (RELATIV ZUR KAMERA!) =====
            Vector3 movement = Vector3.Zero;
            
            // Berechne Kamera-Richtungsvektoren
            Vector3 forward = GetForwardVector();
            Vector3 right = GetRightVector();
            
            if (Raylib.IsKeyDown(KeyboardKey.W))
                movement += forward;
            if (Raylib.IsKeyDown(KeyboardKey.S))
                movement -= forward;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                movement += right;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                movement -= right;
            
            // Normalisieren und Speed anwenden
            if (movement.Length() > 0)
            {
                movement = Vector3.Normalize(movement);
                
                float speed = MoveSpeed;
                if (Raylib.IsKeyDown(KeyboardKey.LeftShift))
                    speed *= FastMoveMultiplier;
                if (Raylib.IsKeyDown(KeyboardKey.LeftControl))
                    speed *= SlowMoveMultiplier;
                
                targetPosition += movement * speed * deltaTime;
            }
            
            // ===== RECHTE MAUSTASTE - ROTATION (HORIZONTAL) =====
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                //Vector2 mouseDelta = Raylib.GetMouseDelta();
                //horizontalAngle -= mouseDelta.X * MouseRotationSpeed;
            }
            
            // ===== MITTLERE MAUSTASTE - PANNING & TILT =====
            if (Raylib.IsMouseButtonDown(MouseButton.Middle))
            {
                Vector2 mouseDelta = Raylib.GetMouseDelta();
                
                // Horizontal: Pan (Target bewegen relativ zur Kamera)
                if (MathF.Abs(mouseDelta.X) > MathF.Abs(mouseDelta.Y))
                {
                    horizontalAngle -= mouseDelta.X * MouseRotationSpeed;
                }
                // Vertikal: Tilt (Kamera neigen)
                else
                {
                    verticalAngle += mouseDelta.Y * MouseTiltSpeed;
                    verticalAngle = Math.Clamp(verticalAngle, MinVerticalAngle, MaxVerticalAngle);
                }
                
            }
            
            // ===== MAUSRAD - ZOOM =====
            float wheel = Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                distance -= wheel * ZoomSpeed;
                distance = Math.Clamp(distance, MinZoom, MaxZoom);
            }
            
            // Apply Bounds
            if (UseBounds)
            {
                targetPosition.X = Math.Clamp(targetPosition.X, MinX, MaxX);
                targetPosition.Z = Math.Clamp(targetPosition.Z, MinZ, MaxZ);
            }
            
            // Update Camera
            Camera.Target = targetPosition;
            Camera.Position = CalculateCameraPosition();
        }
        
        // ===== ORBIT MODE UPDATE =====
        private void UpdateOrbitMode(float deltaTime)
        {
            // WASD für Orbit
            if (Raylib.IsKeyDown(KeyboardKey.A))
                horizontalAngle += 100.0f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                horizontalAngle -= 100.0f * deltaTime;
            if (Raylib.IsKeyDown(KeyboardKey.W))
            {
                verticalAngle -= 50.0f * deltaTime;
                verticalAngle = Math.Clamp(verticalAngle, MinVerticalAngle, MaxVerticalAngle);
            }
            if (Raylib.IsKeyDown(KeyboardKey.S))
            {
                verticalAngle += 50.0f * deltaTime;
                verticalAngle = Math.Clamp(verticalAngle, MinVerticalAngle, MaxVerticalAngle);
            }
            
            // Rechte Maustaste - Orbit Rotation
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                Vector2 mouseDelta = Raylib.GetMouseDelta();
                horizontalAngle -= mouseDelta.X * MouseRotationSpeed;
                verticalAngle += mouseDelta.Y * MouseRotationSpeed;
                verticalAngle = Math.Clamp(verticalAngle, MinVerticalAngle, MaxVerticalAngle);
            }
            
            // Mittlere Maustaste - Target bewegen
            if (Raylib.IsMouseButtonDown(MouseButton.Middle))
            {
                Vector2 mouseDelta = Raylib.GetMouseDelta();
                Vector3 right = GetRightVector();
                Vector3 up = new Vector3(0, 1, 0);
                
                float panSpeed = orbitDistance * 0.01f;
                orbitTarget -= right * mouseDelta.X * panSpeed;
                orbitTarget -= up * mouseDelta.Y * panSpeed;
            }
            
            // Zoom
            float wheel = Raylib.GetMouseWheelMove();
            if (wheel != 0)
            {
                orbitDistance -= wheel * ZoomSpeed;
                orbitDistance = Math.Clamp(orbitDistance, 2.0f, 50.0f);
            }
            
            // Update Camera
            Camera.Target = orbitTarget;
            Camera.Position = CalculateOrbitPosition();
        }
        
        // ===== BERECHNE KAMERA-POSITION (FREE MODE) =====
        private Vector3 CalculateCameraPosition()
        {
            float hRad = horizontalAngle * (MathF.PI / 180.0f);
            float vRad = verticalAngle * (MathF.PI / 180.0f);
            
            float x = targetPosition.X - distance * MathF.Cos(vRad) * MathF.Sin(hRad);
            float y = targetPosition.Y + distance * MathF.Sin(vRad);
            float z = targetPosition.Z - distance * MathF.Cos(vRad) * MathF.Cos(hRad);
            
            return new Vector3(x, y, z);
        }
        
        // ===== BERECHNE ORBIT POSITION =====
        private Vector3 CalculateOrbitPosition()
        {
            float hRad = horizontalAngle * (MathF.PI / 180.0f);
            float vRad = verticalAngle * (MathF.PI / 180.0f);
            
            float x = orbitTarget.X - orbitDistance * MathF.Cos(vRad) * MathF.Sin(hRad);
            float y = orbitTarget.Y + orbitDistance * MathF.Sin(vRad);
            float z = orbitTarget.Z - orbitDistance * MathF.Cos(vRad) * MathF.Cos(hRad);
            
            return new Vector3(x, y, z);
        }
        
        // ===== HELPER: GET FORWARD VECTOR (RELATIV ZUR KAMERA) =====
        private Vector3 GetForwardVector()
        {
            // Forward ist die Richtung von Kamera zu Target (projiziert auf XZ-Ebene)
            Vector3 forward = Vector3.Normalize(Camera.Target - Camera.Position);
            forward.Y = 0;  // Nur horizontal
            
            if (forward.Length() > 0)
                return Vector3.Normalize(forward);
            
            return new Vector3(0, 0, -1);
        }
        
        // ===== HELPER: GET RIGHT VECTOR (RELATIV ZUR KAMERA) =====
        private Vector3 GetRightVector()
        {
            Vector3 forward = GetForwardVector();
            Vector3 up = new Vector3(0, 1, 0);
            return Vector3.Normalize(Vector3.Cross(forward, up));
        }
        
        // ===== ENTER ORBIT MODE =====
        public void EnterOrbitMode(Vector3 target)
        {
            Mode = CameraMode.Orbit;
            orbitTarget = target;
            orbitDistance = Vector3.Distance(Camera.Position, target);
            orbitDistance = Math.Clamp(orbitDistance, 2.0f, 50.0f);
            Console.WriteLine($"[Camera] Entered Orbit Mode around {orbitTarget}");
        }
        
        // ===== EXIT ORBIT MODE =====
        public void ExitOrbitMode()
        {
            Mode = CameraMode.Free;
            targetPosition = orbitTarget;
            distance = orbitDistance;
            Console.WriteLine("[Camera] Exited Orbit Mode");
        }
        
        // ===== FOCUS ON POSITION =====
        public void FocusOnPosition(Vector3 position, bool instant = false, bool enterOrbit = false)
        {
            if (enterOrbit)
            {
                EnterOrbitMode(position);
            }
            else
            {
                targetPosition = position;
                
                if (instant)
                {
                    Camera.Target = targetPosition;
                    Camera.Position = CalculateCameraPosition();
                }
            }
        }
        
        // ===== RESET CAMERA =====
        public void ResetCamera()
        {
            if (Mode == CameraMode.Orbit)
                ExitOrbitMode();
            
            targetPosition = Vector3.Zero;
            horizontalAngle = 45.0f;
            verticalAngle = 45.0f;
            distance = 30.0f;
            Camera.Target = targetPosition;
            Camera.Position = CalculateCameraPosition();
        }
        
        // ===== SET BOUNDS =====
        public void SetBounds(float minX, float maxX, float minZ, float maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }
        
        // ===== DEBUG INFO =====
        public string GetDebugInfo()
        {
            string modeStr = Mode == CameraMode.Free ? "FREE" : "ORBIT";
            
            if (Mode == CameraMode.Free)
            {
                return $"Mode: {modeStr} | Pos: ({Camera.Target.X:F1}, {Camera.Target.Z:F1}) | " +
                       $"Dist: {distance:F1} | H: {horizontalAngle:F0}° V: {verticalAngle:F0}°";
            }
            else
            {
                return $"Mode: {modeStr} | Target: ({orbitTarget.X:F1}, {orbitTarget.Y:F1}, {orbitTarget.Z:F1}) | " +
                       $"Dist: {orbitDistance:F1} | H: {horizontalAngle:F0}° V: {verticalAngle:F0}°";
            }
        }
    }
}