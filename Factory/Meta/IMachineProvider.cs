using System.Collections.Generic;
using Raylib_cs;

namespace BeyondIndustry.Factory
{
    // ===== INTERFACE FÜR MASCHINEN DIE SICH SELBST DEFINIEREN =====
    public interface IMachineProvider
    {
        // Jede Maschinen-Klasse muss diese Methode implementieren
        // Sie gibt alle Varianten dieser Maschine zurück
        List<MachineDefinition> GetDefinitions(Model defaultModel);
    }
}