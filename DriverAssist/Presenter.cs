using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist
{
    public interface Presenter
    {
        void OnGui();
        void OnUpdate();
        void OnFixedUpdate();
    }

    public interface UnifiedSettings : DriverAssistSettings, CruiseControlSettings
    {
    }

    public interface DriverAssistSettings
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
