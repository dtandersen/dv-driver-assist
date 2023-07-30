namespace DriverAssist.ECS
{
    public class ShiftSystem : BaseSystem
    {
        private readonly LocoEntity loco;

        public ShiftSystem(LocoEntity loco)
        {
            this.loco = loco;
        }

        public override void OnUpdate()
        {
            if (!loco.Components.GearChangeRequest.HasValue) return;

            GearChangeRequest request = loco.Components.GearChangeRequest.Value;

            int requestedGear = request.RequestedGear;

            if (loco.Throttle == 0 && loco.Gear != requestedGear)
            {
                logger.Info($"Shifting to gear {requestedGear}");
                loco.Gear = requestedGear;
                if (!request.RestoreThrottle.HasValue)
                {
                    logger.Info($"Instant gear shift");
                    loco.Components.GearChangeRequest = null;
                }
            }
            else if (loco.Throttle > 0 && loco.Gear != requestedGear)
            {
                logger.Info($"Throttling down for gear change");
                loco.ZeroThrottle();
            }
            else if (loco.Rpm < 750 && !loco.GearShiftInProgress && request.RestoreThrottle != null)
            {
                logger.Info($"Restoring throttle to {request.RestoreThrottle.Value} rpm={loco.Rpm}");
                loco.Throttle = request.RestoreThrottle.Value;
                loco.Components.GearChangeRequest = null;
            }
            else
            {
                // logger.Info($"Waiting requestedGear={requestedGear} loco.Gear={loco.Gear} rpm={loco.Rpm}");
            }
        }
    }
}
