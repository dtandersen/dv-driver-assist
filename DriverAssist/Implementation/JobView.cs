using System;
using System.Collections.Generic;
using DriverAssist.ECS;
using UnityEngine;

namespace DriverAssist.Implementation
{
    abstract class BaseWindow : MonoBehaviour
    {
        protected Rect windowRect = new(0, 0, 100, 100);
        protected virtual bool Visible { get; set; }
        protected virtual string Title { get; set; } = "";

        public void OnGUI()
        {
            if (!Visible) return;

            GUI.skin = DVGUI.skin;

            windowRect = GUILayout.Window(293847732, windowRect, GUIWindow, Title);
        }

        private void GUIWindow(int id)
        {
            Window();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }


        protected abstract void Window();
    }

    class JobWindow : BaseWindow, JobView
    {
        public UnifiedSettings? Config { get; internal set; }

        private const float SCALE = 1.5f;
        private readonly Logger logger = LogFactory.GetLogger(typeof(JobWindow));
        private bool photoMode;
        private readonly Dictionary<string, TaskRow> rows = new();

        override protected bool Visible
        {
            get
            {
                if (photoMode) return false;
                if (!Config?.ShowJobs ?? false) return false;

                return true;
            }
        }

        public void Awake()
        {
            logger.Info("Awake");
            Title = "Jobs";
            windowRect = new Rect(Screen.width - 300, 10, SCALE * 120, SCALE * 50);
            photoMode = false;
        }

        override protected void Window()
        {
            int labelwidth = (int)(SCALE * 50);

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
        }

        public void Hide()
        {
            logger.Info("Hide");
        }

        internal void OnPhotoModeChanged(bool photoMode)
        {
            logger.Info($"OnPhotoModeChanged photoMode={photoMode}");
            this.photoMode = photoMode;
        }

        public void OnAddJob(TaskRow row)
        {
            logger.Info($"OnAddJob {row.ID}");
            if (rows.ContainsKey(row.ID))
            {
                rows.Remove(row.ID);
            }
            rows.Add(row.ID, row);
            enabled = true;
        }

        public void OnRemoveJob(string id)
        {
            logger.Info($"OnRemoveJob {id}");
            rows.Remove(id);
            enabled = rows.Count > 0;
        }
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
        void OnAddJob(TaskRow row);
        void OnRemoveJob(string id);
    }
}
