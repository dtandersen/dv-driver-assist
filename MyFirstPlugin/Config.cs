using MyFirstPlugin;

namespace CruiseControlPlugin
{
    public interface CruiseControlConfig
    {
        int MaxTorque
        {
            get;
        }

        float Offset
        {
            get;
        }

        float Diff
        {
            get;
        }
    }

    class BepinexCruiseControlConfig : CruiseControlConfig
    {
        private readonly MyPlugin plugin;

        public BepinexCruiseControlConfig(MyPlugin plugin)
        {
            this.plugin = plugin;
        }

        public int MaxTorque
        {
            get
            {
                if (!int.TryParse(plugin.MaxTorque.Value, out int result))
                {
                    return 0;
                }

                return result;
            }
        }

        public float Offset
        {
            get
            {
                if (!float.TryParse(plugin.Offset.Value, out float result))
                {
                    return 0;
                }

                return result;
            }
        }

        public float Diff
        {
            get
            {
                if (!float.TryParse(plugin.Diff.Value, out float result))
                {
                    return 0;
                }

                return result;
            }
        }
    }
}
