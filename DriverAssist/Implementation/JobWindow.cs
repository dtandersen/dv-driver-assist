using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DriverAssist.ECS;
using DriverAssist.Localization;
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
        private readonly Dictionary<string, JobRow> rows = new();

        override protected bool Visible
        {
            get
            {
                if (photoMode) return false;
                if (!Config?.ShowJobs ?? false) return false;
                if (rows.Count == 0) return false;

                return true;
            }
        }

        public void Awake()
        {
            Translation localization = TranslationManager.Current;

            logger.Info("Awake");
            Title = localization.JOB_TITLE;
            windowRect = new Rect(Screen.width - 300, 10, SCALE * 120, SCALE * 50);
            photoMode = false;
        }

        override protected void Window()
        {
            Translation localization = TranslationManager.Current;

            int jobWidth = (int)(SCALE * 60);
            int stationWidth = (int)(SCALE * 50);


            // white = new GUIStyle(EditorStyles.label);
            // white.normal.textColor = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.JOB_ID, GUILayout.Width(jobWidth));
            GUILayout.Label(localization.JOB_ORIGIN, GUILayout.Width(stationWidth));
            GUILayout.Label(localization.JOB_DESTINATION, GUILayout.Width(stationWidth));
            GUILayout.EndHorizontal();

            foreach (JobRow job in rows.Values)
            {
                foreach (TaskRow task in job.Tasks)
                {
                    GUIStyle boxStyle = new GUIStyle(GUI.skin.label);
                    if (task.Complete) boxStyle.normal.textColor = Color.green;
                    else boxStyle.normal.textColor = Color.white;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{job.ID}", boxStyle, GUILayout.Width(jobWidth));
                    GUILayout.Label($"{task.Origin}", boxStyle, GUILayout.Width(stationWidth));
                    GUILayout.Label($"{task.Destination}", boxStyle, GUILayout.Width(stationWidth));
                    GUILayout.EndHorizontal();
                }
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

        public void OnAddJob(JobRow row)
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

    public class JobRow : IEquatable<JobRow>
    {
        public string ID = "";
        public List<TaskRow> Tasks = new();
        // public string Origin = "";
        // public string Destination = "";

        public bool Equals(JobRow other)
        {
            return
                ID == other.ID &&
                Tasks.SequenceEqual(other.Tasks);
        }

        public override string ToString()
        {
            return $"JobRow[ID={ID}, Tasks=[{string.Join(",", Tasks)}]";
        }
    }

    public class TaskRow : IEquatable<TaskRow>
    {
        public string Origin = "";
        public string Destination = "";
        public bool Complete = false;

        public bool Equals(TaskRow other)
        {
            return
                Origin == other.Origin &&
                Destination == other.Destination &&
                Complete == other.Complete;
        }

        public override string ToString()
        {
            return $"TaskRow[Origin={Origin}, Destination={Destination}, Complete={Complete}]";
        }
    }

    public interface JobView
    {
        void OnAddJob(JobRow row);
        void OnRemoveJob(string id);
    }
}
