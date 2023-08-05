using System;

namespace DriverAssist.ECS
{
    public class ControlsChangedSystem : BaseSystem
    {
        private readonly EntityManager entityManager;

        public ControlsChangedSystem(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public override void OnUpdate()
        {
            if (entityManager.Loco == null) return;
            if (entityManager.Loco.Components.LastControls == null) return;

            // logger.Info("OnUpdate");

            LastControls lastControls = entityManager.Loco.Components.LastControls.Value;

            if (
                Changed(entityManager.Loco.IndBrake, lastControls.IndBrake, 1f / 11f) ||
                Changed(entityManager.Loco.TrainBrake, lastControls.TrainBrake, 1f / 11f)
                )
            {
                entityManager.Loco.Components.ControlsChanged = true;
                entityManager.Loco.Components.CruiseControl = null;
                logger.Info($"Brakes applied");
            }
            else
            {
                entityManager.Loco.Components.ControlsChanged = null;
            }
        }

        private bool Changed(float v1, float v2, float amount)
        {
            return Math.Abs(v1 - v2) > amount;
        }
    }
}
