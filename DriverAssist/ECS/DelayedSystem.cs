namespace DriverAssist.ECS
{
    public class DelayedSystem : BaseSystem
    {
        private readonly System system;
        private readonly float interval;
        private readonly float deltaTime;
        private float cooldown;

        public DelayedSystem(System system, float interval, float deltaTime)
        {
            this.system = system;
            this.interval = interval;
            this.deltaTime = deltaTime;
        }

        public override void OnUpdate()
        {
            cooldown -= deltaTime;
            if (cooldown < 0)
            {
                system.OnUpdate();
                cooldown = interval;
            }
        }
    }
}
