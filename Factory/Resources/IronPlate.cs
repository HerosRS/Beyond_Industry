using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public class IronPlate : Resource
    {
        public IronPlate()
        {
            Name = "Iron Plate";
            DisplayName = "Eisenplatte";
            Color = new Color(192, 192, 192, 255);
            Type = ResourceType.ProcessedOre;
            Description = "Geschmolzenes Eisen. Grundmaterial für viele Komponenten.";
            
            StackSize = 100;
            Weight = 0.8f;
        }
    }
}