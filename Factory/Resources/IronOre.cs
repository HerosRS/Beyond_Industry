using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public class IronOre : Resource
    {
        public IronOre()
        {
            Name = "IronOre";
            DisplayName = "Eisenerz";
            Color = new Color(139, 69, 19, 255);
            Type = ResourceType.RawOre;
            Description = "Rohes Eisen direkt aus der Mine. Muss geschmolzen werden.";
            
            StackSize = 100;
            Weight = 1.0f;
        }
    }
}