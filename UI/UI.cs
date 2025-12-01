using System.Numerics;
using BeyondIndustry.Data;
using BeyondIndustry.Utils;
using BeyondIndustry.Debug;
using Raylib_cs;

namespace BeyondIndustry.UI
{
    public class MainUI
    {
        // Diese Methode ist für 2D UI NACH EndMode3D()
        public static void DebugDataUI()
        {
            int yPos = 10;
            
            if (DebugConsole.ShowFPS)
            {
                Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 10, yPos, 20, GlobalColor.DEBUG_GREEN_COLOR);
                yPos += 30;
            }

            if (DebugConsole.ShowCameraInfo)
            {
                Raylib.DrawText($"Camera Target: ({GlobalData.camera.Target.X:F1}, {GlobalData.camera.Target.Z:F1})", 
                               10, yPos, 20, GlobalColor.DEBUG_GREEN_COLOR);
                yPos += 30;
                
                Raylib.DrawText($"Camera Height: {GlobalData.camera.Position.Y:F1}", 
                               10, yPos, 20, GlobalColor.DEBUG_GREEN_COLOR);
                yPos += 30;
            }
            
            // Info-Texte unten
            Raylib.DrawText("[F1] Debug Console", 10, GlobalData.SCREEN_HEIGHT - 25, 14, Color.Gray);
            Raylib.DrawText($"[G] Grid: {(DebugConsole.ShowGrid ? "ON" : "OFF")}", 10, GlobalData.SCREEN_HEIGHT - 45, 14, Color.Gray);
        }

        // NEUE Methode für 3D-Elemente (muss IN BeginMode3D() aufgerufen werden!)
        public static void Draw3DElements()
        {
            // Grid zeichnen (wenn aktiviert)
            if (DebugConsole.ShowGrid)
            {
                Raylib.DrawGrid(10, 1.0f);
            }
        }
    }
}