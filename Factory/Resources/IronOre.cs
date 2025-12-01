using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public class IronOre : Resource
    {
        public IronOre()
        {
            Name = "Iron Ore";
            DisplayName = "Eisenerz";
            Color = new Color(139, 69, 19, 255);
            Type = ResourceType.RawOre;
            Description = "Rohes Eisenerz direkt aus der Mine. Muss geschmolzen werden.";
            
            StackSize = 100;
            Weight = 1.0f;
        }
    }
}