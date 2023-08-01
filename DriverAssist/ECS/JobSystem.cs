using System;
using System.Collections.Generic;
using DriverAssist.Implementation;
using DV.Logic.Job;

namespace DriverAssist.ECS
{
    public delegate void UpdateTask(JobRow job);
    public delegate void RemoveTask(string id);

    public struct JobTask
    {
        public JobWrapper Job;
        public List<TaskWrapper> Tasks;
    }

    public class JobSystem : BaseSystem
    {
        public Dictionary<string, JobTask> Jobs2 = new();

        public UpdateTask? UpdateTask = delegate { };
        public RemoveTask? RemoveTask = delegate { };

        public override void OnUpdate()
        {
            List<JobTask> updates = new();
            foreach (JobTask jobTask in Jobs2.Values)
            {
                if (IsAllComplete(jobTask.Tasks))
                {
                    JobWrapper job = jobTask.Job;
                    List<TaskWrapper> tasks = NextTasks(job);
                    logger.Info($"{job.ID}: Tasks completed");
                    UpdateTask?.Invoke(MakeJob(job, tasks));

                    updates.Add(new JobTask()
                    {
                        Job = job,
                        Tasks = tasks
                    });

                }
            }

            foreach (JobTask jobTask in updates)
            {
                Jobs2.Remove(jobTask.Job.ID);
                Jobs2[jobTask.Job.ID] = jobTask;
            }
        }

        private bool IsAllComplete(List<TaskWrapper> tasks)
        {
            foreach (TaskWrapper task in tasks)
            {
                if (!task.IsComplete) return false;
            }
            return true;
        }

        public JobRow MakeJob(JobWrapper job, List<TaskWrapper> tasks)
        {
            List<TaskRow> taskrows = new();
            foreach (TaskWrapper task in tasks)
            {
                taskrows.Add(new TaskRow()
                {
                    Origin = task.Source,
                    Destination = task.Destination
                });
            }
            return new JobRow()
            {
                ID = job.ID,
                Tasks = taskrows
            };
        }

        public void AddJob(JobWrapper job)
        {
            logger.Info($"AddJob {job.ID}");
            // TaskWrapper? task = job.GetNextTask();
            List<TaskWrapper> tasks = NextTasks(job);
            Jobs2[job.ID] = new JobTask()
            {
                Job = job,
                Tasks = tasks
            };
            UpdateTask?.Invoke(MakeJob(job, tasks));
        }

        public List<TaskWrapper> NextTasks(JobWrapper job)
        {
            logger.Info($"NextTasks {job.ID}");
            return NextTasks(job.Tasks, false);
        }

        public List<TaskWrapper> NextTasks(List<TaskWrapper> tasks, bool parentIsParallel)
        {
            logger.Info($"NextTasks {string.Join(",", tasks)} {parentIsParallel}");
            if (parentIsParallel && !IsAllComplete(tasks))
            {
                logger.Info($"Parallel parent");
                return tasks;
            }
            foreach (TaskWrapper task in tasks)
            {
                logger.Info($"evaluating {task}");
                if (task.IsComplete)
                {
                    logger.Info($"{task} is complete");
                    continue;
                }
                else if (!task.IsComplete && task.IsSingular)
                {
                    logger.Info($"Selected {task}");
                    return new List<TaskWrapper>() { task };
                }
                else if (task.IsComplete && task.IsSingular)
                {
                    continue;
                }

                List<TaskWrapper> nextTasks = NextTasks(task.Tasks, task.IsParallel);
                if (IsAllComplete(nextTasks))
                {
                    continue;
                }
                return nextTasks;
            }

            return new();
        }

        public void RemoveJob(string id)
        {
            logger.Info($"RemoveJob {id}");
            Jobs2.Remove(id);
            RemoveTask?.Invoke(id);
        }
    }
}
