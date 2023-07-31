using System;
using DriverAssist.Cruise;
using DriverAssist.ECS;
using DriverAssist.Localization;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class CruiseControlWindow : MonoBehaviour
    {
        public CruiseControl? CruiseControl { get; internal set; }
        public UnifiedSettings? Config { get; internal set; }

        private Rect windowRect;
        private const float SCALE = 1.5f;
        private readonly Logger logger = LogFactory.GetLogger(typeof(CruiseControlWindow));
        private LocoEntity? locoEntity;
        private bool photoMode;

        public void Awake()
        {
            photoMode = false;
            logger.Info("Awake");
            windowRect = new Rect(10, 10, SCALE * 120, SCALE * 50);
            enabled = false;
        }

        public void OnGUI()
        {
            if (photoMode) return;
            if (locoEntity == null) return;

            if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
                Event.current.Use();

            if (!locoEntity.IsLoco) return;

            GUI.skin = DVGUI.skin;

            windowRect = GUILayout.Window(2445234, windowRect, GUIWindow, "Cruise Control");
        }

        private void GUIWindow(int id)
        {
            Window();
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        protected void Window()
        {
            if (Config == null) return;
            if (CruiseControl == null) return;
            if (locoEntity == null) return;

            Translation localization = TranslationManager.Current;

            GUIStyle header = new GUIStyle(DVGUI.skin.label)
            {
                fontSize = (int)(SCALE * 10) / 2 * 2,
                fontStyle = FontStyle.Normal
            };
            GUIStyle centered = new GUIStyle(DVGUI.skin.label)
            {
                fontSize = (int)(SCALE * 10) / 2 * 2,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
                // normal.background = 1
            };
            GUIStyle left = new GUIStyle(DVGUI.skin.label)
            {
                fontSize = (int)(SCALE * 10) / 2 * 2,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft
                // normal.background = 1
            };
            var col1 = SCALE * 50;
            var col2 = SCALE * 150;
            // GUI.skin.font.fontSize = SCALE * 12;

            GUILayout.BeginHorizontal();
            GUILayout.Label(localization.CC_SETPOINT, centered, GUILayout.Width(col1));
            GUILayout.Label(localization.CC_STATUS, header, GUILayout.Width(col2));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{CruiseControl.DesiredSpeed}", centered, GUILayout.Width(col1));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{CruiseControl.Status}", left, GUILayout.Width(col2));
            GUILayout.EndHorizontal();
        }

        // void Row(string label, string bal)

        public void OnDestroy()
        {
            logger.Info("OnDestroy");
        }

        public void Show(LocoEntity locoEntity)
        {
            logger.Info("Show");
            this.locoEntity = locoEntity;
            enabled = true;
        }

        public void Hide()
        {
            logger.Info("Hide");
            this.locoEntity = null;
            enabled = false;
        }

        internal void OnPhotoModeChanged(bool photoMode)
        {
            logger.Info($"OnPhotoModeChanged photoMode={photoMode}");
            this.photoMode = photoMode;
        }
    }
}
