using Raylib_cs;

namespace BeyondIndustry.Core
{
    /// <summary>
    /// Repräsentiert eine Ressource/Item im Spiel
    /// </summary>
    public class Item
    {
        public string Name { get; }
        public Color Color { get; }
        
        public Item(string name, Color color)
        {
            Name = name;
            Color = color;
        }
        
        // Vordefinierte Items (wie ein Katalog)
        public static readonly Item Ore = new Item("Ore", new Color(139, 69, 19, 255));      // Braun
        public static readonly Item IronPlate = new Item("Iron", new Color(169, 169, 169, 255)); // Grau
        public static readonly Item CopperOre = new Item("Copper", new Color(184, 115, 51, 255)); // Kupfer
    }
}