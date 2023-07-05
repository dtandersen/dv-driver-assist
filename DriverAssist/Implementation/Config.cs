using BepInEx.Configuration;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class BepInExCruiseControlConfig : CruiseControlConfig
    {
        private ConfigEntry<string> maxTorque;
        private ConfigEntry<string> offset;
        private ConfigEntry<string> diff;
        private ConfigEntry<KeyboardShortcut> faster;
        private ConfigEntry<KeyboardShortcut> slower;
        private ConfigEntry<KeyboardShortcut> toggle;
        private ConfigEntry<string> showStats;

        public BepInExCruiseControlConfig(ConfigFile config)
        {
            offset = config.Bind("CruiseControl", "Offset", "0", "This amount is subtracted from setpoint before adding the diff");
            diff = config.Bind("CruiseControl", "Diff", "2.5", "speed = setpoint +/- diff");
            maxTorque = config.Bind("DE2", "MaxTorque", "25000", "Maximum torque");

            faster = config.Bind("Hotkeys", "Faster", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftControl));
            slower = config.Bind("Hotkeys", "Slower", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftControl));
            toggle = config.Bind("Hotkeys", "Toggle", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftControl));

            showStats = config.Bind("UI", "ShowStats", "1", "Show stats");
        }

        public int MaxTorque
        {
            get
            {
                if (!int.TryParse(maxTorque.Value, out int result))
                {
                    return 0;
                }

                return result;
            }
        }

        public float Offset
        {
            get
            {
                if (!float.TryParse(offset.Value, out float result))
                {
                    return 0;
                }

                return result;
            }
        }

        public float Diff
        {
            get
            {
                if (!float.TryParse(diff.Value, out float result))
                {
                    return 0;
                }

                return result;
            }
        }

        public float UpdateInterval { get { return 1; } }
    }
}
