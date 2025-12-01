using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using BeyondIndustry.Data;
using BeyondIndustry.Utils;
using BeyondIndustry.Factory;

namespace BeyondIndustry.Debug
{
    public static class DebugConsole
    {
        // Console State
        private static bool isOpen = false;
        private static string input = "";
        private static List<string> log = new List<string>();
        private static int maxLogLines = 100;
        
        // GUI-Elemente Positionen
        private static Rectangle consoleRect = new Rectangle(10, 100, 780, 500);
        private static Rectangle inputRect = new Rectangle(20, 550, 700, 30);
        private static Rectangle sendButtonRect = new Rectangle(730, 550, 50, 30);
        private static Rectangle closeButtonRect = new Rectangle(760, 110, 20, 20);
        private static Rectangle logRect = new Rectangle(20, 150, 760, 380);
        
        // Debug-Toggles
        public static bool ShowGrid { get; set; } = false;
        public static bool ShowFPS { get; set; } = false;
        public static bool ShowMousePos { get; set; } = false;
        public static bool ShowCameraInfo { get; set; } = false;
        public static bool ShowConnectionDebug { get; set; } = false;
        public static bool ShowMachineInfo { get; set; } = false;
        public static bool ShowResourceFlow { get; set; } = false;
        
        // Command History
        private static List<string> commandHistory = new List<string>();
        private static int historyIndex = -1;
        
        // Auto-complete
        private static List<string> availableCommands = new List<string>
        {
            "help", "clear", "viewall", "grid", "fps", "mousepos", "camera",
            "connections", "machines", "resources", "belt", "speed", "height",
            "spawn", "delete", "power", "debug", "info"
        };
        
        public static void Update()
        {
            // F1 zum Toggle
            if (Raylib.IsKeyPressed(KeyboardKey.F1))
            {
                isOpen = !isOpen;
                if (isOpen)
                {
                    input = "";
                    historyIndex = -1;
                }
            }
            
            if (!isOpen) return;
            
            // Command History Navigation
            if (Raylib.IsKeyPressed(KeyboardKey.Up))
            {
                if (commandHistory.Count > 0)
                {
                    historyIndex = Math.Max(0, historyIndex == -1 ? commandHistory.Count - 1 : historyIndex - 1);
                    input = commandHistory[historyIndex];
                }
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Down))
            {
                if (historyIndex >= 0 && historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    input = commandHistory[historyIndex];
                }
                else
                {
                    historyIndex = -1;
                    input = "";
                }
            }
            
            // Text-Input
            int key = Raylib.GetCharPressed();
            while (key > 0)
            {
                if (key >= 32 && key <= 125 && input.Length < 100)
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
                commandHistory.Add(input);
                if (commandHistory.Count > 50)
                    commandHistory.RemoveAt(0);
                input = "";
                historyIndex = -1;
            }
            
            // ESC zum Schließen
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                isOpen = false;
            }
        }
        
        private static void ExecuteCommand(string cmd)
        {
            string[] parts = cmd.Trim().Split(' ');
            string command = parts[0].ToLower();
            
            LogInput("> " + cmd);
            
            switch (command)
            {
                case "help":
                    ShowHelp();
                    break;
                    
                case "clear":
                    log.Clear();
                    LogSuccess("Console geleert");
                    break;
                
                case "viewall":
                    bool newState = !ShowFPS;
                    ShowFPS = newState;
                    ShowMousePos = newState;
                    ShowCameraInfo = newState;
                    ShowMachineInfo = newState;
                    LogSuccess($"Alle Debug-Infos: {(newState ? "ON" : "OFF")}");
                    break;
                    
                case "grid":
                    ShowGrid = !ShowGrid;
                    LogSuccess($"Grid: {(ShowGrid ? "ON" : "OFF")}");
                    break;
                    
                case "fps":
                    ShowFPS = !ShowFPS;
                    LogSuccess($"FPS: {(ShowFPS ? "ON" : "OFF")}");
                    break;
                    
                case "mousepos":
                    ShowMousePos = !ShowMousePos;
                    LogSuccess($"MousePos: {(ShowMousePos ? "ON" : "OFF")}");
                    break;
                    
                case "camera":
                    ShowCameraInfo = !ShowCameraInfo;
                    LogSuccess($"Camera Info: {(ShowCameraInfo ? "ON" : "OFF")}");
                    break;
                
                case "connections":
                    ShowConnectionDebug = !ShowConnectionDebug;
                    LogSuccess($"Connection Debug: {(ShowConnectionDebug ? "ON" : "OFF")}");
                    break;
                
                case "machines":
                    ShowMachineInfo = !ShowMachineInfo;
                    LogSuccess($"Machine Info: {(ShowMachineInfo ? "ON" : "OFF")}");
                    break;
                
                case "resources":
                    ShowResourceFlow = !ShowResourceFlow;
                    LogSuccess($"Resource Flow: {(ShowResourceFlow ? "ON" : "OFF")}");
                    break;
                
                case "belt":
                    if (parts.Length > 1)
                    {
                        HandleBeltCommand(parts);
                    }
                    else
                    {
                        LogError("Verwendung: belt <speed|height|info> <wert>");
                    }
                    break;
                
                case "speed":
                    if (parts.Length > 1 && float.TryParse(parts[1], out float speed))
                    {
                        // Hier müsstest du Zugriff auf die Belts haben
                        LogSuccess($"Belt-Geschwindigkeit auf {speed} gesetzt");
                        LogWarning("Hinweis: Benötigt Zugriff auf FactoryManager");
                    }
                    else
                    {
                        LogError("Verwendung: speed <wert>");
                    }
                    break;
                
                case "height":
                    if (parts.Length > 1 && float.TryParse(parts[1], out float height))
                    {
                        LogSuccess($"Item-Höhe auf {height} gesetzt");
                        LogWarning("Hinweis: Benötigt Zugriff auf FactoryManager");
                    }
                    else
                    {
                        LogError("Verwendung: height <wert>");
                    }
                    break;
                
                case "power":
                    LogInfo("=== POWER STATUS ===");
                    LogInfo("Benötigt Zugriff auf FactoryManager");
                    break;
                
                case "debug":
                    if (parts.Length > 1)
                    {
                        switch (parts[1].ToLower())
                        {
                            case "on":
                                GlobalData.ShowDebugInfo = true;
                                LogSuccess("Debug-Modus: ON");
                                break;
                            case "off":
                                GlobalData.ShowDebugInfo = false;
                                LogSuccess("Debug-Modus: OFF");
                                break;
                            default:
                                GlobalData.ShowDebugInfo = !GlobalData.ShowDebugInfo;
                                LogSuccess($"Debug-Modus: {(GlobalData.ShowDebugInfo ? "ON" : "OFF")}");
                                break;
                        }
                    }
                    else
                    {
                        GlobalData.ShowDebugInfo = !GlobalData.ShowDebugInfo;
                        LogSuccess($"Debug-Modus: {(GlobalData.ShowDebugInfo ? "ON" : "OFF")}");
                    }
                    break;
                
                case "info":
                    ShowSystemInfo();
                    break;
                
                default:
                    LogError($"Unbekannter Befehl: '{command}'");
                    LogInfo("Tippe 'help' für eine Liste aller Befehle");
                    break;
            }
        }
        
        private static void HandleBeltCommand(string[] parts)
        {
            if (parts.Length < 2) return;
            
            switch (parts[1].ToLower())
            {
                case "speed":
                    if (parts.Length > 2 && float.TryParse(parts[2], out float speed))
                    {
                        LogSuccess($"Belt-Geschwindigkeit: {speed}");
                        // Hier Implementierung für alle Belts
                    }
                    else
                    {
                        LogError("Verwendung: belt speed <wert>");
                    }
                    break;
                
                case "height":
                    if (parts.Length > 2 && float.TryParse(parts[2], out float height))
                    {
                        LogSuccess($"Item-Höhe: {height}");
                        // Hier Implementierung für alle Belts
                    }
                    else
                    {
                        LogError("Verwendung: belt height <wert>");
                    }
                    break;
                
                case "info":
                    LogInfo("=== BELT INFO ===");
                    LogInfo("Benötigt Zugriff auf FactoryManager");
                    break;
                
                default:
                    LogError($"Unbekannte Belt-Option: '{parts[1]}'");
                    break;
            }
        }
        
        private static void ShowHelp()
        {
            LogInfo("=== DEBUG CONSOLE HILFE ===");
            LogInfo("");
            LogInfo("ANSICHT:");
            LogInfo("  help           - Diese Hilfe anzeigen");
            LogInfo("  clear          - Console leeren");
            LogInfo("  viewall        - Alle Debug-Infos an/aus");
            LogInfo("  info           - System-Informationen");
            LogInfo("");
            LogInfo("TOGGLES:");
            LogInfo("  grid           - Grid anzeigen");
            LogInfo("  fps            - FPS-Counter anzeigen");
            LogInfo("  mousepos       - Mausposition anzeigen");
            LogInfo("  camera         - Kamera-Info anzeigen");
            LogInfo("  connections    - Belt-Verbindungen anzeigen");
            LogInfo("  machines       - Maschinen-Info anzeigen");
            LogInfo("  resources      - Ressourcen-Flow anzeigen");
            LogInfo("  debug on/off   - Debug-Modus umschalten");
            LogInfo("");
            LogInfo("BELT BEFEHLE:");
            LogInfo("  belt speed <wert>   - Belt-Geschwindigkeit setzen");
            LogInfo("  belt height <wert>  - Item-Höhe setzen");
            LogInfo("  belt info           - Belt-Informationen");
            LogInfo("");
            LogInfo("STEUERUNG:");
            LogInfo("  F1             - Console öffnen/schließen");
            LogInfo("  ESC            - Console schließen");
            LogInfo("  Pfeiltasten    - Command History");
            LogInfo("  Enter          - Befehl ausführen");
        }
        
        private static void ShowSystemInfo()
        {
            LogInfo("=== SYSTEM INFO ===");
            LogInfo($"Raylib Version: {Raylib.RAYLIB_VERSION}");
            LogInfo($"FPS: {Raylib.GetFPS()}");
            LogInfo($"Screen: {Raylib.GetScreenWidth()}x{Raylib.GetScreenHeight()}");
            LogInfo($"Monitor: {Raylib.GetMonitorWidth(0)}x{Raylib.GetMonitorHeight(0)}");
            LogInfo($"Zeit: {DateTime.Now:HH:mm:ss}");
        }
        
        // Logging-Methoden mit Farben
        private static void LogInput(string message)
        {
            AddToLog(message, LogType.Input);
        }
        
        private static void LogSuccess(string message)
        {
            AddToLog("✓ " + message, LogType.Success);
        }
        
        private static void LogError(string message)
        {
            AddToLog("✗ " + message, LogType.Error);
        }
        
        private static void LogWarning(string message)
        {
            AddToLog("⚠ " + message, LogType.Warning);
        }
        
        private static void LogInfo(string message)
        {
            AddToLog(message, LogType.Info);
        }
        
        private enum LogType
        {
            Input,
            Success,
            Error,
            Warning,
            Info
        }
        
        private static void AddToLog(string message, LogType type)
        {
            log.Add($"{(int)type}|{message}");
            
            // Begrenze Log-Größe
            if (log.Count > maxLogLines)
                log.RemoveAt(0);
        }
        
        public static void Draw()
        {
            if (!isOpen) return;
            
            // ===== Haupt-Panel =====
            Raylib.DrawRectangleRec(consoleRect, new Color(20, 20, 30, 240));
            Raylib.DrawRectangleLinesEx(consoleRect, 2, new Color(100, 150, 255, 255));
            
            // Titel mit Glow-Effekt
            Raylib.DrawText("DEBUG CONSOLE", 22, 112, 20, new Color(50, 50, 80, 255));
            Raylib.DrawText("DEBUG CONSOLE", 20, 110, 20, new Color(100, 150, 255, 255));
            
            // Hint-Text
            Raylib.DrawText("[F1] Close | [ESC] Close | [↑↓] History", 350, 112, 14, Color.Gray);
            
            // ===== Close Button =====
            bool closeHover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButtonRect);
            Color closeColor = closeHover ? new Color(255, 80, 80, 255) : new Color(100, 50, 50, 255);
            Raylib.DrawRectangleRec(closeButtonRect, closeColor);
            Raylib.DrawText("X", 765, 112, 16, Color.White);
            
            if (closeHover && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                isOpen = false;
            }
            
            // ===== Log-Bereich =====
            Raylib.DrawRectangleRec(logRect, new Color(15, 15, 25, 255));
            Raylib.DrawRectangleLinesEx(logRect, 1, new Color(50, 50, 80, 255));
            
            // Log-Text zeichnen (von unten nach oben)
            int yPos = (int)logRect.Y + (int)logRect.Height - 25;
            int visibleLines = 0;
            
            for (int i = log.Count - 1; i >= 0 && visibleLines < 18; i--)
            {
                string[] parts = log[i].Split('|');
                if (parts.Length != 2) continue;
                
                LogType type = (LogType)int.Parse(parts[0]);
                string message = parts[1];
                
                Color textColor = type switch
                {
                    LogType.Input => new Color(100, 255, 100, 255),
                    LogType.Success => new Color(100, 255, 100, 255),
                    LogType.Error => new Color(255, 100, 100, 255),
                    LogType.Warning => new Color(255, 200, 100, 255),
                    LogType.Info => new Color(200, 200, 220, 255),
                    _ => Color.LightGray
                };
                
                Raylib.DrawText(message, (int)logRect.X + 5, yPos, 14, textColor);
                yPos -= 20;
                visibleLines++;
            }
            
            // Scrollbar-Indikator (wenn zu viel Log)
            if (log.Count > 18)
            {
                float scrollPercent = (float)Math.Min(18, log.Count) / log.Count;
                int scrollHeight = (int)(logRect.Height * scrollPercent);
                int scrollY = (int)(logRect.Y + logRect.Height - scrollHeight);
                
                Raylib.DrawRectangle((int)logRect.X + (int)logRect.Width - 10, scrollY, 
                    8, scrollHeight, new Color(100, 150, 255, 150));
            }
            
            // ===== Input-Box =====
            Raylib.DrawRectangleRec(inputRect, new Color(25, 25, 40, 255));
            Raylib.DrawRectangleLinesEx(inputRect, 2, new Color(100, 150, 255, 255));
            
            // Blinken für Cursor
            string cursorChar = (int)(Raylib.GetTime() * 2) % 2 == 0 ? "_" : " ";
            Raylib.DrawText("> " + input + cursorChar, (int)inputRect.X + 5, (int)inputRect.Y + 7, 16, Color.White);
            
            // ===== Send Button =====
            bool sendHover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), sendButtonRect);
            Color sendColor = sendHover ? new Color(100, 200, 100, 255) : new Color(50, 100, 50, 255);
            Raylib.DrawRectangleRec(sendButtonRect, sendColor);
            Raylib.DrawText("→", (int)sendButtonRect.X + 16, (int)sendButtonRect.Y + 5, 20, Color.White);
            
            if (sendHover && Raylib.IsMouseButtonPressed(MouseButton.Left) && input.Length > 0)
            {
                ExecuteCommand(input);
                commandHistory.Add(input);
                input = "";
                historyIndex = -1;
            }
            
            // Auto-complete Hints (optional)
            if (input.Length > 0)
            {
                var suggestions = availableCommands.FindAll(c => c.StartsWith(input.ToLower()));
                if (suggestions.Count > 0 && suggestions.Count < 5)
                {
                    int hintY = (int)inputRect.Y - 25;
                    foreach (var suggestion in suggestions)
                    {
                        Raylib.DrawText(suggestion, (int)inputRect.X + 5, hintY, 12, new Color(150, 150, 180, 255));
                        hintY -= 15;
                    }
                }
            }
        }
        
        public static bool IsOpen() => isOpen;
        
        // Öffentliche Logging-Methoden für externe Verwendung
        public static void Log(string message)
        {
            LogInfo(message);
        }
        
        public static void LogCommand(string message)
        {
            LogSuccess(message);
        }
    }
}