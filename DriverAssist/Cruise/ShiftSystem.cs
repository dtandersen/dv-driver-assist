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
                loco.Throttle = 0;
            }
            else if (loco.Gear != requestedGear)
            {
                loco.Gear = requestedGear;
                if (!request.RestoreThrottle.HasValue)
                {
                    loco.Components.GearChangeRequest = null;
                }
            }
            else if (!loco.GearShiftInProgress)
            {
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