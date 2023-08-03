using System;
using System.Collections.Generic;
using DriverAssist.Implementation;

namespace DriverAssist.ECS
{
    public delegate void JobUpdated(JobRow job);
    public delegate void JobRemoved(string id);

    public class TaskBundle
    {
        // public Booklet Booklet;
        public List<TaskWrapper> Tasks;
        public bool IsComplete;

        // private readonly Logger logger;

        public TaskBundle(List<TaskWrapper> tasks)
        {
            // logger = LogFactory.GetLogger(GetType().Name);
            // Booklet = booklet;
            Tasks = tasks;

            // logger.Info("");
        }

        internal bool Refresh()
        {
            bool newIsComplete = IsTasksComplete(Tasks);
            bool statusChanged = newIsComplete != IsComplete;
            IsComplete = newIsComplete;

            return statusChanged;
        }

        private bool IsTasksComplete(List<TaskWrapper> tasks)
        {
            foreach (TaskWrapper task in tasks)
            {
                if (!task.IsComplete)
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return $"TaskBundle[Tasks=[{string.Join(",", Tasks)}]]";
        }
    }

    public class Booklet
    {
        private readonly List<TaskBundle> taskBundles;
        private readonly JobWrapper job;
        // private readonly Logger logger;

        private int current = 0;

        public string ID { get { return job.ID; } }

        public TaskBundle CurrentTasks { get; set; }

        public Booklet(JobWrapper job)
        {
            // logger = LogFactory.GetLogger(this.GetType().Name);
            taskBundles = new();
            this.job = job;
            Generate(job.Tasks, false);
            Refresh();
            CurrentTasks = taskBundles[current];
        }

        public bool Refresh()
        {
            bool refresh = false;

            foreach (TaskBundle taskBundle in taskBundles)
            {
                if (taskBundle.Refresh())
                {
                    refresh = true;
                }
            }

            if (refresh)
            {
                current = FirstIncomplete();
                CurrentTasks = taskBundles[current];
            }

            return refresh;
        }

        public override string ToString()
        {
            return $"Booklet[taskBundles=[{string.Join(",", taskBundles)}], currenttasks={this.CurrentTasks}]";
        }

        private void Generate(List<TaskWrapper> tasks, bool parentIsParallel)
        {
            if (parentIsParallel)
            {
                AddTasks(tasks);
                return;
            }

            foreach (TaskWrapper task in tasks)
            {
                if (task.IsSingular)
                {
                    AddTasks(new List<TaskWrapper>() { task });
                    continue;
                }
                else
                {
                    Generate(task.Tasks, task.IsParallel);
                }
            }
        }

        private int FirstIncomplete()
        {
            int i = 0;
            foreach (TaskBundle taskBundle in taskBundles)
            {
                if (!taskBundle.IsComplete)
                {
                    current = i;
                    return i;
                }
                i++;
            }
            current = taskBundles.Count - 1;
            return current;
        }

        private void AddTasks(List<TaskWrapper> tasks)
        {
            // logger.Info($"tasks[{taskBundles.Count}]={string.Join(",", tasks)}");
            taskBundles.Add(new TaskBundle(tasks));
        }
    }

    public struct JobTask
    {
        public Booklet Booklet { get; internal set; }
    }

    public class JobSystem : BaseSystem
    {
        public Dictionary<string, JobTask> Jobs = new();
        public JobUpdated? JobUpdated = delegate { };
        public JobRemoved? JobRemoved = delegate { };

        public override void OnUpdate()
        {
            foreach (JobTask jobTask in Jobs.Values)
            {
                Booklet booklet = jobTask.Booklet;
                if (booklet.Refresh())
                {
                    logger.Info($"Booklet refreshed {booklet}");
                    NotifyObservers(booklet);
                }
            }
        }

        public void NotifyObservers(Booklet booklet)
        {
            List<TaskRow> taskrows = booklet.CurrentTasks.Tasks.ConvertAll(new Converter<TaskWrapper, TaskRow>(TaskToTaskRow));
            var jobRow = new JobRow()
            {
                ID = booklet.ID,
                Tasks = taskrows
            };

            JobUpdated?.Invoke(jobRow);
        }

        public void AddJob(JobWrapper job)
        {
            logger.Info($"AddJob {job.ID}");
            Booklet booklet = new Booklet(job);
            Jobs[job.ID] = new JobTask()
            {
                Booklet = booklet
            };
            NotifyObservers(booklet);
        }

        public void RemoveJob(string id)
        {
            logger.Info($"RemoveJob {id}");
            Jobs.Remove(id);
            JobRemoved?.Invoke(id);
        }

        private TaskRow TaskToTaskRow(TaskWrapper task)
        {
            return new TaskRow()
            {
                Origin = task.Source,
                Destination = task.Destination,
                Complete = task.IsComplete
            };
        }
    }
}
