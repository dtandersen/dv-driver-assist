using System;
using System.Collections.Generic;

namespace DriverAssist
{
    public interface DASystem
    {
        bool Enabled { get; set; }

        void OnUpdate();
    }

    public abstract class BaseSystem : DASystem
    {
        private bool enabled;
        public bool Enabled { get { return enabled; } set { logger.Info($"enabled={enabled}"); enabled = value; } }

        public abstract void OnUpdate();

        protected Logger logger;

        public BaseSystem()
        {
            logger = LogFactory.GetLogger(this.GetType().Name);
            Enabled = true;
            // logger.Info($"BaseSystem Enabled={Enabled}");
        }
    }

    public class SystemManager
    {
        private readonly List<DASystem> systems;
        protected Logger logger;

        public SystemManager()
        {
            logger = LogFactory.GetLogger(this.GetType().Name);
            systems = new();
        }

        public void AddSystem(DASystem system)
        {
            systems.Add(system);
        }

        public void Update()
        {
            foreach (DASystem system in systems)
            {
                // system.Enabled = true;
                // logger.Info($"enabled={system.Enabled}");
                if (system.Enabled) system.OnUpdate();
            }
        }
    }
}
