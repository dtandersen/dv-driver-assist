using System.Collections.Generic;

namespace DriverAssist.ECS
{
    public interface System
    {
        bool Enabled { get; set; }

        void OnUpdate();
    }

    public abstract class BaseSystem : System
    {
        public bool Enabled { get; set; }

        public abstract void OnUpdate();

        protected Logger logger;

        public BaseSystem()
        {
            logger = LogFactory.GetLogger(this.GetType().Name);
            Enabled = true;
        }
    }

    public class SystemManager
    {
        private readonly List<System> systems;
        protected Logger logger;

        public SystemManager()
        {
            logger = LogFactory.GetLogger(this.GetType().Name);
            systems = new();
        }

        public void AddSystem(System system)
        {
            systems.Add(system);
        }

        public void Update()
        {
            foreach (System system in systems)
            {
                if (system.Enabled) system.OnUpdate();
            }
        }
    }
}
