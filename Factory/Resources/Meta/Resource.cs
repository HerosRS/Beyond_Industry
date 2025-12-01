using Raylib_cs;

namespace BeyondIndustry.Factory.Resources
{
    // ===== RESSOURCEN-TYPEN =====
    public enum ResourceType
    {
        RawOre,         // Roherze
        ProcessedOre,   // Verarbeitete Erze
        Component,      // Komponenten
        Fluid,          // Flüssigkeiten
        Energy          // Energie
    }
    
    // ===== BASIS-RESSOURCE =====
    public abstract class Resource
    {
        public string Name { get; protected set; } = "";
        public string DisplayName { get; protected set; } = "";
        public Color Color { get; protected set; }
        public ResourceType Type { get; protected set; }
        public string Description { get; protected set; } = "";
        
        // Physikalische Eigenschaften (optional für später)
        public float StackSize { get; protected set; } = 100;
        public bool IsFluid { get; protected set; } = false;
        public float Weight { get; protected set; } = 1.0f;
        
        protected Resource()
        {
        }
    }
}