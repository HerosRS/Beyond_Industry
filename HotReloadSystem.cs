using System;
using System.IO;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Utils
{
    public static class HotReloadSystem
    {
        private class FileWatch
        {
            public string Path { get; set; }
            public DateTime LastWrite { get; set; }
            public bool Changed { get; set; }
            
            public FileWatch(string path)
            {
                Path = path;
                LastWrite = File.Exists(path) ? File.GetLastWriteTime(path) : DateTime.MinValue;
                Changed = false;
            }
        }
        
        private static Dictionary<string, FileWatch> watchedFiles = new Dictionary<string, FileWatch>();
        private static float checkInterval = 1.0f;
        private static float timeSinceLastCheck = 0f;
        
        public static bool Enabled { get; set; } = true;
        public static bool NeedsRestart { get; private set; } = false;
        
        // ===== EVENTS =====
        public static event Action<string>? OnFileChanged;
        
        // ===== WATCH FILE =====
        public static void WatchFile(string key, string path)
        {
            if (!watchedFiles.ContainsKey(key))
            {
                watchedFiles[key] = new FileWatch(path);
                Console.WriteLine($"[HotReload] Watching: {key} → {path}");
            }
        }
        
        // ===== UPDATE =====
        public static void Update(float deltaTime)
        {
            if (!Enabled) return;
            
            timeSinceLastCheck += deltaTime;
            
            if (timeSinceLastCheck >= checkInterval)
            {
                timeSinceLastCheck = 0f;
                CheckForChanges();
            }
        }
        
        // ===== CHECK FOR CHANGES =====
        private static void CheckForChanges()
        {
            foreach (var kvp in watchedFiles)
            {
                var watch = kvp.Value;
                
                if (!File.Exists(watch.Path))
                    continue;
                
                try
                {
                    DateTime currentWrite = File.GetLastWriteTime(watch.Path);
                    
                    if (currentWrite > watch.LastWrite)
                    {
                        watch.LastWrite = currentWrite;
                        watch.Changed = true;
                        
                        Console.WriteLine($"[HotReload] ⚠ Change detected: {kvp.Key}");
                        Console.WriteLine($"[HotReload] Press F6 to reload (game will restart)");
                        
                        OnFileChanged?.Invoke(kvp.Key);
                        NeedsRestart = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HotReload] Error checking {kvp.Key}: {ex.Message}");
                }
            }
        }
        
        // ===== GET CHANGED FILES =====
        public static List<string> GetChangedFiles()
        {
            var changed = new List<string>();
            
            foreach (var kvp in watchedFiles)
            {
                if (kvp.Value.Changed)
                {
                    changed.Add(kvp.Key);
                }
            }
            
            return changed;
        }
        
        // ===== RESET =====
        public static void ResetChanges()
        {
            foreach (var watch in watchedFiles.Values)
            {
                watch.Changed = false;
            }
            NeedsRestart = false;
        }
        
        // ===== CLEANUP =====
        public static void Cleanup()
        {
            watchedFiles.Clear();
            NeedsRestart = false;
            Console.WriteLine("[HotReload] Cleaned up");
        }
    }
}