using Raylib_cs;
using BeyondIndustry.Core;

namespace BeyondIndustry.Buildings
{
    /// <summary>
    /// Produziert Erz (Ore) über Zeit
    /// </summary>
    public class Miner : ProducerBuilding
    {
        public Miner() : base(
            name: "Miner",
            color: new Color(139, 69, 19, 255),  // Braun
            outputItem: Item.IronOre,
            productionTime: 2.0f,  // 2 Sekunden pro Erz
            maxStorage: 10
        )
        {
        }
    }
}