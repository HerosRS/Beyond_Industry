using System;
using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public static class ResourceRegistry
    {
        private static Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
        
        public static void Initialize()
        {
            resources.Clear();
            
            Console.WriteLine("[ResourceRegistry] Initialisiere Ressourcen...");
            
            // ===== ROHERZE =====
            Register(new IronOre());
            Register(new CopperOre());
            Register(new Coal());
            
            // ===== VERARBEITETE ERZE =====
            Register(new IronPlate());
            //Register(new CopperPlate());

            Console.WriteLine($"[ResourceRegistry] {resources.Count} Ressourcen registriert");
        }
        
        private static void Register(Resource resource)
        {
            if (resources.ContainsKey(resource.Name))
            {
                Console.WriteLine($"[ResourceRegistry] WARNUNG: Ressource '{resource.Name}' bereits registriert!");
                return;
            }
            
            resources[resource.Name] = resource;
            Console.WriteLine($"[ResourceRegistry] Registriert: {resource.DisplayName} ({resource.Name})");
        }
        
        public static Resource? Get(string name)
        {
            if (resources.TryGetValue(name, out Resource? resource))
            {
                return resource;
            }
            
            Console.WriteLine($"[ResourceRegistry] WARNUNG: Ressource '{name}' nicht gefunden! Verfügbare: {string.Join(", ", resources.Keys)}");
            return null;
        }
        
        public static Color GetColor(string name)
        {
            Resource? resource = Get(name);
            if (resource == null)
            {
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
        
        public static void PrintAll()
        {
            Console.WriteLine("\n===== REGISTRIERTE RESSOURCEN =====");
            foreach (var resource in resources.Values)
            {
                Console.WriteLine($"{resource.DisplayName} ({resource.Name}) - {resource.Type} - RGB({resource.Color.R}, {resource.Color.G}, {resource.Color.B})");
            }
            Console.WriteLine("====================================\n");
        }
    }
}
