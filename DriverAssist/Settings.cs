using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist
{
    public interface KeyMatcher
    {
        bool IsKeyPressed();
    }

    public interface UnifiedSettings : DriverAssistSettings, CruiseControlSettings
    {
    }

    public interface DriverAssistSettings
    {
        KeyMatcher AccelerateKeys { get; }
        KeyMatcher DecelerateKeys { get; }
        KeyMatcher ToggleKeys { get; }
        KeyMatcher Upshift { get; }
        KeyMatcher Downshift { get; }
        KeyMatcher DumpPorts { get; }
        bool ShowStats { get; }
        bool ShowJobs { get; }
    }
}
