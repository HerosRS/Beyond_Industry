using System;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    // ===== RESSOURCEN-REGISTRY =====
    public static class ResourceRegistry
    {
        private static Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
        
        // ===== INITIALISIERUNG =====
        public static void Initialize()
        {
            resources.Clear();
            
            Console.WriteLine("[ResourceRegistry] Initialisiere Ressourcen...");
            
            // ===== HIER ALLE RESSOURCEN REGISTRIEREN =====
            // Roherze
            Register(new IronOre());
            Register(new Coal());
            
            // Verarbeitete Erze
            Register(new IronPlate());

            Console.WriteLine($"[ResourceRegistry] {resources.Count} Ressourcen registriert");
        }
        
        // ===== REGISTRIERUNG =====
        private static void Register(Resource resource)
        {
            if (resources.ContainsKey(resource.Name))
            {
                Console.WriteLine($"[ResourceRegistry] WARNUNG: Ressource '{resource.Name}' bereits registriert!");
                return;
            }
            
            resources[resource.Name] = resource;
            Console.WriteLine($"[ResourceRegistry] Registriert: {resource.Name} ({resource.DisplayName})");
        }
        
        // ===== ABRUFEN =====
        public static Resource? Get(string name)
        {
            return resources.GetValueOrDefault(name);
        }
        
        public static Color GetColor(string name)
        {
            Resource? resource = Get(name);
            if (resource == null)
            {
                Console.WriteLine($"[ResourceRegistry] WARNUNG: Ressource '{name}' nicht gefunden! Verwende White.");
                return Color.White;
            }
            return resource.Color;
        }
        
        public static string GetDisplayName(string name)
        {
            Resource? resource = Get(name);
            return resource?.DisplayName ?? name;
        }
        
        public static List<Resource> GetAllResources()
        {
            return new List<Resource>(resources.Values);
        }
        
        public static List<Resource> GetByType(ResourceType type)
        {
            List<Resource> result = new List<Resource>();
            foreach (var resource in resources.Values)
            {
                if (resource.Type == type)
                {
                    result.Add(resource);
                }
            }
            return result;
        }
        
        public static bool Exists(string name)
        {
            return resources.ContainsKey(name);
        }
        
        // ===== DEBUG =====
        public static void PrintAll()
        {
            Console.WriteLine("\n===== REGISTRIERTE RESSOURCEN =====");
            foreach (var resource in resources.Values)
            {
                Console.WriteLine($"{resource.Name} ({resource.DisplayName}) - {resource.Type}");
            }
            Console.WriteLine("====================================\n");
        }
    }
}