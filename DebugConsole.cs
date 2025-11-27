using Raylib_cs;
using System;
using System.Collections.Generic;
using BeyondIndustry.Data;
using BeyondIndustry.Utils;

namespace BeyondIndustry.Debug
{
    public class DebugConsole
    {
        private static bool isOpen = false;
        private static string input = "";
        private static List<string> log = new List<string>();
        private static int scrollIndex = 0;
        
        // GUI-Elemente Positionen
        private static Rectangle consoleRect = new Rectangle(10, 100, 780, 500);
        private static Rectangle inputRect = new Rectangle(20, 550, 700, 30);
        private static Rectangle sendButtonRect = new Rectangle(730, 550, 50, 30);
        private static Rectangle closeButtonRect = new Rectangle(760, 110, 20, 20);
        private static Rectangle logRect = new Rectangle(20, 150, 760, 380);
        
        // Toggles
        public static bool ShowGrid = true;
        public static bool ShowFPS = false;
        public static bool ShowMousePos = false;
                // In DebugConsole.cs
        public static bool ShowCameraInfo = false;
        public static void Update()
        {
            // F1 zum Toggle
            if (Raylib.IsKeyPressed(KeyboardKey.F1))
            {
                isOpen = !isOpen;
                if (isOpen) input = "";
            }
            
            if (!isOpen) return;
            
            // Text-Input (wenn nicht im GUI-Textbox)
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if (key >= 32 && key <= 125 && input.Length < 50)
                    input += (char)key;
                key = Raylib.GetCharPressed();
            }
            
            // Backspace
            if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && input.Length > 0)
                input = input.Substring(0, input.Length - 1);
            
            // Enter zum Ausführen
            if (Raylib.IsKeyPressed(KeyboardKey.Enter) && input.Length > 0)
            {
                ExecuteCommand(input);
                input = "";
            }
        }
        
        private static void ExecuteCommand(string cmd)
        {
            string[] parts = cmd.ToLower().Split(' ');
            
            log.Add("> " + cmd);
            
            switch (parts[0])
            {
                case "help":
                    log.Add("=== BEFEHLE ===");
                    log.Add("help - Diese Hilfe");
                    log.Add("viewall - alles anzeigen");
                    log.Add("clear - Console leeren");
                    log.Add("grid - Grid an/aus");
                    log.Add("fps - FPS an/aus");
                    log.Add("mousepos - Mauspos an/aus");
                    log.Add("fill <wert> - Grid füllen");
                    log.Add("clearmap - Map leeren");
                    break;
                    
                case "clear":
                    log.Clear();
                    break;

                case "viewall":
                    ShowFPS = !ShowFPS;
                    ShowMousePos = !ShowMousePos;
                    log.Add($"Alles: {(ShowFPS ? "ON" : "OFF")}");
                    break;
                    
                case "grid":
                    ShowGrid = !ShowGrid;
                    log.Add($"Grid: {(ShowGrid ? "ON" : "OFF")}");
                    break;
                    
                case "fps":
                    ShowFPS = !ShowFPS;
                    log.Add($"FPS: {(ShowFPS ? "ON" : "OFF")}");
                    break;
                    
                case "mousepos":
                    ShowMousePos = !ShowMousePos;
                    log.Add($"MousePos: {(ShowMousePos ? "ON" : "OFF")}");
                    break;
                    
                default:
                    log.Add("Unbekannt! 'help' für Befehle");
                    break;
            }
            
            // Begrenze Log
            if (log.Count > 50)
                log.RemoveAt(0);
        }
        
        public static void Draw()
        {
            if (!isOpen) return;
            
            // ===== Haupt-Panel =====
            Raylib.DrawRectangleRec(consoleRect, GlobalColor.BACKGROUND_COLOR);
            Raylib.DrawRectangleLinesEx(consoleRect, 2, GlobalColor.FORGROUND_COLOR);
            
            // Titel
            Raylib.DrawText("DEBUG CONSOLE", 20, 110, 20, GlobalColor.FORGROUND_COLOR);
            Raylib.DrawText("[F1] Close", 650, 110, 16, GlobalColor.FORGROUND_COLOR);
            
            // ===== Close Button =====
            bool closeHover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButtonRect);
            Color closeColor = closeHover ? Color.Red : Color.DarkGray;
            Raylib.DrawRectangleRec(closeButtonRect, closeColor);
            Raylib.DrawText("X", 765, 112, 16, Color.White);
            
            if (closeHover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                isOpen = false;
            }
            
            // ===== Log-Bereich =====
            Raylib.DrawRectangleRec(logRect, new Color(20, 20, 20, 255));
            Raylib.DrawRectangleLinesEx(logRect, 1, Color.DarkGray);
            
            // Log-Text zeichnen (von unten nach oben)
            int yPos = (int)logRect.Y + (int)logRect.Height - 25;
            int visibleLines = 0;
            
            for (int i = log.Count - 1; i >= 0 && visibleLines < 18; i--)
            {
                Color textColor = log[i].StartsWith(">") ? Color.Lime : Color.LightGray;
                Raylib.DrawText(log[i], (int)logRect.X + 5, yPos, 14, textColor);
                yPos -= 20;
                visibleLines++;
            }
            
            // ===== Input-Box =====
            Raylib.DrawRectangleRec(inputRect, new Color(30, 30, 30, 255));
            Raylib.DrawRectangleLinesEx(inputRect, 2, Color.Green);
            Raylib.DrawText("> " + input + "_", (int)inputRect.X + 5, (int)inputRect.Y + 7, 16, Color.White);
            
            // ===== Send Button =====
            bool sendHover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), sendButtonRect);
            Color sendColor = sendHover ? Color.Green : Color.DarkGreen;
            Raylib.DrawRectangleRec(sendButtonRect, sendColor);
            Raylib.DrawText(">", (int)sendButtonRect.X + 18, (int)sendButtonRect.Y + 5, 20, Color.White);
            
            if (sendHover && Raylib.IsMouseButtonPressed(MouseButton.Left) && input.Length > 0)
            {
                ExecuteCommand(input);
                input = "";
            }
        }
        
        public static bool IsOpen() => isOpen;
    }
}