using Raylib_cs;
using System.Numerics;

namespace BeyondIndustry.Data
{
    public static class GlobalData
    {
        // ===== SCREEN SETTINGS =====
        public static int SCREEN_WIDTH = 1600;
        public static int SCREEN_HEIGHT = 900;
        public static int CELL_SIZE = 2;
        
        // ===== CAMERA =====
         public static Camera3D camera = new Camera3D
        {
            Position = new Vector3(5.0f, 5.0f, 5.0f),
            Target = new Vector3(0.0f, 0.0f, 0.0f),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };
        public static void UpdateScreenSize()
        {
            SCREEN_WIDTH = Raylib.GetScreenWidth();
            SCREEN_HEIGHT = Raylib.GetScreenHeight();
        }
        // ===== DEBUG FLAGS =====
        public static bool ShowDebugInfo = false;           // Master Debug Toggle
        public static bool ShowFPS = false;                // FPS Counter
        public static bool ShowMousePos = false;           // Mausposition
        public static bool ShowCameraInfo = false;         // Kamera-Details
        public static bool ShowGrid = false;               // 3D Grid
        public static bool ShowConnectionDebug = false;    // Belt-Verbindungen
        public static bool ShowMachineInfo = false;        // Maschinen-Status
        public static bool ShowResourceFlow = false;       // Ressourcen-Flow
        public static bool ShowCollisionBoxes = false;     // Kollisions-Boxen
        public static bool ShowPerformanceStats = false;   // Performance-Daten
        
        // ===== GAME SETTINGS =====
        public static float TimeScale = 1.0f;              // Spiel-Geschwindigkeit (0.5 = halbe Speed, 2.0 = doppelte Speed)
        public static bool IsPaused = false;               // Spiel pausiert
        
        // ===== BELT SETTINGS =====
        public static float GlobalBeltSpeed = 1.0f;        // Globale Belt-Geschwindigkeit
        public static float GlobalItemHeight = 0.3f;       // Globale Item-Höhe
        
        // ===== STATISTICS =====
        public static int TotalMachinesPlaced = 0;
        public static int TotalItemsTransported = 0;
        public static float TotalPowerConsumed = 0f;
    }
    
    public static class GlobalColor
    
    {
        public static Color skyColor = new Color(135, 206, 235, 255);  // NEU
        // ===== UI COLORS =====
        public static Color BACKGROUND_COLOR = new Color(40, 40, 40, 255);
        public static Color FORGROUND_COLOR = new Color(56, 55, 52, 255);
        public static Color TEXT_COLOR = new Color(211, 196, 165, 255);
        
        // ===== DEBUG COLORS =====
        public static Color DEBUG_GREEN_COLOR = new Color(0, 255, 0, 255);
        public static Color DEBUG_RED_COLOR = new Color(255, 0, 0, 255);
        public static Color DEBUG_BLUE_COLOR = new Color(0, 100, 255, 255);
        public static Color DEBUG_YELLOW_COLOR = new Color(255, 255, 0, 255);
        public static Color DEBUG_ORANGE_COLOR = new Color(255, 165, 0, 255);
        
        // ===== STATUS COLORS =====
        public static Color STATUS_ACTIVE = new Color(0, 255, 0, 200);      // Grün = aktiv
        public static Color STATUS_IDLE = new Color(255, 255, 0, 200);      // Gelb = wartend
        public static Color STATUS_ERROR = new Color(255, 0, 0, 200);       // Rot = Fehler
        public static Color STATUS_DISABLED = new Color(128, 128, 128, 200); // Grau = aus
        
        // ===== BELT COLORS =====
        public static Color BELT_RUNNING = new Color(70, 70, 70, 255);
        public static Color BELT_STOPPED = new Color(100, 100, 100, 255);
        public static Color BELT_INPUT_MARKER = new Color(0, 255, 0, 200);   // Grün
        public static Color BELT_OUTPUT_MARKER = new Color(0, 100, 255, 200); // Blau
        
        // ===== MACHINE COLORS =====
        public static Color MACHINE_MINING = new Color(139, 69, 19, 255);    // Braun
        public static Color MACHINE_FURNACE = new Color(255, 140, 0, 255);   // Orange
        public static Color MACHINE_ASSEMBLER = new Color(100, 149, 237, 255); // Blau
        
        // ===== PREVIEW COLORS =====
        public static Color PREVIEW_VALID = new Color(0, 255, 0, 100);       // Grün = platzierbar
        public static Color PREVIEW_INVALID = new Color(255, 0, 0, 100);     // Rot = nicht platzierbar
        
        // ===== GRID COLORS =====
        public static Color GRID_MAIN = new Color(100, 100, 100, 100);       // Haupt-Grid
        public static Color GRID_SUB = new Color(80, 80, 80, 50);            // Unter-Grid
        
        // ===== HELPER METHODS =====
        public static Color WithAlpha(Color color, byte alpha)
        {
            return new Color(color.R, color.G, color.B, alpha);
        }
        
        public static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Color(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t),
                (byte)(a.A + (b.A - a.A) * t)
            );
        }
    }
    
    // ===== PERFORMANCE TRACKING =====
    public static class PerformanceStats
    {
        public static float AverageFrameTime = 0f;
        public static int DrawCalls = 0;
        public static int ActiveMachines = 0;
        public static int ActiveBelts = 0;
        public static int ItemsOnBelts = 0;
        
        private static float[] frameTimeHistory = new float[60];
        private static int frameIndex = 0;
        
        public static void RecordFrameTime(float deltaTime)
        {
            frameTimeHistory[frameIndex] = deltaTime;
            frameIndex = (frameIndex + 1) % frameTimeHistory.Length;
            
            // Berechne Durchschnitt
            float total = 0f;
            foreach (float time in frameTimeHistory)
                total += time;
            AverageFrameTime = total / frameTimeHistory.Length;
        }
        
        public static void Reset()
        {
            DrawCalls = 0;
        }
    }
    
    // ===== GAME CONSTANTS =====
    public static class GameConstants
    {
        // Grid
        public const int DEFAULT_GRID_SIZE = 10;
        public const float DEFAULT_CELL_SIZE = 1.0f;
        
        // Belts
        public const float MIN_BELT_SPEED = 0.1f;
        public const float MAX_BELT_SPEED = 5.0f;
        public const float DEFAULT_BELT_SPEED = 1.0f;
        public const int MAX_ITEMS_PER_BELT = 10;
        
        // Items
        public const float MIN_ITEM_HEIGHT = 0.1f;
        public const float MAX_ITEM_HEIGHT = 1.0f;
        public const float DEFAULT_ITEM_HEIGHT = 0.3f;
        
        // Machines
        public const float MIN_PRODUCTION_SPEED = 0.1f;
        public const float MAX_PRODUCTION_SPEED = 10.0f;
        public const int DEFAULT_BUFFER_SIZE = 10;
        
        // Power
        public const float DEFAULT_POWER_GENERATION = 200f;
        public const float POWER_WARNING_THRESHOLD = 0.9f;  // 90%
        
        // Camera
        public const float MIN_CAMERA_FOV = 5f;
        public const float MAX_CAMERA_FOV = 90f;
        public const float DEFAULT_CAMERA_FOV = 15f;
    }
}