using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    public class Coal : Resource
    {
        public Coal()
        {
            Name = "Coal";
            DisplayName = "Kohle";
            Color = new Color(50, 50, 50, 255);
            Type = ResourceType.RawOre;
            Description = "Brennstoff für Öfen und Generatoren.";
            
            StackSize = 100;
            Weight = 0.5f;
        }
    }
}