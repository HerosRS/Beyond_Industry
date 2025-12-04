using System.Collections.Generic;

namespace BeyondIndustry.Data
{
    public interface ISaveable
    {
        string GetSaveId();
        Dictionary<string, object> Serialize();
        void Deserialize(Dictionary<string, object> data);
    }
}