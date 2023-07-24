using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist
{
#pragma warning disable IDE1006
    public interface UnifiedSettings : DriverAssistSettings, CruiseControlSettings
#pragma warning restore IDE1006
    {
    }

#pragma warning disable IDE1006
    public interface DriverAssistSettings
#pragma warning restore IDE1006
    {
        int[] AccelerateKeys { get; }
        int[] DecelerateKeys { get; }
        int[] ToggleKeys { get; }
        int[] Upshift { get; }
        int[] Downshift { get; }
        int[] DumpPorts { get; }
        bool ShowStats { get; }
    }
}
