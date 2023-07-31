using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DriverAssist.Cruise;
using DriverAssist.ECS;
using DriverAssist.Localization;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class JobWindow : MonoBehaviour, JobView
    {
        public UnifiedSettings? Config { get; internal set; }

        private Rect windowRect;
        private const float SCALE = 1.5f;
        private readonly Logger logger = LogFactory.GetLogger(typeof(JobWindow));
        // private LocoEntity? locoController;
        private bool photoMode;
        private readonly Dictionary<string, TaskRow> rows = new();

        public void Awake()
        {
            photoMode = false;
            logger.Info("Awake");
            windowRect = new Rect(Screen.width - 300, 10, SCALE * 120, SCALE * 50);
            // enabled = true;
        }

        public void OnGUI()
        {
            if (photoMode) return;
            // if (locoController == null) return;
            if (!Config?.ShowJobs ?? false) return;

            // if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
            //     Event.current.Use();

            // if (!locoController.IsLoco) return;

            GUI.skin = DVGUI.skin;

            windowRect = GUILayout.Window(293847732, windowRect, GUIWindow, "Jobs");
        }

        private void GUIWindow(int id)
        {
            Window();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        // bool laststats;
        protected void Window()
        {
            if (Config == null) return;
            // if (CruiseControl == null) return;
            // if (locoController == null) return;

            // Translation localization = TranslationManager.Current;
            // bool x = false;
            // if (GUILayout.Button("X"))
            // {
            //     //Config.ShowStats = false;
            //     Hide();
            // }
            // if (laststats != Config.ShowStats)
            // {
            //     windowRect = new Rect(20, 20, SCALE * 120, SCALE * 50);
            //     laststats = Config.ShowStats;
            // }
            // if (LocoController == null) return;
            // float Speed = loco.RelativeSpeedKmh;
            // float Throttle = loco.Throttle;
            // float mass = LocoController.Mass;
            // float powerkw = Mass * loco.RelativeAccelerationMs * loco.RelativeSpeedKmh / 3.6f / 1000;
            // float Force = Mass * 9.8f / 2f;

            int labelwidth = (int)(SCALE * 50);
            // int width = (int)(SCALE * 50);

            // if (Config.ShowStats)
            // {
            // int mass = (int)(locoController.Mass / 1000);
            // int locoMass = (int)(locoController.LocoMass / 1000);
            // int cargoMass = (int)(locoController.CargoMass / 1000);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Job", GUILayout.Width(labelwidth));
            GUILayout.Label($"Origin", GUILayout.Width(labelwidth));
            GUILayout.Label($"Destination", GUILayout.Width(labelwidth));
            GUILayout.EndHorizontal();

            foreach (TaskRow row in rows.Values)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{row.ID}", GUILayout.Width(labelwidth));
                GUILayout.Label($"{row.Origin}", GUILayout.Width(labelwidth));
                GUILayout.Label($"{row.Destination}", GUILayout.Width(labelwidth));
                GUILayout.EndHorizontal();
            }
        }

        public void OnDestroy()
        {
            logger.Info("OnDestroy");
        }

#pragma warning disable IDE0060
        public void Show(LocoEntity locoEntity)
#pragma warning restore IDE0060
        {
            logger.Info("Show");
            // enabled = true;
        }

        public void Hide()
        {
            logger.Info("Hide");
            // enabled = false;
        }

        internal void OnPhotoModeChanged(bool photoMode)
        {
            logger.Info($"OnPhotoModeChanged photoMode={photoMode}");
            this.photoMode = photoMode;
        }

        public void OnJobAccepted(TaskRow row)
        {
            logger.Info($"OnJobAccepted {row.ID}");
            if (rows.ContainsKey(row.ID))
            {
                rows.Remove(row.ID);
            }
            rows.Add(row.ID, row);
            enabled = true;
        }

        public void OnJobRemoved(string id)
        {
            logger.Info($"OnJobRemoved {id}");
            rows.Remove(id);
            enabled = rows.Count > 0;
        }

        // public void OnRegisterJob(TaskRow row)
        // {
        //     throw new NotImplementedException();
        // }
    }

    public class TaskRow : IEquatable<TaskRow>
    {
        public string ID = "";
        public string Origin = "";
        public string Destination = "";

        public bool Equals(TaskRow other)
        {
            return
                ID == other.ID &&
                Origin == other.Origin &&
                Destination == other.Destination;
        }
    }

    public interface JobView
    {
        void OnJobAccepted(TaskRow row);
        void OnJobRemoved(string id);
    }
}
