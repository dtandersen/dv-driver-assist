using System.Collections.Generic;
using BepInEx.Configuration;
using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class BepInExDriverAssistConfig : DriverAssistConfig, CruiseControlConfig
    {
        private ConfigEntry<float> offset;
        private ConfigEntry<float> diff;
        private ConfigEntry<int> minTorque;
        private ConfigEntry<int> minAmps;
        private ConfigEntry<int> maxAmps;
        private ConfigEntry<int> maxTemperature;
        private ConfigEntry<int> overdriveTemperature;
        private ConfigEntry<bool> overdriveEnabled;
        // private ConfigEntry<string> acceleration;
        // private ConfigEntry<string> deceleration;
        private ConfigEntry<KeyboardShortcut> faster;
        private ConfigEntry<KeyboardShortcut> slower;
        private ConfigEntry<KeyboardShortcut> toggle;
        private ConfigEntry<bool> showStats;

        public BepInExDriverAssistConfig(ConfigFile config)
        {
            diff = config.Bind("Speed", "Diff", 2.5f, "speed = setpoint +/- diff");
            offset = config.Bind("Speed", "Offset", 0f, "This amount is subtracted from setpoint before adding the diff");
            minTorque = config.Bind("Acceleration", "MinTorque", 20000, "Upshift torque threshold");
            minAmps = config.Bind("Acceleration", "MinAmps", 400, "Upshift amperage threshold");
            maxAmps = config.Bind("Acceleration", "MaxAmps", 725, "Downshift amperage threshold");
            maxTemperature = config.Bind("Acceleration", "MaxTemperature", 104, "Downshift temperature threshhold");
            overdriveTemperature = config.Bind("Acceleration", "OverdriveTemperature", 118, "Downshift temperature threshhold during overdrive");
            overdriveEnabled = config.Bind("Acceleration", "Overdrive", true, "Enable overdrive when train is slowing down");
            // acceleration = config.Bind("CruiseControl", "Acceleration", "DriverAssist.Cruise.PredictiveAcceleration", "Maximum torque");
            // deceleration = config.Bind("CruiseControl", "Deceleration", "DriverAssist.Cruise.PredictiveDeceleration", "Maximum torque");

            faster = config.Bind("Hotkeys", "Faster", new KeyboardShortcut(KeyCode.PageUp));
            slower = config.Bind("Hotkeys", "Slower", new KeyboardShortcut(KeyCode.PageDown));
            toggle = config.Bind("Hotkeys", "Toggle", new KeyboardShortcut(KeyCode.RightControl));

            showStats = config.Bind("UI", "ShowStats", true, "Show stats");
        }

        public int MinTorque
        {
            get
            {
                return minTorque.Value;
            }
        }

        public int MinAmps
        {
            get
            {
                return minAmps.Value;
            }
        }

        public int MaxAmps
        {
            get
            {
                return maxAmps.Value;
            }
        }

        public int MaxTemperature
        {
            get
            {
                return maxTemperature.Value;
            }
        }

        public int OverdriveTemperature
        {
            get
            {
                return overdriveTemperature.Value;
            }
        }

        public bool OverdriveEnabled
        {
            get
            {
                return overdriveEnabled.Value;
            }
        }

        public float Offset
        {
            get
            {
                return offset.Value;
            }
        }

        public float Diff
        {
            get
            {
                return diff.Value;
            }
        }

        public float UpdateInterval { get { return 1; } }

        public string Acceleration
        {
            get
            {
                // return acceleration.Value;
                return "DriverAssist.Cruise.PredictiveAcceleration";
            }
        }

        public string Deceleration
        {
            get
            {
                // return deceleration.Value;
                return "DriverAssist.Cruise.PredictiveDeceleration";
            }
        }

        public int[] AccelerateKeys
        {
            get
            {
                return BindingFor(faster);
            }
        }

        public int[] DecelerateKeys
        {
            get
            {
                return BindingFor(slower);
            }
        }

        public int[] ToggleKeys
        {
            get
            {
                return BindingFor(toggle);
            }
        }

        public bool ShowStats
        {
            get
            {
                return showStats.Value;
            }
        }

        public int[] BindingFor(ConfigEntry<KeyboardShortcut> shortcut)
        {
            List<int> keys = new List<int>();
            keys.Add((int)shortcut.Value.MainKey);
            foreach (int key in shortcut.Value.Modifiers)
            {
                keys.Add(key);
            }

            return keys.ToArray();
        }
    }
}
