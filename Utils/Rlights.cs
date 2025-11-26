using System;
using System.Numerics;
using Raylib_cs;

namespace BeyondIndustry.Utils
{
    public enum LightType
    {
        Directional = 0,
        Point = 1
    }

    public struct Light
    {
        public bool Enabled;
        public LightType Type;
        public Vector3 Position;
        public Vector3 Target;
        public Color Color;
        
        // Shader Locations
        public int EnabledLoc;
        public int TypeLoc;
        public int PositionLoc;
        public int TargetLoc;
        public int ColorLoc;
    }

    public static class Rlights
    {
        private static int lightCount = 0;

        public static Light CreateLight(LightType type, Vector3 position, Vector3 target, Color color, Shader shader)
        {
            Light light = new Light();
            
            if (lightCount < 4) // Max 4 Lichter
            {
                light.Enabled = true;
                light.Type = type;
                light.Position = position;
                light.Target = target;
                light.Color = color;

                string enabledName = $"lights[{lightCount}].enabled";
                string typeName = $"lights[{lightCount}].type";
                string positionName = $"lights[{lightCount}].position";
                string targetName = $"lights[{lightCount}].target";
                string colorName = $"lights[{lightCount}].color";

                light.EnabledLoc = Raylib.GetShaderLocation(shader, enabledName);
                light.TypeLoc = Raylib.GetShaderLocation(shader, typeName);
                light.PositionLoc = Raylib.GetShaderLocation(shader, positionName);
                light.TargetLoc = Raylib.GetShaderLocation(shader, targetName);
                light.ColorLoc = Raylib.GetShaderLocation(shader, colorName);

                UpdateLight(shader, light);
                
                lightCount++;
            }

            return light;
        }

        public static void UpdateLight(Shader shader, Light light)
        {
            // Enabled
            Raylib.SetShaderValue(shader, light.EnabledLoc, light.Enabled ? 1 : 0, ShaderUniformDataType.Int);

            // Type
            Raylib.SetShaderValue(shader, light.TypeLoc, (int)light.Type, ShaderUniformDataType.Int);

            // Position
            float[] position = { light.Position.X, light.Position.Y, light.Position.Z };
            Raylib.SetShaderValue(shader, light.PositionLoc, position, ShaderUniformDataType.Vec3);

            // Target
            float[] target = { light.Target.X, light.Target.Y, light.Target.Z };
            Raylib.SetShaderValue(shader, light.TargetLoc, target, ShaderUniformDataType.Vec3);

            // Color
            float[] color = {
                light.Color.R / 255.0f,
                light.Color.G / 255.0f,
                light.Color.B / 255.0f,
                light.Color.A / 255.0f
            };
            Raylib.SetShaderValue(shader, light.ColorLoc, color, ShaderUniformDataType.Vec4);
        }
    }
}