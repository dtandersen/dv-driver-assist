using MyFirstPlugin;

namespace CruiseControlPlugin
{
    public interface CruiseControlConfig
    {
        int MaxTorque
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
    }
}
