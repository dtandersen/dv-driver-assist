#if UMM

using System.Collections.Generic;
using DriverAssist.Cruise;
using DriverAssist.Implementation;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace DriverAssist.UMM
{
    [EnableReloading]
    public static class DriverAssistUmmMod
    {
        public static Settings settings;

        private static DriverAssistController presenter;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            PluginLoggerSingleton.Instance = new UmmLogger(modEntry.Logger);

            settings = Settings.Load<Settings>(modEntry);
            SettingsWrapper config = new SettingsWrapper(settings);

            presenter = new DriverAssistController(config);
            presenter.Init();

            // modEntry.OnFixedGUI = OnFixedGUI;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.OnUpdate = OnUpdate;
            modEntry.OnFixedUpdate = OnFixedUpdate;

            return true;
        }

        static void OnUpdate(ModEntry e, float dt)
        {
            presenter.OnUpdate();
        }

        static void OnFixedUpdate(ModEntry e, float dt)
        {
            presenter.OnFixedUpdate();
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        static bool OnUnload(ModEntry e)
        {
            PluginLoggerSingleton.Instance.Info($"UMM:OnUnload");
            presenter.Unload();
            presenter.OnDestroy();

            return true;
        }
    }

    [DrawFields(DrawFieldMask.Public)]
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        public float Offset = 0;
        public float Diff = 2.5f;

        public KeyBinding Accelerate = new KeyBinding() { keyCode = KeyCode.PageUp };
        public KeyBinding Decelerate = new KeyBinding() { keyCode = KeyCode.PageDown };
        public KeyBinding Toggle = new KeyBinding() { keyCode = KeyCode.RightControl };
        public KeyBinding Upshift = new KeyBinding() { keyCode = KeyCode.Home };
        public KeyBinding Downshift = new KeyBinding() { keyCode = KeyCode.End };
        public KeyBinding DumpPorts = new KeyBinding() { keyCode = KeyCode.F9 };
        public bool ShowStats = false;

        public int de2MinTorque = 22000;
        public int de2MinAmps = 400;
        public int de2MaxAmps = 750;
        public int de2MaxTemperature = 105;
        public int de2OverdriveTemperature = 118;

        public int de6MinTorque = 50000;
        public int de6MinAmps = 200;
        public int de6MaxAmps = 435;
        public int de6MaxTemperature = 105;
        public int de6OverdriveTemperature = 118;

        public int dh4MinTorque = 35000;
        public int dh4MaxTemperature = 105;
        public int dh4OverdriveTemperature = 119;

        public int dm3MinTorque = 35000;
        public int dm3MaxTemperature = 105;
        public int dm3OverdriveTemperature = 118;

        public int brakingTime = 10;
        public float brakeReleaseFactor = 0.5f;
        public float minBrake = 0.1f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
        }
    }

    public class SettingsWrapper : UnifiedSettings
    {
        private Settings settings;

        public SettingsWrapper(Settings settings)
        {
            this.settings = settings;
        }

        public int[] AccelerateKeys { get { return new int[] { (int)settings.Accelerate.keyCode }; } }

        public int[] DecelerateKeys { get { return new int[] { (int)settings.Decelerate.keyCode }; } }

        public int[] ToggleKeys { get { return new int[] { (int)settings.Toggle.keyCode }; } }

        public int[] Upshift { get { return new int[] { (int)settings.Upshift.keyCode }; } }

        public int[] Downshift { get { return new int[] { (int)settings.Downshift.keyCode }; } }

        public int[] DumpPorts { get { return new int[] { (int)settings.DumpPorts.keyCode }; } }

        public bool ShowStats { get { return settings.ShowStats; } }

        public float Offset { get { return settings.Offset; } }

        public float Diff { get { return settings.Diff; } }

        public float UpdateInterval { get { return 1; } }

        public string Acceleration { get { return "DriverAssist.Cruise.PredictiveAcceleration"; } }

        public string Deceleration { get { return "DriverAssist.Cruise.PredictiveDeceleration"; } }

        public Dictionary<string, LocoSettings> LocoSettings
        {
            get
            {
                Dictionary<string, LocoSettings> locoSettings = new Dictionary<string, LocoSettings>();
                locoSettings[LocoType.DE2] = new UmmLocoSettings(
                    minTorque: settings.de2MinTorque,
                    minAmps: settings.de2MinAmps,
                    maxAmps: settings.de2MaxAmps,
                    maxTemperature: settings.de2MaxTemperature,
                    overdriveTemperature: settings.de2OverdriveTemperature,
                    brakingTime: settings.brakingTime,
                    brakeReleaseFactor: settings.brakeReleaseFactor,
                    minBrake: settings.minBrake
                );
                locoSettings[LocoType.DE6] = new UmmLocoSettings(
                    minTorque: settings.de6MinTorque,
                    minAmps: settings.de6MinAmps,
                    maxAmps: settings.de6MaxAmps,
                    maxTemperature: settings.de6MaxTemperature,
                    overdriveTemperature: settings.de6OverdriveTemperature,
                    brakingTime: settings.brakingTime,
                    brakeReleaseFactor: settings.brakeReleaseFactor,
                    minBrake: settings.minBrake
                );
                locoSettings[LocoType.DH4] = new UmmLocoSettings(
                    minTorque: settings.dh4MinTorque,
                    minAmps: 0,
                    maxAmps: 1000,
                    maxTemperature: settings.dh4MaxTemperature,
                    overdriveTemperature: settings.dh4OverdriveTemperature,
                    brakingTime: settings.brakingTime,
                    brakeReleaseFactor: settings.brakeReleaseFactor,
                    minBrake: settings.minBrake
                );
                locoSettings[LocoType.DM3] = new UmmLocoSettings(
                    minTorque: settings.dm3MinTorque,
                    minAmps: 0,
                    maxAmps: 1000,
                    maxTemperature: settings.dm3MaxTemperature,
                    overdriveTemperature: settings.dm3OverdriveTemperature,
                    brakingTime: settings.brakingTime,
                    brakeReleaseFactor: settings.brakeReleaseFactor,
                    minBrake: settings.minBrake
                );

                return locoSettings;
            }
        }
    }

    internal class UmmLocoSettings : LocoSettings
    {
        private int minTorque;
        private int minAmps;
        private int maxAmps;
        private int maxTemperature;
        private int overdriveTemperature;
        private int brakingTime;
        private float brakeReleaseFactor;
        private float minBrake;

        public UmmLocoSettings(
         int minTorque,
         int minAmps,
         int maxAmps,
         int maxTemperature,
         int overdriveTemperature,
         int brakingTime,
         float brakeReleaseFactor,
         float minBrake
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
                return minTorque;
            }
        }

        public int MinAmps
        {
            get
            {
                return minAmps;
            }
        }

        public int MaxAmps
        {
            get
            {
                return maxAmps;
            }
        }

        public int MaxTemperature
        {
            get
            {
                return maxTemperature;
            }
        }

        public int HillClimbTemp
        {
            get
            {
                return overdriveTemperature;
            }
        }

        public int BrakingTime
        {
            get
            {
                return brakingTime;
            }
        }

        public float BrakeReleaseFactor
        {
            get
            {
                return brakeReleaseFactor;
            }
        }

        public float MinBrake
        {
            get
            {
                return minBrake;
            }
        }

        public float HillClimbAccel { get { return 0.025f; } }

        public float CruiseAccel { get { return 0.05f; } }

        public float MaxAccel { get { return 0.25f; } }
    }

    class UmmLogger : PluginLogger
    {
        private ModEntry.ModLogger logger;

        public UmmLogger(ModEntry.ModLogger logger)
        {
            this.logger = logger;
        }

        public string Prefix { get; set; }

        public void Info(string message)
        {
            logger.Log($"{Prefix}{message}");
        }
    }
}

#endif