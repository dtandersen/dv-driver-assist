using DriverAssist.Extension;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.Cruise
{
    public class SystemTest
    {
        private readonly SystemManager systemManager;
        private readonly FakeSystem system1;
        private readonly FakeSystem system2;

        public SystemTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);
            systemManager = new SystemManager();
            system1 = new FakeSystem();
            system2 = new FakeSystem();
        }

        [Fact]
        public void UpdatesSystems()
        {
            systemManager.AddSystem(system1);
            systemManager.AddSystem(system2);

            WhenSystemsRun();

            Assert.True(system1.Updated);
            Assert.True(system2.Updated);
        }

        [Fact]
        public void DoesntUpdateDisabledSystems()
        {
            systemManager.AddSystem(system1);
            systemManager.AddSystem(system2);
            system1.Enabled = false;
            system2.Enabled = false;

            WhenSystemsRun();

            Assert.False(system1.Updated);
            Assert.False(system2.Updated);
        }


        void WhenSystemsRun()
        {
            systemManager.Update();
        }
    }

    class FakeSystem : BaseSystem
    {
        public bool Updated;

        public override void OnUpdate()
        {
            Updated = true;
        }
    }
}
