using DriverAssist.Cruise;

namespace DriverAssist
{
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
        bool ShowJobs { get; }
    }
}
