#if UMM

using System.Collections.Generic;
using System.Reflection;
using DriverAssist.Cruise;
using DriverAssist.ECS;
using DriverAssist.Implementation;
using DriverAssist.Localization;
using DV.Logic.Job;
using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;

namespace DriverAssist.UMM
{
    [EnableReloading]
    public static class DriverAssistUmmMod
    {
#pragma warning disable CS8618
        private static Settings settings;
        private static Logger logger;

        private static DriverAssistController controller;
#pragma warning restore CS8618

        public static bool Load(ModEntry modEntry)
        {
            LogFactory.Factory.Value = (scope) =>
            {
                return new UmmLogger(modEntry.Logger, scope);
            };
            logger = LogFactory.GetLogger(typeof(DriverAssistUmmMod));

            logger.Info($"Loading...");

            TranslationManager.SetLangage(LocalizationManager.CurrentLanguage);

            settings = ModSettings.Load<Settings>(modEntry);
            SettingsWrapper config = new(settings);

            controller = new DriverAssistController(config);
            controller.Init();

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.OnUpdate = OnUpdate;
            modEntry.OnFixedUpdate = OnFixedUpdate;
            modEntry.OnToggle = OnToggle;

            logger.Info($"Finished loading");

            return true;
        }

        private static bool OnToggle(ModEntry modEntry, bool value)
        {
            Harmony harmony = new Harmony(modEntry.Info.Id);

            if (value)
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                harmony.UnpatchAll(modEntry.Info.Id);
            }

            return true;
        }

        static void OnUpdate(ModEntry e, float dt)
        {
            controller.OnUpdate();
        }

        static void OnFixedUpdate(ModEntry e, float dt)
        {
            controller.OnFixedUpdate();
        }

        static void OnGUI(ModEntry modEntry)
        {
            settings.Draw(modEntry);
        }

        static void OnSaveGUI(ModEntry modEntry)
        {
            logger.Info($"OnSaveGUI");
            settings.Save(modEntry);
        }

        static bool OnUnload(ModEntry e)
        {
            logger.Info($"OnUnload");
            controller.Unload();
            controller.OnDestroy();

            return true;
        }
    }

    public class Settings : ModSettings, IDrawable
    {
        [Header("Cruise Control")]
        [Draw("Offset", Precision = 1)]
        public float Offset = 0;
        [Draw("Diff", Precision = 1, Min = 0)]
        public float Diff = 2.5f;

        [Header("DE2")]
        [Draw("Minimum Torque", Min = 0)]
        public int De2MinTorque = 22000;
        [Draw("Maximum Amps", Min = 0)]
        public int De2MaxAmps = 750;
        [Draw("Operating Temperature", Min = 0)]
        public int De2MaxTemperature = 105;
        [Draw("Maximum Temperature", Min = 0)]
        public int De2OverdriveTemperature = 118;

        [Header("DE6")]
        [Draw("Minimum Torque", Min = 0)]
        public int De6MinTorque = 50000;
        [Draw("Maximum Amps", Min = 0)]
        public int De6MaxAmps = 435;
        [Draw("Operating Temperature", Min = 0)]
        public int De6MaxTemperature = 105;
        [Draw("Maximum Temperature", Min = 0)]
        public int De6OverdriveTemperature = 118;

        [Header("DH4")]
        [Draw("Minimum Torque", Min = 0)]
        public int Dh4MinTorque = 35000;
        [Draw("Operating Temperature", Min = 0)]
        public int Dh4MaxTemperature = 105;
        [Draw("Maximum Temperature", Min = 0)]
        public int Dh4OverdriveTemperature = 118;

        [Header("DM3")]
        [Draw("Minimum Torque", Min = 0)]
        public int Dm3MinTorque = 35000;
        [Draw("Operating Temperature", Min = 0)]
        public int Dm3MaxTemperature = 105;
        [Draw("Maximum Temperature", Min = 0)]
        public int Dm3OverdriveTemperature = 118;

        [Header("Brakes")]
        [Draw("Braking Time", Min = 0)]
        public int BrakingTime = 10;
        [Draw("Release Factor", Precision = 1, Min = 0)]
        public float BrakeReleaseFactor = 0.5f;
        [Draw("Minimum Brake", Precision = 1, Min = 0)]
        public float MinBrake = 0.1f;

        [Header("Key Bindings")]
        [Draw("Increase cruise control setpoint")]
        public KeyBinding Accelerate = new() { keyCode = KeyCode.PageUp };
        [Draw("Decrease cruise control setpoint")]
        public KeyBinding Decelerate = new() { keyCode = KeyCode.PageDown };
        [Draw("Toggle cruise control")]
        public KeyBinding Toggle = new() { keyCode = KeyCode.RightControl };
        [Draw("Upshift")]
        public KeyBinding Upshift = new() { keyCode = KeyCode.Home };
        [Draw("Downshift")]
        public KeyBinding Downshift = new() { keyCode = KeyCode.End };

        [Header("Developer Settings")]
        [Draw("Show stats")]
        public bool ShowStats = false;
        [Draw("Dump ports")]
        public KeyBinding DumpPorts = new() { keyCode = KeyCode.F9 };

        public override void Save(ModEntry modEntry)
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

        static KeyMatcher GetKeyCode(KeyBinding keyBinding)
        {
            return new UmmKeyMatcher(keyBinding);
        }

        public KeyMatcher AccelerateKeys { get { return GetKeyCode(settings.Accelerate); } }

        public KeyMatcher DecelerateKeys { get { return GetKeyCode(settings.Decelerate); } }

        public KeyMatcher ToggleKeys { get { return GetKeyCode(settings.Toggle); } }

        public KeyMatcher Upshift { get { return GetKeyCode(settings.Upshift); } }

        public KeyMatcher Downshift { get { return GetKeyCode(settings.Downshift); } }

        public KeyMatcher DumpPorts { get { return GetKeyCode(settings.DumpPorts); } }

        public bool ShowStats { get { return settings.ShowStats; } }

        public bool ShowJobs { get { return true; } }

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
                    maxAmps: settings.De2MaxAmps,
                    maxTemperature: settings.De2MaxTemperature,
                    overdriveTemperature: settings.De2OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: settings.MinBrake
                ),
                    [LocoType.DE6] = new UmmLocoSettings(
                    minTorque: settings.De6MinTorque,
                    maxAmps: settings.De6MaxAmps,
                    maxTemperature: settings.De6MaxTemperature,
                    overdriveTemperature: settings.De6OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: settings.MinBrake
                ),
                    [LocoType.DH4] = new UmmLocoSettings(
                    minTorque: settings.Dh4MinTorque,
                    maxAmps: 1000,
                    maxTemperature: settings.Dh4MaxTemperature,
                    overdriveTemperature: settings.Dh4OverdriveTemperature,
                    brakingTime: settings.BrakingTime,
                    brakeReleaseFactor: settings.BrakeReleaseFactor,
                    minBrake: settings.MinBrake
                ),
                    [LocoType.DM3] = new UmmLocoSettings(
                    minTorque: settings.Dm3MinTorque,
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
        private readonly int maxAmps;
        private readonly int maxTemperature;
        private readonly int overdriveTemperature;
        private readonly int brakingTime;
        private readonly float brakeReleaseFactor;
        private readonly float minBrake;

        public UmmLocoSettings(
         int minTorque,
         int maxAmps,
         int maxTemperature,
         int overdriveTemperature,
         int brakingTime,
         float brakeReleaseFactor,
         float minBrake
            )
        {
            this.minTorque = minTorque;
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

        public int MaxAmps
        {
            get
            {
                return maxAmps;
            }
        }

        public int OperatingTemp
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

    class UmmLogger : Logger
    {
        private readonly ModLogger logger;
        private readonly string scope;

        public UmmLogger(ModLogger logger, string scope)
        {
            this.logger = logger;
            this.scope = scope;
        }

        public void Info(string message)
        {
            logger.Log($"{scope,-25} INFO  {message}");
        }

        public void Warn(string message)
        {
            logger.Log($"{scope,-25} WARN  {message}");
        }
    }

    class UmmKeyMatcher : KeyMatcher
    {
        private KeyCode Key { get; }
        private KeyCode[] Modifiers { get; }

        private const byte CTRL = 1;
        private const byte SHIFT = 2;
        private const byte ALT = 4;

        public UmmKeyMatcher(KeyBinding keyBinding)
        {
            Key = keyBinding.keyCode;

            if ((keyBinding.modifiers & CTRL) > 0)
            {
                Modifiers = new KeyCode[] { KeyCode.LeftControl, KeyCode.RightControl };
            }
            else if ((keyBinding.modifiers & SHIFT) > 0)
            {
                Modifiers = new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift };
            }
            else if ((keyBinding.modifiers & ALT) > 0)
            {
                Modifiers = new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt };
            }
            else
            {
                Modifiers = new KeyCode[0];
            }
        }

        public bool IsKeyPressed()
        {
            return IsKeyPressed(Key) && IsModifierPressed();
        }

        private bool IsModifierPressed()
        {
            if (Modifiers.Length == 0) return true;

            foreach (KeyCode key in Modifiers)
            {
                if (IsKeyPressed(key)) return true;
            }

            return false;
        }

        private bool IsKeyPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }
    }

    public static class JobPatches
    {
        [HarmonyPatch(typeof(JobsManager), nameof(JobsManager.RegisterGeneratedJob))]
        public static class JobsManager_RegisterGeneratedJob
        {
#pragma warning disable IDE0060, IDE1006
            public static void Postfix(JobsManager __instance, Job job, HashSet<TrainCar> cars)
#pragma warning restore IDE0060, IDE1006
            {
                // DriverAssistController.Instance?.OnRegisterJob(job);
            }
        }

        [HarmonyPatch(typeof(JobsManager), nameof(JobsManager.UnregisterJob))]
        public static class JobsManager_UnregisterJob
        {
#pragma warning disable IDE0060, IDE1006
            public static void Postfix(JobsManager __instance, Job job)
#pragma warning restore IDE0060, IDE1006
            {
                // DriverAssistController.Instance?.OnUnregisterJob(job);
            }
        }

        [HarmonyPatch(typeof(JobsManager), nameof(JobsManager.TakeJob))]
        public static class JobsManager_TakeJob
        {
#pragma warning disable IDE0060, IDE1006
            public static void Postfix(JobsManager __instance, Job job, bool takenViaLoadGame = false)
#pragma warning restore IDE0060, IDE1006
            {
                DriverAssistController.Instance?.OnRegisterJob(job);
            }
        }

        [HarmonyPatch(typeof(JobsManager), nameof(JobsManager.AbandonJob))]
        public static class JobsManager_AbandonJob
        {
#pragma warning disable IDE0060, IDE1006
            public static void Postfix(JobsManager __instance, Job job)
#pragma warning restore IDE0060, IDE1006
            {
                DriverAssistController.Instance?.OnUnregisterJob(job);
            }
        }

        [HarmonyPatch(typeof(JobsManager), nameof(JobsManager.CompleteTheJob))]
        public static class JobsManager_CompleteTheJob
        {
#pragma warning disable IDE0060, IDE1006
            public static void Postfix(JobsManager __instance, Job job)
#pragma warning restore IDE0060, IDE1006
            {
                DriverAssistController.Instance?.OnUnregisterJob(job);
            }
        }
    }
}

#endif
