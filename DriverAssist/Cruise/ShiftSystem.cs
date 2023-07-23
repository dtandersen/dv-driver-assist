namespace DriverAssist.Cruise
{
    public class ShiftSystem : BaseSystem
    {
        private readonly LocoController loco;

        public ShiftSystem(LocoController loco)
        {
            this.loco = loco;
        }

        public override void OnUpdate()
        {
            if (!loco.Components.GearChangeRequest.HasValue) return;

            GearChangeRequest request = loco.Components.GearChangeRequest.Value;

            int requestedGear = request.RequestedGear;

            if (loco.Throttle > 0 && !loco.GearShiftInProgress && loco.Gear != requestedGear)
            {
                PluginLoggerSingleton.Instance.Info($"ShiftSystem: Throttle=0");
                loco.Throttle = 0;
            }
            else if (loco.Gear != requestedGear)
            {
                PluginLoggerSingleton.Instance.Info($"ShiftSystem: Setting gear to {requestedGear}");
                loco.Gear = requestedGear;
                if (!request.RestoreThrottle.HasValue)
                {
                    PluginLoggerSingleton.Instance.Info($"ShiftSystem: End request");
                    loco.Components.GearChangeRequest = null;
                }
            }
            else if (!loco.GearShiftInProgress)
            {
                PluginLoggerSingleton.Instance.Info($"ShiftSystem: Restoring throttle to {request.RestoreThrottle.Value}");
                loco.Throttle = request.RestoreThrottle.Value;
                loco.Components.GearChangeRequest = null;
            }
        }
    }

    public abstract class BaseSystem : DASystem
    {
        public bool Enabled { get; set; }

        public abstract void OnUpdate();
    }
}