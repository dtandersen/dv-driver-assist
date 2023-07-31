using System;
using System.Collections.Generic;
using DriverAssist.Implementation;
using DV.Logic.Job;

namespace DriverAssist.ECS
{
    public delegate void UpdateTask(TaskRow job);
    public delegate void RemoveTask(string id);

    public struct JobTask
    {
        public JobWrapper Job;
        public TaskWrapper? Task;
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
                TaskWrapper? task = jobTask.Task;
                if (task == null)
                {
                    // logger.Info($"Task is null");
                    continue;
                }

                // logger.Info($"{task.IsComplete}");
                if (task != null && task.IsComplete)
                {
                    JobWrapper job = jobTask.Job;
                    logger.Info($"{job.ID}: Task completed");
                    TaskWrapper? nextTask = job.GetNextTask();
                    if (nextTask == null)
                    {
                        // logger.Info($"nextTask is null");
                        updates.Add(new JobTask()
                        {
                            Job = job,
                            Task = null
                        });
                        continue;
                    }
                    UpdateTask?.Invoke(new TaskRow()
                    {
                        ID = job.ID,
                        Origin = nextTask?.Source ?? "",
                        Destination = nextTask?.Destination ?? ""
                    });

                    //utested
                    updates.Add(new JobTask()
                    {
                        Job = job,
                        Task = nextTask
                    });

                }
            }

            foreach (JobTask jobTask in updates)
            {
                Jobs2.Remove(jobTask.Job.ID);
                Jobs2[jobTask.Job.ID] = jobTask;
            }
        }

        public void AddJob(JobWrapper job)
        {
            logger.Info($"AddJob {job.ID}");
            TaskWrapper? task = job.GetNextTask();
            Jobs2[job.ID] = new JobTask()
            {
                Job = job,
                Task = task
            };
            UpdateTask?.Invoke(new TaskRow()
            {
                ID = job.ID,
                Origin = string.Copy(task?.Source ?? ""),
                Destination = string.Copy(task?.Destination ?? "")
            });
        }

        public void RemoveJob(string id)
        {
            logger.Info($"RemoveJob {id}");
            Jobs2.Remove(id);
            RemoveTask?.Invoke(id);
        }
    }
}
