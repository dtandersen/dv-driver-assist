#if UMM

using System.Collections.Generic;
using System.Globalization;
using DriverAssist.Cruise;
using DriverAssist.Implementation;
using DriverAssist.Localization;
using I2.Loc;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;

namespace DriverAssist.UMM
{
    [EnableReloading]
    public static class DriverAssistUmmMod
    {
#pragma warning disable CS8618
        private static Settings settings;

        private static DriverAssistController presenter;
#pragma warning restore CS8618

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            PluginLoggerSingleton.Instance = new UmmLogger(modEntry.Logger);

            PluginLoggerSingleton.Instance.Info($"Detected language {LocalizationManager.CurrentLanguage}");
            TranslationManager.SetLangage(LocalizationManager.CurrentLanguage);

            PluginLoggerSingleton.Instance.Info($"Begin DriverAssistUmmMod::Load");

            settings = Settings.Load<Settings>(modEntry);
            SettingsWrapper config = new(settings);

            presenter = new DriverAssistController(config);
            presenter.Init();

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.OnUpdate = OnUpdate;
            modEntry.OnFixedUpdate = OnFixedUpdate;

            PluginLoggerSingleton.Instance.Info($"End DriverAssistUmmMod::Load");
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
            PluginLoggerSingleton.Instance.Info($"UMM:OnSaveGUI");
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

        public KeyBinding Accelerate = new() { keyCode = KeyCode.PageUp };
        public KeyBinding Decelerate = new() { keyCode = KeyCode.PageDown };
        public KeyBinding Toggle = new() { keyCode = KeyCode.RightControl };
        public KeyBinding Upshift = new() { keyCode = KeyCode.Home };
        public KeyBinding Downshift = new() { keyCode = KeyCode.End };
        public KeyBinding DumpPorts = new() { keyCode = KeyCode.F9 };
        public bool ShowStats = false;

        public int De2MinTorque = 22000;
        public int De2MinAmps = 400;
        public int De2MaxAmps = 750;
        public int De2MaxTemperature = 105;
        public int De2OverdriveTemperature = 118;

        public int De6MinTorque = 50000;
        public int De6MinAmps = 200;
        public int De6MaxAmps = 435;
        public int De6MaxTemperature = 105;
        public int De6OverdriveTemperature = 118;

        public int Dh4MinTorque = 35000;
        public int Dh4MaxTemperature = 105;
        public int Dh4OverdriveTemperature = 119;

        public int Dm3MinTorque = 35000;
        public int Dm3MaxTemperature = 105;
        public int Dm3OverdriveTemperature = 118;

        public int BrakingTime = 10;
        public float BrakeReleaseFactor = 0.5f;
        public float MinBrake = 0.1f;

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
        private readonly Settings settings;

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

        public string Acceleration { get { return typeof(PredictiveAcceleration).AssemblyQualifiedName; } }

        public string Deceleration { get { return typeof(PredictiveDeceleration).AssemblyQualifiedName; } }

        public Dictionary<string, LocoSettings> LocoSettings
        {
            get
            {
                Dictionary<string, LocoSettings> locoSettings = new()
                {
                    [LocoType.DE2] = new UmmLocoSettings(
                    minTorque: settings.De2MinTorque,
                    minAmps: settings.De2MinAmps,
                    maxAmps: settings.De2MaxAmps,
                    maxTemperature: settings.De2MaxTemperature,
                    overdriveTemperature: settings.De2OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: settings.MinBrake
                ),
                    [LocoType.DE6] = new UmmLocoSettings(
                    minTorque: settings.De6MinTorque,
                    minAmps: settings.De6MinAmps,
                    maxAmps: settings.De6MaxAmps,
                    maxTemperature: settings.De6MaxTemperature,
                    overdriveTemperature: settings.De6OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: settings.MinBrake
                ),
                    [LocoType.DH4] = new UmmLocoSettings(
                    minTorque: settings.Dh4MinTorque,
                    minAmps: 0,
                    maxAmps: 1000,
                    maxTemperature: settings.Dh4MaxTemperature,
                    overdriveTemperature: settings.Dh4OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: settings.MinBrake
                ),
                    [LocoType.DM3] = new UmmLocoSettings(
                    minTorque: settings.Dm3MinTorque,
                    minAmps: 0,
                    maxAmps: 1000,
                    maxTemperature: settings.Dm3MaxTemperature,
                    overdriveTemperature: settings.Dm3OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: 0
                )
                };

                return locoSettings;
            }
        }
    }

    internal class UmmLocoSettings : LocoSettings
    {
        private readonly int minTorque;
        private readonly int minAmps;
        private readonly int maxAmps;
        private readonly int maxTemperature;
        private readonly int overdriveTemperature;
        private readonly int brakingTime;
        private readonly float brakeReleaseFactor;
        private readonly float minBrake;

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
        private readonly ModEntry.ModLogger logger;

        public UmmLogger(ModEntry.ModLogger logger)
        {
            Prefix = "";
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
