namespace DriverAssist.ECS
{
    public class LastControlsSystem : BaseSystem
    {
        private readonly EntityManager entityManager;

        public LastControlsSystem(EntityManager entityManager)
        {
            this.entityManager = entityManager;
        }

        public override void OnUpdate()
        {
            if (entityManager.Loco == null) return;

            // logger.Info("OnUpdate");

            entityManager.Loco.Components.LastControls = new LastControls()
            {
                IndBrake = entityManager.Loco.IndBrake,
                TrainBrake = entityManager.Loco.TrainBrake
            };
        }
    }
}
