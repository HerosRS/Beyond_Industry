using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Rendering
{
    public enum TimeOfDay
    {
        Dawn,        // Morgengrauen
        Morning,     // Morgen
        Noon,        // Mittag
        Afternoon,   // Nachmittag
        Dusk,        // Abenddämmerung
        Night        // Nacht
    }
    
    public static class LightingSystem
    {
        // ===== LIGHTING STATE =====
        public static TimeOfDay CurrentTime { get; private set; } = TimeOfDay.Afternoon;
        public static float TimeProgress { get; private set; } = 0.0f;
        public static bool AutoCycle { get; set; } = false;
        public static float CycleSpeed { get; set; } = 0.1f;  // Wie schnell der Tag/Nacht-Zyklus läuft
        
        // ===== LIGHT SETTINGS =====
        private static Vector3 lightDirection = new Vector3(-0.5f, -1.0f, -0.5f);
        private static Color ambientColor = new Color(180, 160, 140, 255);
        private static Color skyColor = new Color(135, 206, 235, 255);
        private static Color fogColor = new Color(200, 210, 220, 255);
        
        // ===== SHADER LOCATIONS =====
        private static int ambientLoc = -1;
        private static int lightDirLoc = -1;
        private static int lightColorLoc = -1;
        
        // ===== SUN RENDERING =====
        private static Vector3 sunPosition = new Vector3(0, 50, 0);
        private static float sunSize = 3.0f;
        private static bool renderSun = true;
        public static void DrawSun(Camera3D camera)
        {
            if (!renderSun)
                return;
            
            var preset = GetPreset(CurrentTime);
            
            // Berechne Sonnenposition basierend auf Lichtrichtung
            Vector3 sunDir = Vector3.Normalize(-lightDirection);
            sunPosition = camera.Target + sunDir * 100.0f;
            
            // Sonnenfarbe basierend auf Tageszeit
            Color sunColor = preset.LightColor;
            
            // Bei Nacht: Mond statt Sonne
            if (CurrentTime == TimeOfDay.Night)
            {
                sunColor = new Color(200, 210, 230, 255);  // Bläulicher Mond
                sunSize = 2.0f;
            }
            else
            {
                sunSize = 3.0f;
            }
            
            // Zeichne Sonne als leuchtende Kugel
            Raylib.DrawSphere(sunPosition, sunSize, sunColor);
            
            // Glow-Effekt (mehrere Spheres mit Transparenz)
            for (int i = 1; i <= 3; i++)
            {
                float glowSize = sunSize + (i * 0.5f);
                byte alpha = (byte)(80 / i);
                Color glowColor = new Color(sunColor.R, sunColor.G, sunColor.B, alpha);
                Raylib.DrawSphere(sunPosition, glowSize, glowColor);
            }
        }

        public static void ToggleSun()
        {
            renderSun = !renderSun;
            Console.WriteLine($"[Lighting] Sun rendering: {renderSun}");
        }
        // ===== PREDEFINED LIGHTING PRESETS =====
        
        public static LightingPreset GetPreset(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Dawn:
                    return new LightingPreset
                    {
                        Name = "Dawn",
                        SkyColor = new Color(255, 183, 158, 255),      // Warmes Orange-Rosa
                        AmbientColor = new Color(120, 100, 140, 255),  // Kühles Lila
                        LightDirection = new Vector3(-0.8f, -0.3f, -0.5f),
                        LightColor = new Color(255, 200, 150, 255),    // Warmes Orange
                        FogColor = new Color(180, 150, 170, 255),
                        FogDensity = 0.02f
                    };
                    
                case TimeOfDay.Morning:
                    return new LightingPreset
                    {
                        Name = "Morning",
                        SkyColor = new Color(135, 206, 250, 255),      // Helles Blau
                        AmbientColor = new Color(200, 190, 180, 255),  // Warmes Weiß
                        LightDirection = new Vector3(-0.6f, -0.5f, -0.4f),
                        LightColor = new Color(255, 240, 220, 255),    // Helles Warmweiß
                        FogColor = new Color(220, 230, 240, 255),
                        FogDensity = 0.01f
                    };
                    
                case TimeOfDay.Noon:
                    return new LightingPreset
                    {
                        Name = "Noon",
                        SkyColor = new Color(100, 180, 255, 255),      // Klares Blau
                        AmbientColor = new Color(220, 220, 220, 255),  // Helles Grau
                        LightDirection = new Vector3(-0.3f, -1.0f, -0.3f),
                        LightColor = new Color(255, 255, 255, 255),    // Reines Weiß
                        FogColor = new Color(240, 245, 250, 255),
                        FogDensity = 0.005f
                    };
                    
                case TimeOfDay.Afternoon:
                    return new LightingPreset
                    {
                        Name = "Afternoon (Cozy)",
                        SkyColor = new Color(135, 195, 235, 255),      // Sanftes Blau
                        AmbientColor = new Color(200, 180, 160, 255),  // Warmes Beige
                        LightDirection = new Vector3(-0.5f, -0.8f, -0.6f),
                        LightColor = new Color(255, 235, 200, 255),    // Warmes Goldgelb
                        FogColor = new Color(210, 200, 190, 255),
                        FogDensity = 0.012f
                    };
                    
                case TimeOfDay.Dusk:
                    return new LightingPreset
                    {
                        Name = "Dusk (Golden Hour)",
                        SkyColor = new Color(255, 140, 100, 255),      // Tiefes Orange
                        AmbientColor = new Color(140, 100, 120, 255),  // Violett-Grau
                        LightDirection = new Vector3(-0.9f, -0.2f, -0.5f),
                        LightColor = new Color(255, 160, 100, 255),    // Intensives Orange
                        FogColor = new Color(160, 120, 140, 255),
                        FogDensity = 0.025f
                    };
                    
                case TimeOfDay.Night:
                    return new LightingPreset
                    {
                        Name = "Night (Moody)",
                        SkyColor = new Color(20, 24, 40, 255),         // Dunkles Blau-Grau
                        AmbientColor = new Color(40, 50, 80, 255),     // Tiefes Blau
                        LightDirection = new Vector3(-0.5f, -0.5f, -0.8f),
                        LightColor = new Color(150, 170, 200, 255),    // Kühles Mondlicht
                        FogColor = new Color(30, 40, 60, 255),
                        FogDensity = 0.03f
                    };
                    
                default:
                    return GetPreset(TimeOfDay.Afternoon);
            }
        }
        
        // ===== INITIALIZE =====
        public static void Initialize(Shader shader)
        {
            // Get shader locations
            ambientLoc = Raylib.GetShaderLocation(shader, "ambient");
            lightDirLoc = Raylib.GetShaderLocation(shader, "lightDir");
            lightColorLoc = Raylib.GetShaderLocation(shader, "lightColor");
            
            // Set initial lighting (Cozy Afternoon)
            SetTimeOfDay(TimeOfDay.Afternoon);
            
            Console.WriteLine("[Lighting] Initialized with Cozy Afternoon preset");
        }
        
        // ===== UPDATE =====
        public static void Update(float deltaTime, Shader? shader)
        {
            if (AutoCycle)
            {
                TimeProgress += deltaTime * CycleSpeed;
                
                if (TimeProgress >= 1.0f)
                {
                    TimeProgress = 0.0f;
                    CycleToNextTime();
                }
                
                // Smooth transition between times
                if (shader.HasValue)
                {
                    LerpBetweenPresets(shader.Value, TimeProgress);
                }
            }
        }
        
        // ===== SET TIME OF DAY =====
        public static void SetTimeOfDay(TimeOfDay time)
        {
            CurrentTime = time;
            TimeProgress = 0.0f;
            
            var preset = GetPreset(time);
            ApplyPreset(preset);
            
            Console.WriteLine($"[Lighting] Changed to {preset.Name}");
        }
        
        // ===== APPLY PRESET =====
        public static void ApplyPreset(LightingPreset preset)
        {
            skyColor = preset.SkyColor;
            ambientColor = preset.AmbientColor;
            lightDirection = Vector3.Normalize(preset.LightDirection);
            fogColor = preset.FogColor;
            
            // Update global colors
            Data.GlobalColor.skyColor = skyColor;
        }
        
        // ===== UPDATE SHADER =====
        public static void UpdateShader(Shader shader)
        {
            if (ambientLoc >= 0)
            {
                float[] ambient = new float[] {
                    ambientColor.R / 255.0f,
                    ambientColor.G / 255.0f,
                    ambientColor.B / 255.0f,
                    1.0f
                };
                Raylib.SetShaderValue(shader, ambientLoc, ambient, ShaderUniformDataType.Vec4);
            }
            
            if (lightDirLoc >= 0)
            {
                float[] dir = new float[] { lightDirection.X, lightDirection.Y, lightDirection.Z };
                Raylib.SetShaderValue(shader, lightDirLoc, dir, ShaderUniformDataType.Vec3);
            }
            
            if (lightColorLoc >= 0)
            {
                var preset = GetPreset(CurrentTime);
                float[] color = new float[] {
                    preset.LightColor.R / 255.0f,
                    preset.LightColor.G / 255.0f,
                    preset.LightColor.B / 255.0f,
                    1.0f
                };
                Raylib.SetShaderValue(shader, lightColorLoc, color, ShaderUniformDataType.Vec4);
            }
        }
        
        // ===== LERP BETWEEN PRESETS =====
        private static void LerpBetweenPresets(Shader shader, float t)
        {
            var current = GetPreset(CurrentTime);
            var next = GetPreset(GetNextTime(CurrentTime));
            
            // Lerp colors
            skyColor = LerpColor(current.SkyColor, next.SkyColor, t);
            ambientColor = LerpColor(current.AmbientColor, next.AmbientColor, t);
            fogColor = LerpColor(current.FogColor, next.FogColor, t);
            
            // Lerp light direction
            lightDirection = Vector3.Normalize(Vector3.Lerp(current.LightDirection, next.LightDirection, t));
            
            Data.GlobalColor.skyColor = skyColor;
            UpdateShader(shader);
        }
        
        // ===== CYCLE TO NEXT TIME =====
        private static void CycleToNextTime()
        {
            CurrentTime = GetNextTime(CurrentTime);
            Console.WriteLine($"[Lighting] Cycled to {CurrentTime}");
        }
        
        private static TimeOfDay GetNextTime(TimeOfDay current)
        {
            switch (current)
            {
                case TimeOfDay.Dawn: return TimeOfDay.Morning;
                case TimeOfDay.Morning: return TimeOfDay.Noon;
                case TimeOfDay.Noon: return TimeOfDay.Afternoon;
                case TimeOfDay.Afternoon: return TimeOfDay.Dusk;
                case TimeOfDay.Dusk: return TimeOfDay.Night;
                case TimeOfDay.Night: return TimeOfDay.Dawn;
                default: return TimeOfDay.Dawn;
            }
        }
        
        // ===== HELPER: LERP COLOR =====
        // ===== HELPER: LERP COLOR =====
        private static Color LerpColor(Color a, Color b, float t)
        {
            return new Color(
                (byte)((int)a.R + ((int)b.R - (int)a.R) * t),
                (byte)((int)a.G + ((int)b.G - (int)a.G) * t),
                (byte)((int)a.B + ((int)b.B - (int)a.B) * t),
                (byte)255
            );
        }
        
        // ===== GETTERS =====
        public static Color GetSkyColor() => skyColor;
        public static Color GetFogColor() => fogColor;
        public static Color GetAmbientColor() => ambientColor;
        
        // ===== DEBUG INFO =====
        public static string GetDebugInfo()
        {
            return $"Time: {CurrentTime} | Auto: {AutoCycle} | Progress: {TimeProgress:F2}";
        }
    }
    
    // ===== LIGHTING PRESET CLASS =====
    public class LightingPreset
    {
        public string Name { get; set; } = "";
        public Color SkyColor { get; set; }
        public Color AmbientColor { get; set; }
        public Vector3 LightDirection { get; set; }
        public Color LightColor { get; set; }
        public Color FogColor { get; set; }
        public float FogDensity { get; set; }
    }
}