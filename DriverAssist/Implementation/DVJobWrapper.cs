using System.Collections.Generic;
using DV.Logic.Job;
using DV.Utils;

namespace DriverAssist.Implementation
{
    class DVJobWrapper : JobWrapper
    {
        private readonly Job job;

        public DVJobWrapper(Job job)
        {
            this.job = job;
        }

        public string ID { get { return job.ID; } }

        public string Type { get { return job.jobType.ToString(); } }

        public string Origin { get { return job.chainData.chainOriginYardId; } }

        public string Destination { get { return job.chainData.chainDestinationYardId; } }

        public TaskWrapper? GetNextTask()
        {
            return GetNextTask(job.tasks);
        }

        public TaskWrapper? GetNextTask(List<Task> tasks)
        {
            foreach (Task task in tasks)
            {
                if (task.state != 0)
                {
                    switch (task.InstanceTaskType)
                    {
                        case TaskType.Transport:
                        case TaskType.Warehouse:
                            return new DVTaskWrapper(task);
                        case TaskType.Sequential:
                        case TaskType.Parallel:
                            return GetNextTask(task.GetTaskData().nestedTasks);
                    }
                }
            }
            return null;
        }
    }

    class DVTaskWrapper : TaskWrapper
    {
        private readonly Task task;

        public string Type { get { return task.InstanceTaskType.ToString(); } }

        public string Source { get { return task.GetTaskData()?.startTrack?.ID?.FullDisplayID ?? ""; } }

        public string Destination { get { return task.GetTaskData()?.destinationTrack?.ID?.FullDisplayID ?? ""; } }

        public bool IsComplete { get { return task.GetTaskData().state == TaskState.Done; } }

        public DVTaskWrapper(Task task)
        {
            this.task = task;
        }
    }
}
