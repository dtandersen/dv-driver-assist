#if BEPINEX

using System.Collections.Generic;
using BepInEx.Configuration;
using DriverAssist.Cruise;
using UnityEngine;

namespace DriverAssist.Implementation
{
    public class BepInExDriverAssistSettings : UnifiedSettings
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

        private ConfigEntry<int> dm3MinTorque;
        private ConfigEntry<int> dm3MaxTemperature;
        private ConfigEntry<int> dm3OverdriveTemperature;

        private ConfigEntry<int> brakingTime;
        private ConfigEntry<float> brakeReleaseFactor;
        private ConfigEntry<float> minBrake;

        // private ConfigEntry<string> acceleration;
        // private ConfigEntry<string> deceleration;
        private ConfigEntry<KeyboardShortcut> faster;
        private ConfigEntry<KeyboardShortcut> slower;
        private ConfigEntry<KeyboardShortcut> toggle;
        private ConfigEntry<KeyboardShortcut> upshift;
        private ConfigEntry<KeyboardShortcut> downshift;
        private ConfigEntry<KeyboardShortcut> dumpPorts;
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

            dm3MinTorque = config.Bind("DM3", "MinTorque", 35000, "Upshift torque threshold");
            dm3MaxTemperature = config.Bind("DM3", "MaxTemperature", 104, "Downshift temperature threshhold");
            dm3OverdriveTemperature = config.Bind("DM3", "OverdriveTemperature", 118, "Downshift temperature threshhold during overdrive");

            brakingTime = config.Bind("Braking", "DecelerationTime", 10, "Time to decelerate");
            brakeReleaseFactor = config.Bind("Braking", "BrakeReleaseFactor", .5f, "Brake = Brake - BrakeReleaseFactor * Brake");
            minBrake = config.Bind("Braking", "MinBrake", .1f, "Minimum braking (0=0%, 1=100%)");

            faster = config.Bind("Hotkeys", "Faster", new KeyboardShortcut(KeyCode.PageUp));
            slower = config.Bind("Hotkeys", "Slower", new KeyboardShortcut(KeyCode.PageDown));
            toggle = config.Bind("Hotkeys", "Toggle", new KeyboardShortcut(KeyCode.RightControl));
            upshift = config.Bind("Hotkeys", "Upshift", new KeyboardShortcut(KeyCode.Home));
            downshift = config.Bind("Hotkeys", "Downshift", new KeyboardShortcut(KeyCode.End));
            dumpPorts = config.Bind("Hotkeys", "DumpPorts", new KeyboardShortcut(KeyCode.F9));

            showStats = config.Bind("UI", "ShowStats", false, "Show stats");

            LocoSettings = new Dictionary<string, LocoSettings>();
            LocoSettings[LocoType.DE2] = new BepInExLocoSettings(
                minTorque: de2MinTorque,
                minAmps: de2MinAmps,
                maxAmps: de2MaxAmps,
                maxTemperature: de2MaxTemperature,
                overdriveTemperature: de2OverdriveTemperature,
                brakingTime: brakingTime,
                brakeReleaseFactor: brakeReleaseFactor,
                minBrake: minBrake
            );
            LocoSettings[LocoType.DE6] = new BepInExLocoSettings(
                minTorque: de6MinTorque,
                minAmps: de6MinAmps,
                maxAmps: de6MaxAmps,
                maxTemperature: de6MaxTemperature,
                overdriveTemperature: de6OverdriveTemperature,
                brakingTime: brakingTime,
                brakeReleaseFactor: brakeReleaseFactor,
                minBrake: minBrake
            );
            LocoSettings[LocoType.DH4] = new BepInExLocoSettings(
                minTorque: dh4MinTorque,
                minAmps: null,
                maxAmps: null,
                maxTemperature: dh4MaxTemperature,
                overdriveTemperature: dh4OverdriveTemperature,
                brakingTime: brakingTime,
                brakeReleaseFactor: brakeReleaseFactor,
                minBrake: minBrake
            );
            LocoSettings[LocoType.DM3] = new BepInExLocoSettings(
                minTorque: dm3MinTorque,
                minAmps: null,
                maxAmps: null,
                maxTemperature: dm3MaxTemperature,
                overdriveTemperature: dm3OverdriveTemperature,
                brakingTime: brakingTime,
                brakeReleaseFactor: brakeReleaseFactor,
                minBrake: minBrake
            );
        }

        public Dictionary<string, LocoSettings> LocoSettings { get; }

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
                return typeof(PredictiveAcceleration).AssemblyQualifiedName;
            }
        }

        public string Deceleration
        {
            get
            {
                return typeof(PredictiveDeceleration).AssemblyQualifiedName;
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

        public int[] Upshift
        {
            get
            {
                return BindingFor(upshift);
            }
        }

        public int[] Downshift
        {
            get
            {
                return BindingFor(downshift);
            }
        }

        public int[] DumpPorts
        {
            get
            {
                return BindingFor(dumpPorts);
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
        private ConfigEntry<int> brakingTime;
        private ConfigEntry<float> brakeReleaseFactor;
        private ConfigEntry<float> minBrake;

        public BepInExLocoSettings(
            ConfigEntry<int> minTorque,
            ConfigEntry<int> minAmps,
            ConfigEntry<int> maxAmps,
            ConfigEntry<int> maxTemperature,
            ConfigEntry<int> overdriveTemperature,
            ConfigEntry<int> brakingTime,
            ConfigEntry<float> brakeReleaseFactor,
            ConfigEntry<float> minBrake
            )
        {
            this.minTorque = minTorque;
            this.minAmps = minAmps;
            this.maxAmps = maxAmps;
            this.maxTemperature = maxTemperature;
            this.overdriveTemperature = overdriveTemperature;
            this.brakingTime = brakingTime;
            this.brakeReleaseFactor = brakeReleaseFactor;
            this.minBrake = minBrake;
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

        public int HillClimbTemp
        {
            get
            {
                return overdriveTemperature?.Value ?? 0;
            }
        }

        public int BrakingTime
        {
            get
            {
                return brakingTime?.Value ?? 0;
            }
        }

        public float BrakeReleaseFactor
        {
            get
            {
                return brakeReleaseFactor?.Value ?? 0;
            }
        }

        public float MinBrake
        {
            get
            {
                return minBrake?.Value ?? 0;
            }
        }

        public float HillClimbAccel { get { return 0.025f; } }

        public float CruiseAccel { get { return 0.05f; } }

        public float MaxAccel { get { return 0.25f; } }
    }
}

#endif