using Raylib_cs;
using BeyondIndustry.Core;

namespace BeyondIndustry.Buildings
{
    /// <summary>
    /// Produziert Eisen-Platten aus Erz
    /// </summary>
    public class Smelter : ProducerBuilding
    {
        public Smelter() : base(
            name: "Smelter",
            color: Color.Orange,
            outputItem: Item.IronPlate,
            productionTime: 3.0f,  // 3 Sekunden pro Eisen
            maxStorage: 5
        )
        {
        }
    }
}