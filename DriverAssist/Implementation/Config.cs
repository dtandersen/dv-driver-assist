using System.Collections.Generic;
using BepInEx.Configuration;
using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class BepInExDriverAssistSettings : DriverAssistSettings, CruiseControlSettings
    {
        private ConfigEntry<float> offset;
        private ConfigEntry<float> diff;

        private ConfigEntry<int> de2MinTorque;
        private ConfigEntry<int> de2MinAmps;
        private ConfigEntry<int> de2MaxAmps;
        private ConfigEntry<int> de2MaxTemperature;
        private ConfigEntry<int> de2OverdriveTemperature;
        // private ConfigEntry<bool> de2OverdriveEnabled;

        private ConfigEntry<int> de6MinTorque;
        private ConfigEntry<int> de6MinAmps;
        private ConfigEntry<int> de6MaxAmps;
        private ConfigEntry<int> de6MaxTemperature;
        private ConfigEntry<int> de6OverdriveTemperature;
        // private ConfigEntry<bool> de6OverdriveEnabled;

        private ConfigEntry<int> dh4MinTorque;
        private ConfigEntry<int> dh4MaxTemperature;
        private ConfigEntry<int> dh4OverdriveTemperature;
        // private ConfigEntry<bool> dh4OverdriveEnabled;

        private ConfigEntry<int> brakingTime;

        // private ConfigEntry<string> acceleration;
        // private ConfigEntry<string> deceleration;
        private ConfigEntry<KeyboardShortcut> faster;
        private ConfigEntry<KeyboardShortcut> slower;
        private ConfigEntry<KeyboardShortcut> toggle;
        private ConfigEntry<bool> showStats;
        // private Dictionary<string, LocoSettings> locoSettings;

        public BepInExDriverAssistSettings(ConfigFile config)
        {
            diff = config.Bind("Speed", "Diff", 2.5f, "speed = setpoint +/- diff");
            offset = config.Bind("Speed", "Offset", 0f, "This amount is subtracted from setpoint before adding the diff");

            de2MinTorque = config.Bind("DE2", "MinTorque", 20000, "Upshift torque threshold");
            de2MinAmps = config.Bind("DE2", "MinAmps", 400, "Upshift amperage threshold");
            de2MaxAmps = config.Bind("DE2", "MaxAmps", (int)(1000f * 0.725f), "Downshift amperage threshold");
            de2MaxTemperature = config.Bind("DE2", "MaxTemperature", 104, "Downshift temperature threshhold");
            de2OverdriveTemperature = config.Bind("DE2", "OverdriveTemperature", 118, "Downshift temperature threshhold during overdrive");
            // de2OverdriveEnabled = config.Bind("DE2", "Overdrive", true, "Enable overdrive when train is slowing down");

            de6MinTorque = config.Bind("DE6", "MinTorque", 50000, "Upshift torque threshold");
            de6MinAmps = config.Bind("DE6", "MinAmps", 200, "Upshift amperage threshold");
            de6MaxAmps = config.Bind("DE6", "MaxAmps", (int)(600f * 0.725f), "Downshift amperage threshold");
            de6MaxTemperature = config.Bind("DE6", "MaxTemperature", 104, "Downshift temperature threshhold");
            de6OverdriveTemperature = config.Bind("DE6", "OverdriveTemperature", 118, "Downshift temperature threshhold during overdrive");
            // de6OverdriveEnabled = config.Bind("DE6", "Overdrive", true, "Enable overdrive when train is slowing down");

            dh4MinTorque = config.Bind("DH4", "MinTorque", 50000, "Upshift torque threshold");
            dh4MaxTemperature = config.Bind("DH4", "MaxTemperature", 104, "Downshift temperature threshhold");
            dh4OverdriveTemperature = config.Bind("DH4", "OverdriveTemperature", 118, "Downshift temperature threshhold during overdrive");
            // dh4OverdriveEnabled = config.Bind("DH4", "Overdrive", true, "Enable overdrive when train is slowing down");

            brakingTime = config.Bind("Braking", "Time", 10, "Time to decelerate");

            // acceleration = config.Bind("CruiseControl", "Acceleration", "DriverAssist.Cruise.PredictiveAcceleration", "Maximum torque");
            // deceleration = config.Bind("CruiseControl", "Deceleration", "DriverAssist.Cruise.PredictiveDeceleration", "Maximum torque");

            faster = config.Bind("Hotkeys", "Faster", new KeyboardShortcut(KeyCode.PageUp));
            slower = config.Bind("Hotkeys", "Slower", new KeyboardShortcut(KeyCode.PageDown));
            toggle = config.Bind("Hotkeys", "Toggle", new KeyboardShortcut(KeyCode.RightControl));

            showStats = config.Bind("UI", "ShowStats", true, "Show stats");

            LocoSettings = new Dictionary<string, LocoSettings>();
            LocoSettings[LocoType.DE2] = new BepInExLocoSettings(
                minTorque: de2MinTorque,
                minAmps: de2MinAmps,
                maxAmps: de2MaxAmps,
                maxTemperature: de2MaxTemperature,
                overdriveTemperature: de2OverdriveTemperature,
                overdriveEnabled: null,
                brakingTime: brakingTime
            );
            LocoSettings[LocoType.DE6] = new BepInExLocoSettings(
                minTorque: de6MinTorque,
                minAmps: de6MinAmps,
                maxAmps: de6MaxAmps,
                maxTemperature: de6MaxTemperature,
                overdriveTemperature: de6OverdriveTemperature,
                overdriveEnabled: null,
                brakingTime: brakingTime
            );
            LocoSettings[LocoType.DH4] = new BepInExLocoSettings(
                minTorque: dh4MinTorque,
                minAmps: null,
                maxAmps: null,
                maxTemperature: dh4MaxTemperature,
                overdriveTemperature: dh4OverdriveTemperature,
                overdriveEnabled: null,
                brakingTime: brakingTime
            );
        }

        public Dictionary<string, LocoSettings> LocoSettings { get; }

        // public int MinTorque
        // {
        //     get
        //     {
        //         return de2MinTorque.Value;
        //     }
        // }

        // public int MinAmps
        // {
        //     get
        //     {
        //         return de2MinAmps.Value;
        //     }
        // }

        // public int MaxAmps
        // {
        //     get
        //     {
        //         return de2MaxAmps.Value;
        //     }
        // }

        // public int MaxTemperature
        // {
        //     get
        //     {
        //         return de2MaxTemperature.Value;
        //     }
        // }

        // public int OverdriveTemperature
        // {
        //     get
        //     {
        //         return de2OverdriveTemperature.Value;
        //     }
        // }

        // public bool OverdriveEnabled
        // {
        //     get
        //     {
        //         return de2OverdriveEnabled?.Value??true;
        //     }
        // }

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

    internal class BepInExLocoSettings : LocoSettings
    {
        private ConfigEntry<int> minTorque;
        private ConfigEntry<int> minAmps;
        private ConfigEntry<int> maxAmps;
        private ConfigEntry<int> maxTemperature;
        private ConfigEntry<int> overdriveTemperature;
        private ConfigEntry<bool> overdriveEnabled;
        private ConfigEntry<int> brakingTime;

        public BepInExLocoSettings(
            ConfigEntry<int> minTorque,
            ConfigEntry<int> minAmps,
            ConfigEntry<int> maxAmps,
            ConfigEntry<int> maxTemperature,
            ConfigEntry<int> overdriveTemperature,
            ConfigEntry<bool> overdriveEnabled,
            ConfigEntry<int> brakingTime
            )
        {
            this.minTorque = minTorque;
            this.minAmps = minAmps;
            this.maxAmps = maxAmps;
            this.maxTemperature = maxTemperature;
            this.overdriveTemperature = overdriveTemperature;
            this.overdriveEnabled = overdriveEnabled;
            this.brakingTime = brakingTime;
        }

        public int MinTorque
        {
            get
            {
                return minTorque?.Value ?? 0;
            }
        }

        public int MinAmps
        {
            get
            {
                return minAmps?.Value ?? 0;
            }
        }

        public int MaxAmps
        {
            get
            {
                return maxAmps?.Value ?? 0;
            }
        }

        public int MaxTemperature
        {
            get
            {
                return maxTemperature?.Value ?? 0;
            }
        }

        public int OverdriveTemperature
        {
            get
            {
                return overdriveTemperature?.Value ?? 0;
            }
        }

        public bool OverdriveEnabled
        {
            get
            {
                return overdriveEnabled?.Value ?? false;
            }
        }

        public int BrakingTime
        {
            get
            {
                return brakingTime?.Value ?? 0;
            }
        }
    }
}
