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

        public List<TaskWrapper> Tasks
        {
            get
            {
                List<TaskWrapper> tasks = new();
                foreach (Task task in job.tasks)
                {
                    tasks.Add(new DVTaskWrapper(task));
                }
                return tasks;
            }
        }

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

        int TaskWrapper.Type { get { return (int)task.InstanceTaskType; } }

        public bool IsSingular { get { return task.InstanceTaskType == TaskType.Transport || task.InstanceTaskType == TaskType.Warehouse; } }

        public bool IsParallel { get { return task.InstanceTaskType == TaskType.Parallel; } }

        public bool IsSequential { get { return task.InstanceTaskType == TaskType.Sequential; } }

        public List<TaskWrapper> Tasks
        {
            get
            {
                List<TaskWrapper> tasks = new();
                foreach (Task task in task.GetTaskData().nestedTasks)
                {
                    tasks.Add(new DVTaskWrapper(task));
                }
                return tasks;
            }
        }

        public DVTaskWrapper(Task task)
        {
            this.task = task;
        }
    }
}
