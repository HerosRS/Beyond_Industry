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
            Description = "Brennstoff für Öfen und Energieerzeugung.";
            
            StackSize = 100;
            Weight = 0.8f;
        }
    }
}