using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public class CopperOre : Resource
    {
        public CopperOre()
        {
            Name = "CopperOre";
            DisplayName = "Kupfererz";
            Color = new Color(184, 115, 51, 255);
            Type = ResourceType.RawOre;
            Description = "Rohes Kupfer direkt aus der Mine. Muss geschmolzen werden.";
            
            StackSize = 100;
            Weight = 1.0f;
        }
    }
}