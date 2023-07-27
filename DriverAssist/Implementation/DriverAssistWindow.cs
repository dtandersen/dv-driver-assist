using System;
using DriverAssist.Cruise;
using DriverAssist.Localization;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class DriverAssistWindow : MonoBehaviour
    {
        public CruiseControl? CruiseControl { get; internal set; }
        public UnifiedSettings? Config { get; internal set; }

        private Rect windowRect;
        private const float SCALE = 1.5f;
        private readonly Logger logger = LogFactory.GetLogger("DriverAssistWindow");
        private LocoController? locoController;

        public void Awake()
        {
            logger.Info("Awake");
            windowRect = new Rect(20, 20, SCALE * 120, SCALE * 50);
            enabled = false;
        }

        public void OnGUI()
        {
            if (locoController == null) return;

            if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
                Event.current.Use();

            if (!locoController.IsLoco) return;

            GUI.skin = DVGUI.skin;

            windowRect = GUILayout.Window(4444324, windowRect, GUIWindow, "stats");
        }

        private void GUIWindow(int id)
        {
            Window();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        bool laststats;
        protected void Window()
        {
            if (Config == null) return;
            if (CruiseControl == null) return;
            if (locoController == null) return;

            Translation localization = TranslationManager.Current;

            if (laststats != Config.ShowStats)
            {
                windowRect = new Rect(20, 20, SCALE * 120, SCALE * 50);
                laststats = Config.ShowStats;
            }
            // if (LocoController == null) return;
            // float Speed = loco.RelativeSpeedKmh;
            // float Throttle = loco.Throttle;
            // float mass = LocoController.Mass;
            // float powerkw = Mass * loco.RelativeAccelerationMs * loco.RelativeSpeedKmh / 3.6f / 1000;
            // float Force = Mass * 9.8f / 2f;

            int labelwidth = (int)(SCALE * 100);
            int width = (int)(SCALE * 50);

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_SETPOINT, GUILayout.Width(labelwidth));
            GUILayout.TextField($"{CruiseControl.DesiredSpeed}", GUILayout.Width(width));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_STATUS, GUILayout.Width(labelwidth));
            GUILayout.TextField($"{CruiseControl.Status}", GUILayout.Width(SCALE * 150));
            GUILayout.EndHorizontal();

            if (Config.ShowStats)
            {
                int mass = (int)(locoController.Mass / 1000);
                int locoMass = (int)(locoController.LocoMass / 1000);
                int cargoMass = (int)(locoController.CargoMass / 1000);

                GUILayout.BeginHorizontal();
                GUILayout.Label($"", GUILayout.Width(labelwidth));
                GUILayout.Label(localization.TRAIN, GUILayout.Width(width));
                GUILayout.Label(localization.LOCO_ABBV, GUILayout.Width(width));
                GUILayout.Label(localization.CARGO, GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{localization.STAT_MASS}", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{mass}", GUILayout.Width(width));
                GUILayout.TextField($"{locoMass}", GUILayout.Width(width));
                GUILayout.TextField($"{cargoMass}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                float predTime = 5f;

                GUILayout.BeginHorizontal();
                GUILayout.Label($"", GUILayout.Width(labelwidth));
                GUILayout.Label(localization.STAT_CURRENT, GUILayout.Width(width));
                GUILayout.Label($"{localization.STAT_CHANGE}/s", GUILayout.Width(width));
                GUILayout.Label($"{(int)predTime}s", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_SPEED, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.RelativeSpeedKmh:N1}", GUILayout.Width(width));
                GUILayout.TextField($"{locoController.RelativeAccelerationMs:N3}", GUILayout.Width(width));
                GUILayout.TextField($"{locoController.RelativeSpeedKmh + predTime * locoController.RelativeAccelerationMs * 3.6f:N1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_TEMPERATURE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.Temperature:N1}", GUILayout.Width(width));
                GUILayout.TextField($"{locoController.TemperatureChange:N2}", GUILayout.Width(width));
                GUILayout.TextField($"{locoController.Temperature + predTime * locoController.TemperatureChange:N1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_AMPS, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.Amps:N0}", GUILayout.Width(width));
                GUILayout.TextField($"{locoController.AmpsRoc:N1}", GUILayout.Width(width));
                GUILayout.TextField($"{locoController.Amps + predTime * locoController.AmpsRoc:N0}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_RPM, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{Math.Round(locoController.Rpm, 0)}", GUILayout.Width(width));
                GUILayout.TextField($"", GUILayout.Width(width));
                GUILayout.TextField($"", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_TORQUE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{(int)locoController.RelativeTorque}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(localization.STAT_THROTTLE, GUILayout.Width(labelwidth));
                GUILayout.TextField($"{(int)(locoController.Throttle * 100)}%", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gear", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.Gear + 1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                // if (loco.Components.GearChangeRequest.HasValue)
                // {
                GearChangeRequest? req = locoController.Components?.GearChangeRequest;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Requested Gear", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{req?.RequestedGear ?? -1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Restore Throttle", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{req?.RestoreThrottle ?? -1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();
                // }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Gear Ratio", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.GearRatio}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Shifting", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.GearShiftInProgress}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Train Brake", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.TrainBrake:F2}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Acceleration", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{locoController.Components?.LocoStats.AccelerationMs2:F3}", GUILayout.Width(width));
                GUILayout.EndHorizontal();

                float speed2 = 3f / 25f * (float)Math.PI * locoController.WheelRadius * locoController.Rpm / locoController.GearRatio;
                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed", GUILayout.Width(labelwidth));
                GUILayout.TextField($"{speed2:N1}", GUILayout.Width(width));
                GUILayout.EndHorizontal();
            }
        }

        public void OnDestroy()
        {
            logger.Info("OnDestroy");
        }

        public void Show(LocoController loco)
        {
            logger.Info("Show");
            this.locoController = loco;
            enabled = true;
        }

        public void Hide()
        {
            logger.Info("Hide");
            this.locoController = null;
            enabled = false;
        }
    }
}
