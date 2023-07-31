namespace DriverAssist
{
    public interface JobWrapper
    {
        string ID { get; }
        string Type { get; }
        string Origin { get; }
        string Destination { get; }
        TaskWrapper? GetNextTask();
    }

    public interface TaskWrapper
    {
        string Type { get; }
        string Source { get; }
        string Destination { get; }
        bool IsComplete { get; }
    }
}
