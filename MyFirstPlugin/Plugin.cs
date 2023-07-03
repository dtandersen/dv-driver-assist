using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CommandTerminal;
using DV.HUD;
using DV.Simulation.Cars;
using DV.UI.LocoHUD;
using DV.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyFirstPlugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class MyPlugin : BaseUnityPlugin
    {
        private CruiseControl cruiseControl;
        static string cmd = "cc";
        // ExampleClass z;

        // private InteriorControlsManager manager;
        // private HUDLocoControls controls;
        public ConfigEntry<KeyboardShortcut> Faster { get; set; }
        public ConfigEntry<KeyboardShortcut> Slower { get; set; }
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin2 {PluginInfo.PLUGIN_GUID} is loaded!");
            // manager = GetComponent<InteriorControlsManager>();
            // SingletonBehaviour<HUDInterfacer>.Instance.HUDChanged += OnHUDChanged;
            cruiseControl = new CruiseControl(Logger);
            RegisterCommands1();
            Debug.Log($"Awake cc={cruiseControl}");
            Debug.Log($"Awake cc={cruiseControl} {this.name}");
            MyPlugin plugin = (MyPlugin)FindObjectOfType(typeof(MyPlugin));
            Debug.Log($"Awake cc={cruiseControl} {this.name} plugin.name={plugin.name}");
            // tag = PluginInfo.PLUGIN_GUID;

            Debug.Log("new ExampleClass()");

            // z.Start();
            // GameObject g = new GameObject();
            // g.AddComponent<Int64>();

            // MakeButton(); 
            ConfigFile configFile = new ConfigFile(PluginInfo.PLUGIN_NAME, true);
            Faster = configFile.Bind("Hotkeys", "Faster", new KeyboardShortcut(KeyCode.PageUp, KeyCode.LeftControl));
            Slower = configFile.Bind("Hotkeys", "Slower", new KeyboardShortcut(KeyCode.PageDown, KeyCode.LeftControl));
        }

        int STEP = 5;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                cruiseControl.DesiredSpeed += STEP;
                Logger.LogInfo($"sp={cruiseControl.DesiredSpeed}");
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                cruiseControl.DesiredSpeed -= STEP;
                Logger.LogInfo($"sp={cruiseControl.DesiredSpeed}");
            }
            // Logger.LogInfo($"Tick sp={cc.sp}");
            cruiseControl.Tick();
        }

        void OnGUI()
        {
            if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
                Event.current.Use();

            GUILayout.BeginArea(new Rect(0, 0, 300, 500));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Set Point");
            GUILayout.TextField($"{cruiseControl.DesiredSpeed}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Throttle");
            GUILayout.TextField($"{cruiseControl.Throttle}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Speed (km/h)");
            GUILayout.TextField($"{cruiseControl.Speed}");
            GUILayout.TextField($"{cruiseControl.DesiredSpeed - cruiseControl.Speed}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Acceleration (m/s^2)");
            GUILayout.TextField($"{cruiseControl.Acceleration}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Force");
            GUILayout.TextField($"{cruiseControl.Force}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Power");
            GUILayout.TextField($"{cruiseControl.Power}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Hoursepower");
            GUILayout.TextField($"{cruiseControl.Hoursepower}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Torque");
            GUILayout.TextField($"{cruiseControl.Torque}");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Mass (t)");
            GUILayout.TextField($"{cruiseControl.Mass / 1000}");
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            // GUILayout.Button("I am not inside an Area");
            // GUILayout.BeginArea(new Rect(Screen.width / 2, Screen.height / 2, 300, 300));
            // GUILayout.Button("I am completely inside an Area");
            // GUILayout.EndArea();
        }

        void Start()
        {
            // Debug.Log("MyPlugin.Start()");
            // GameObject newCanvas = new GameObject("Canvas");

            // Canvas c = newCanvas.AddComponent<Canvas>();
            // c.renderMode = RenderMode.ScreenSpaceOverlay;

            // newCanvas.AddComponent<CanvasScaler>();
            // newCanvas.AddComponent<GraphicRaycaster>();

            // GameObject panel = new GameObject("Panel");
            // panel.AddComponent<CanvasRenderer>();

            // Image i = panel.AddComponent<Image>();
            // i.color = Color.red;

            // panel.transform.SetParent(newCanvas.transform, false);








            // z = new ExampleClass();
            // GameObject obj = new GameObject("[test]");
            // Canvas canvas = obj.AddComponent<Canvas>();
            // canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // CanvasScaler canvasScaler = obj.AddComponent<CanvasScaler>();
            // canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            // canvasScaler.matchWidthOrHeight = 1f;
            // canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            // GameObject obj2 = new GameObject("Label");
            // obj2.transform.SetParent(canvas.transform, worldPositionStays: false);
            // TextMeshProUGUI textMeshProUGUI = obj2.AddComponent<TextMeshProUGUI>();
            // RectTransform rectTransform = textMeshProUGUI.rectTransform;
            // rectTransform.anchorMin = Vector2.zero;
            // rectTransform.anchorMax = Vector2.one;
            // rectTransform.sizeDelta = Vector2.zero;
            // rectTransform.offsetMin = new Vector2(50f, 50f);
            // rectTransform.offsetMax = new Vector2(-50f, -50f);
            // Material material = textMeshProUGUI.material;
            // material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            // material.SetFloat("_UnderlayDilate", 1f);
            // material.SetFloat("_UnderlayOffsetY", -1f);
            // textMeshProUGUI.material = material;
            // textMeshProUGUI.color = new Color(1f, 1f, 0.5f, 1f);
            // textMeshProUGUI.text = "";
            // textMeshProUGUI.enabled = true;            // z.enabled = true;

        }

        // private void CreateButtons()
        // {
        //     presentedButtons = new List<UnityEngine.UI.Button>();

        //     for (int i = 0; i < inventoryToDisplay.Container.ItemLines.Count; i++)
        //     {
        //         presentedButtons.Add(Instantiate(ButtonPrefab, new Vector3(XStartPosition, YStartPosition - (i * YSpaceBetweenRows), 0), Quaternion.identity));
        //         presentedButtons[i].transform.SetParent(GetComponent<RectTransform>(), false);
        //         TMP_Text txt = presentedButtons[i].GetComponentInChildren<TMP_Text>();
        //         txt.text = inventoryToDisplay.Container.ItemLines[i].item.Name + " (" + inventoryToDisplay.Container.ItemLines[i].amount + ")";

        //         presentedButtons[i].onClick.AddListener(() =>
        //         {
        //             _spawner.testButtonPress(inventoryToDisplay.Container.ItemLines[i].item);
        //         });
        //     }
        // }
        // public void CreateButton(Transform panel, Vector3 position, Vector2 size,
        // UnityEngine.Events.UnityAction method)
        // {
        //     GameObject button = new GameObject();
        //     button.transform.parent = panel;
        //     button.AddComponent<RectTransform>();
        //     button.AddComponent<Button>();
        //     button.transform.position = position;
        //     button.GetComponent<RectTransform>().SetSize(size);
        //     button.GetComponent<Button>().onClick.AddListener(method);
        // }

        private void MakeButton()
        {
            // GameObject newButton = DefaultControls.CreateButton(new Resources());

            // GameObject button = new GameObject();
            // button.transform.parent = panel;
            // button.AddComponent<RectTransform>();
            // button.AddComponent<Button>();
            // button.transform.position = position;
            // button.GetComponent<RectTransform>().SetSize(size);
            // button.GetComponent<Button>().onClick.AddListener(method);
        }

        private void RegisterCommands1()
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.FindObjectOfType<Terminal>().StartCoroutine(RegisterCommands());
            }
        }

        private IEnumerator RegisterCommands()
        {
            yield return WaitFor.EndOfFrame;

            // Logger.LogInfo($"commands2={Terminal.Shell.Commands}");
            // Terminal.Shell.Commands.Remove(cmd);
            // if (!Terminal.Shell.Commands.ContainsKey(cmd))
            // {
            CommandInfo command = Terminal.Shell.AddCommand(cmd, Cruise, 1, 1, "");
            // Terminal.Autocomplete.Register(command);
            // }
        }

        public float Speed
        {
            set
            {
                Debug.Log("speed.set");
                cruiseControl.DesiredSpeed = value;
            }
        }

        private static void Cruise(CommandArg[] args)
        {
            // Only already loaded mods show up in PluginInfos, thats why SoftDependency is used above to ensure that.
            // MyPlugin plugin = null;
            BaseUnityPlugin plugin = null;
            foreach (var info in Chainloader.PluginInfos)
            {
                var metadata = info.Value.Metadata;
                if (metadata.GUID.Equals(PluginInfo.PLUGIN_GUID))
                {
                    // found it
                    Debug.Log($"Found {PluginInfo.PLUGIN_GUID}");
                    // plugin = info.Value.Instance;
                    // break;
                }
            }

            foreach (var info in Chainloader.PluginInfos)
            {
                var metadata = info.Value.Metadata;
                if (metadata.GUID.Equals(PluginInfo.PLUGIN_GUID))
                {
                    // found it
                    Debug.Log($"Found2 {PluginInfo.PLUGIN_GUID}");
                    plugin = info.Value.Instance;
                    break;
                }
            }

            // BaseUnityPlugin[] plugins = (BaseUnityPlugin[])FindObjectsOfType(typeof(BaseUnityPlugin));
            // MyPlugin plugin = null;
            // foreach (BaseUnityPlugin p in plugins)
            // {
            //     // plugin = (MyPlugin)plugins[0];
            //     Debug.Log($"Cruise2 {args} plugin={plugin} plugin.name={plugin.name} plugin.tag={plugin.tag}");
            //     plugin = p;
            // }
            // EntityManager.
            // MyPlugin plugin = (MyPlugin)GameObject.Find("MyPlugin");
            // MyPlugin plugin = (MyPlugin)GameObject.FindGameObjectsWithTag(PluginInfo.PLUGIN_GUID)[0];
            // Debug.Log($"Cruise2 {args} plugin={plugin} plugin.name={plugin.name} plugin.tag={plugin.tag}");
            Type type = plugin.GetType().BaseType;

            PropertyInfo prop = type.GetProperty("Speed");

            prop.SetValue(plugin, args[0].Float, null);
            // plugin.cc.sp = args[0].Float;
            // Debug.Log($"sp={plugin.cc.sp}");
        }

        void OnDestroy()
        {
            Terminal.Shell.Commands.Remove(cmd);
            Terminal.Shell.Variables.Remove(cmd);
            Debug.Log($"OnDestroy");
            cruiseControl = null;
            tag = null;
            // z = null;
            // Destroy(z);
            // SingletonBehaviour<HUDInterfacer>.Instance.HUDChanged -= OnHUDChanged;
        }

        // void Update()
        // {
        //     cc.Tick();
        //     // Logger.LogInfo("update");
        //     // return;
        //     // TrainCar locoCar = GetLocomotive();
        //     // float speed = locoCar.GetForwardSpeed();
        //     // Logger.LogInfo($"speed={speed}");
        //     // BaseControlsOverrider obj = locoCar.GetComponent<SimController>()?.controlsOverrider;
        //     // // obj.Brake?.Set(0f);
        //     // // obj.IndependentBrake?.Set(1f);
        //     // // obj.DynamicBrake?.Set(0f);
        //     // // obj.Handbrake?.Set(0f);
        //     // obj.Throttle?.Set(.4f);


        //     // obj.Reverser?.Set(0.5f);
        //     // Logger.LogInfo(obj.Throttle?.Value);
        //     // obj.
        //     // obj.spe

        //     // InteriorControlsManager manager = GetComponent<InteriorControlsManager>();
        //     // HUDLocoControls controls = manager.con
        //     // HUDManager hud = Object.FindObjectOfType<HUDManager>();
        //     // hud.sp
        //     // LocoHUDControlBase speed = controls.basicControls.speedMeter;
        //     // speed.v

        //     // if (manager != null)
        //     // {
        //     //     Logger.LogInfo("manager is NOT null");
        //     //     LocoIndicatorReader indicatorReader = manager.indicatorReader;
        //     //     float speed = indicatorReader.speed.Value;
        //     //     Logger.LogInfo(speed);
        //     // }
        //     // else
        //     // {
        //     //     // Logger.LogInfo("manager is null");
        //     // }
        // }

        private TrainCar GetLocomotive()
        {
            if (!PlayerManager.Car)
            {
                return null;
            }
            if (!PlayerManager.Car.IsLoco)
            {
                return null;
            }
            return PlayerManager.Car;
        }

        private void OnHUDChanged(HUDInterfacer.HUDChangeEvent obj)
        {
            Logger.LogInfo($"OnHUDChanged oldBase={obj.oldBase} oldControls={obj.oldControls} oldManager={obj.oldManager}");
            Logger.LogInfo($"OnHUDChanged newBase={obj.newBase} newControls={obj.newControls} newManager={obj.newManager}");
            if (!obj.newManager)
            {
                Logger.LogInfo("hud removed");
                return;
            }
            Logger.LogInfo("hud changed2");
            // Logger.LogInfo("newManager=" + obj.newManager);
            // manager = obj.newManager;
            // return;
            InteriorControlsManager manager = obj.newManager;
            HUDLocoControls locoControls = obj.newControls;

            LocoIndicatorReader indicatorReader = manager.indicatorReader;
            if (indicatorReader != null)
            {
                if ((bool)locoControls.basicControls.speedMeter)
                {
                    float speed = indicatorReader.speed.Value;
                    Logger.LogInfo(speed);
                }
            }
            // if (manager == obj.oldManager && (bool)obj.oldControls && (bool)obj.oldControls.mechanical.tractionMotorFuse)
            // {
            //     obj.oldControls.mechanical.tractionMotorFuse.controlModule.ValueChanged -= ControlModuleOnValueChanged;
            // }
            // if (manager == obj.newManager && (bool)obj.newControls)
            // {
            //     controls = obj.newControls;
            //     if ((bool)controls.mechanical.tractionMotorFuse)
            //     {
            //         controls.mechanical.tractionMotorFuse.controlModule.ValueChanged += ControlModuleOnValueChanged;
            //     }
            // }
        }
    }


    // public class ExampleClass : MonoBehaviour
    // {
    //     // public GameObject canvas;
    //     // public GameObject Panel;
    //     // public GameObject image;
    //     // int size = 40;

    //     // private float scaler = 0.0125f;

    //     void Start()
    //     {
    //         GameObject obj = new GameObject("[test]");
    //         Canvas canvas = obj.AddComponent<Canvas>();
    //         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    //         CanvasScaler canvasScaler = obj.AddComponent<CanvasScaler>();
    //         canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    //         canvasScaler.matchWidthOrHeight = 1f;
    //         canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
    //         GameObject obj2 = new GameObject("Label");
    //         obj2.transform.SetParent(canvas.transform, worldPositionStays: false);
    //         TextMeshProUGUI textMeshProUGUI = obj2.AddComponent<TextMeshProUGUI>();
    //         RectTransform rectTransform = textMeshProUGUI.rectTransform;
    //         rectTransform.anchorMin = Vector2.zero;
    //         rectTransform.anchorMax = Vector2.one;
    //         rectTransform.sizeDelta = Vector2.zero;
    //         rectTransform.offsetMin = new Vector2(50f, 50f);
    //         rectTransform.offsetMax = new Vector2(-50f, -50f);
    //         Material material = textMeshProUGUI.material;
    //         material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
    //         material.SetFloat("_UnderlayDilate", 1f);
    //         material.SetFloat("_UnderlayOffsetY", -1f);
    //         textMeshProUGUI.material = material;
    //         textMeshProUGUI.color = new Color(1f, 1f, 0.5f, 1f);
    //         textMeshProUGUI.text = "";
    //         textMeshProUGUI.enabled = true;
    //         // return textMeshProUGUI;
    //     }
    // }

}
