using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public class IronPlate : Resource
    {
        public IronPlate()
        {
            Name = "IronPlate";
            DisplayName = "Eisenplatte";
            Color = new Color(169, 169, 169, 255);
            Type = ResourceType.ProcessedOre;
            Description = "Geschmolzenes Eisen, bereit zur Weiterverarbeitung.";
            
            StackSize = 100;
            Weight = 0.5f;
        }
    }
}