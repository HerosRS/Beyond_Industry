using Raylib_cs;
using BeyondIndustry.Factory.Resources;
using System.Collections.Generic;

namespace BeyondIndustry.UI
{
    public static class ResourceUI
    {
        public static void DrawResourceList(int x, int y)
        {
            Raylib.DrawText("=== RESSOURCEN ===", x, y, 20, Color.White);
            
            int offsetY = 25;
            List<Resource> allResources = ResourceRegistry.GetAllResources();
            
            foreach (var resource in allResources)
            {
                // Farb-Box
                Raylib.DrawRectangle(x, y + offsetY, 20, 20, resource.Color);
                
                // Name
                Raylib.DrawText(resource.DisplayName, x + 25, y + offsetY, 16, Color.Black);
                
                // Typ
                string typeText = resource.Type.ToString();
                Raylib.DrawText($"[{typeText}]", x + 200, y + offsetY, 14, Color.Gray);
                
                offsetY += 25;
            }
        }
        
        public static void DrawResourcesByType(int x, int y, ResourceType type)
        {
            Raylib.DrawText($"=== {type} ===", x, y, 20, Color.White);
            
            int offsetY = 25;
            List<Resource> resources = ResourceRegistry.GetByType(type);
            
            foreach (var resource in resources)
            {
                Raylib.DrawRectangle(x, y + offsetY, 20, 20, resource.Color);
                Raylib.DrawText(resource.DisplayName, x + 25, y + offsetY, 16, Color.Black);
                offsetY += 25;
            }
        }
    }
}