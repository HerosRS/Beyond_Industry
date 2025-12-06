using System;
using System.Numerics;
using Raylib_cs;
using BeyondIndustry.Data;

namespace BeyondIndustry
{
    public class CameraController
    {
        public Camera3D Camera;
        
        // ===== BLENDER-STYLE SETTINGS =====
        public float OrbitSensitivity { get; set; } = 0.3f;      // Mittlere Maustaste Rotation
        public float PanSensitivity { get; set; } = 0.01f;       // Shift + Mittlere Maustaste Pan
        public float ZoomSpeed { get; set; } = 2.0f;             // Mausrad Zoom
        public float WalkSpeed { get; set; } = 15.0f;            // WASD Bewegung
        public float WalkSpeedFast { get; set; } = 40.0f;        // Shift + WASD
        public float WalkSpeedSlow { get; set; } = 5.0f;         // Ctrl + WASD
        
        // ===== CAMERA STATE =====
        private Vector3 targetPosition;      // Der Punkt um den rotiert wird
        private float distance;              // Distanz zum Target
        private float horizontalAngle;       // Rotation um Y-Achse (Azimuth)
        private float verticalAngle;         // Rotation um X-Achse (Elevation)
        
        // ===== LIMITS =====
        public float MinDistance { get; set; } = 2.0f;
        public float MaxDistance { get; set; } = 150.0f;
        public float MinVerticalAngle { get; set; } = 5.0f;
        public float MaxVerticalAngle { get; set; } = 85.0f;
        
        // ===== BOUNDS =====
        public bool UseBounds { get; set; } = true;
        public float MinX { get; set; } = -50f;
        public float MaxX { get; set; } = 50f;
        public float MinZ { get; set; } = -50f;
        public float MaxZ { get; set; } = 50f;
        
        // ===== INITIALIZATION =====
        public CameraController(float moveSpeed = 20.0f, float initialDistance = 30.0f)
        {
            WalkSpeed = moveSpeed;
            targetPosition = new Vector3(0.0f, 0.0f, 0.0f);
            distance = initialDistance;
            horizontalAngle = 45.0f;
            verticalAngle = 45.0f;
            
            Camera = new Camera3D
            {
                Position = CalculateCameraPosition(),
                Target = targetPosition,
                Up = new Vector3(0.0f, 1.0f, 0.0f),
                FovY = GameConstants.DEFAULT_CAMERA_FOV,
                Projection = CameraProjection.Perspective
            };
        }
        
        // ===== GETTERS =====
        public float GetHorizontalAngle() => horizontalAngle;
        public float GetVerticalAngle() => verticalAngle;
        public float GetDistance() => distance;
        public Vector3 GetTargetPosition() => targetPosition;
        
        // ===== SETTERS =====
        public void SetPosition(Vector3 target, float horizontal, float vertical, float dist)
        {
            targetPosition = target;
            horizontalAngle = horizontal;
            verticalAngle = vertical;
            distance = dist;
            UpdateCamera();
        }
        
        // ===== MAIN UPDATE =====
        public void Update()
        {
            float deltaTime = Raylib.GetFrameTime();
            
            // ===== BLENDER CONTROLS =====
            
            // 1. MITTLERE MAUSTASTE (OHNE SHIFT) = ORBIT (wie Blender)
            if (Raylib.IsMouseButtonDown(MouseButton.Middle) && 
                !Raylib.IsKeyDown(KeyboardKey.LeftShift) &&
                !Raylib.IsKeyDown(KeyboardKey.RightShift))
            {
                HandleOrbit();
            }
            
            // 2. SHIFT + MITTLERE MAUSTASTE = PAN (wie Blender)
            else if (Raylib.IsMouseButtonDown(MouseButton.Middle) && 
                     (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift)))
            {
                HandlePan();
            }
            
            // 3. MAUSRAD = ZOOM (wie Blender)
            HandleZoom();
            
            // 4. WASD = WALK (zusätzlich zu Blender)
            HandleWASDMovement(deltaTime);
            
            // 5. Apply Bounds
            if (UseBounds)
            {
                targetPosition.X = Math.Clamp(targetPosition.X, MinX, MaxX);
                targetPosition.Z = Math.Clamp(targetPosition.Z, MinZ, MaxZ);
            }
            
            // Update Camera
            UpdateCamera();
        }
        
        // ===== 1. ORBIT (Mittlere Maustaste) =====
        private void HandleOrbit()
        {
            Vector2 mouseDelta = Raylib.GetMouseDelta();
            
            if (mouseDelta.Length() > 0)
            {
                // Horizontal rotation (um Y-Achse)
                horizontalAngle -= mouseDelta.X * OrbitSensitivity;
                
                // Vertical rotation (um X-Achse)
                verticalAngle += mouseDelta.Y * OrbitSensitivity;
                verticalAngle = Math.Clamp(verticalAngle, MinVerticalAngle, MaxVerticalAngle);
            }
        }
        
        // ===== 2. PAN (Shift + Mittlere Maustaste) =====
        private void HandlePan()
        {
            Vector2 mouseDelta = Raylib.GetMouseDelta();
            
            if (mouseDelta.Length() > 0)
            {
                // Berechne Kamera-relative Vektoren
                Vector3 right = GetRightVector();
                Vector3 up = GetUpVector();
                
                // Pan-Geschwindigkeit abhängig von Distanz
                float panSpeed = distance * PanSensitivity;
                
                // Bewege Target
                targetPosition -= right * mouseDelta.X * panSpeed;
                targetPosition += up * mouseDelta.Y * panSpeed;
            }
        }
        
        // ===== 3. ZOOM (Mausrad) =====
        private void HandleZoom()
        {
            float wheel = Raylib.GetMouseWheelMove();
            
            if (wheel != 0)
            {
                // Zoom towards/away from target
                distance -= wheel * ZoomSpeed;
                distance = Math.Clamp(distance, MinDistance, MaxDistance);
            }
        }
        
        // ===== 4. WASD MOVEMENT =====
        private void HandleWASDMovement(float deltaTime)
        {
            Vector3 movement = Vector3.Zero;
            
            // Berechne Bewegungsrichtungen (relativ zur Kamera-Blickrichtung)
            Vector3 forward = GetForwardVector();
            Vector3 right = GetRightVector();
            
            // Input sammeln
            if (Raylib.IsKeyDown(KeyboardKey.W))
                movement += forward;
            if (Raylib.IsKeyDown(KeyboardKey.S))
                movement -= forward;
            if (Raylib.IsKeyDown(KeyboardKey.D))
                movement += right;
            if (Raylib.IsKeyDown(KeyboardKey.A))
                movement -= right;
            
            // Wenn Bewegung vorhanden
            if (movement.Length() > 0)
            {
                movement = Vector3.Normalize(movement);
                
                // Geschwindigkeit mit Modifiern
                float speed = WalkSpeed;
                
                if (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift))
                    speed = WalkSpeedFast;
                else if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
                    speed = WalkSpeedSlow;
                
                // Bewege Target (Kamera folgt)
                targetPosition += movement * speed * deltaTime;
            }
        }
        
        // ===== CALCULATE CAMERA POSITION =====
        private Vector3 CalculateCameraPosition()
        {
            // Konvertiere Winkel zu Radiant
            float hRad = horizontalAngle * (MathF.PI / 180.0f);
            float vRad = verticalAngle * (MathF.PI / 180.0f);
            
            // Sphärische Koordinaten zu Kartesisch
            float x = targetPosition.X + distance * MathF.Cos(vRad) * MathF.Sin(hRad);
            float y = targetPosition.Y + distance * MathF.Sin(vRad);
            float z = targetPosition.Z + distance * MathF.Cos(vRad) * MathF.Cos(hRad);
            
            return new Vector3(x, y, z);
        }
        
        // ===== UPDATE CAMERA =====
        private void UpdateCamera()
        {
            Camera.Target = targetPosition;
            Camera.Position = CalculateCameraPosition();
        }
        
        // ===== HELPER: GET FORWARD VECTOR =====
        private Vector3 GetForwardVector()
        {
            // Forward auf XZ-Ebene projiziert (keine Y-Komponente)
            Vector3 forward = Vector3.Normalize(Camera.Target - Camera.Position);
            forward.Y = 0;
            
            if (forward.Length() > 0)
                return Vector3.Normalize(forward);
            
            // Fallback
            return new Vector3(0, 0, -1);
        }
        
        // ===== HELPER: GET RIGHT VECTOR =====
        private Vector3 GetRightVector()
        {
            Vector3 forward = GetForwardVector();
            Vector3 worldUp = new Vector3(0, 1, 0);
            return Vector3.Normalize(Vector3.Cross(forward, worldUp));
        }
        
        // ===== HELPER: GET UP VECTOR =====
        private Vector3 GetUpVector()
        {
            // Für Panning: Immer World-Up (Y-Achse)
            return new Vector3(0, 1, 0);
        }
        
        // ===== FOCUS ON POSITION =====
        public void FocusOnPosition(Vector3 position, bool instant = false)
        {
            targetPosition = position;
            
            if (instant)
            {
                UpdateCamera();
            }
        }
        
        // ===== RESET CAMERA =====
        public void ResetCamera()
        {
            targetPosition = Vector3.Zero;
            horizontalAngle = 45.0f;
            verticalAngle = 45.0f;
            distance = 30.0f;
            UpdateCamera();
            
            Console.WriteLine("[Camera] Reset to default position");
        }
        
        // ===== SET BOUNDS =====
        public void SetBounds(float minX, float maxX, float minZ, float maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }
        
        // ===== QUICK VIEWS (wie Numpad in Blender) =====
        
        public void SetTopView()
        {
            verticalAngle = 89.0f;
            horizontalAngle = 0.0f;
            UpdateCamera();
            Console.WriteLine("[Camera] Top View");
        }
        
        public void SetFrontView()
        {
            verticalAngle = 45.0f;
            horizontalAngle = 0.0f;
            UpdateCamera();
            Console.WriteLine("[Camera] Front View");
        }
        
        public void SetRightView()
        {
            verticalAngle = 45.0f;
            horizontalAngle = 90.0f;
            UpdateCamera();
            Console.WriteLine("[Camera] Right View");
        }
        
        public void SetIsometricView()
        {
            verticalAngle = 35.264f;  // Isometrischer Winkel
            horizontalAngle = 45.0f;
            UpdateCamera();
            Console.WriteLine("[Camera] Isometric View");
        }
        
        // ===== DEBUG INFO =====
        public string GetDebugInfo()
        {
            return $"Target: ({targetPosition.X:F1}, {targetPosition.Y:F1}, {targetPosition.Z:F1}) | " +
                   $"Dist: {distance:F1} | H: {horizontalAngle:F0}° V: {verticalAngle:F0}°";
        }
    }
}