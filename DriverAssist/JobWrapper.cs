using System.Collections.Generic;

namespace DriverAssist
{
    public interface JobWrapper
    {
        string ID { get; }
        string Type { get; }
        string Origin { get; }
        string Destination { get; }
        List<TaskWrapper> Tasks { get; }
    }

    public interface TaskWrapper
    {
        int Type { get; }
        string Source { get; }
        string Destination { get; }
        bool IsComplete { get; }
        bool IsSingular { get; }
        bool IsParallel { get; }
        bool IsSequential { get; }
        List<TaskWrapper> Tasks { get; }
    }
}
