using System.Collections.Generic;
using DriverAssist.Implementation;
using DriverAssist.Test;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.ECS
{
    public class JobSystemTest
    {
        private readonly JobSystem system;

        public JobSystemTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);
            system = new JobSystem();
        }

        /// <summary>
        /// The train is a DM3 
        /// and a gear change has been requested.
        /// Proceed.
        /// </summary>
        [Fact]
        public void UpdatesAcceleration()
        {
            FakeJob job = new FakeJob()
            {
                ID = "SM-SU-33",
                Origin = "",
                Destination = "",
                Tasks = new List<TaskWrapper>{new FakeTask()
                {
                    Source = "SM-A6I",
                    Destination = "SM-A7L",
                }}
            };
            FakeJobView view = new FakeJobView();
            system.UpdateTask += view.OnJobAccepted;
            system.RemoveTask += view.OnJobRemoved;
            system.AddJob(job);

            Assert.Equal(system.Jobs2["SM-SU-33"].Job, job);
            Assert.Equal(new List<TaskRow>(view.Rows.Values), new List<TaskRow> {
                new TaskRow() {
                    ID = "SM-SU-33",
                    Origin = "SM-A6I",
                    Destination = "SM-A7L"
                }
            });

            system.RemoveJob(job.ID);
            Assert.Empty(view.Rows);
        }

        [Fact]
        public void JobNext()
        {
            FakeTask fakeTask1 = new FakeTask()
            {
                Source = "SM-A6I",
                Destination = "SM-A7L",
                IsComplete = false
            };
            FakeTask fakeTask2 = new FakeTask()
            {
                Source = "SM-A7L",
                Destination = "SM-B50",
                IsComplete = false
            };

            FakeJob job = new FakeJob()
            {
                ID = "SM-SU-33",
                Origin = "",
                Destination = "",
                Tasks = { fakeTask1, fakeTask2 }
            };
            FakeJobView view = new FakeJobView();
            system.UpdateTask += view.OnJobAccepted;
            system.RemoveTask += view.OnJobRemoved;
            system.AddJob(job);

            // job.Tasks = { fakeTask2};
            fakeTask1.IsComplete = true;
            system.OnUpdate();

            // Assert.Equal(system.Jobs["SM-SU-33"], job);
            Assert.Equal(new List<TaskRow>(view.Rows.Values), new List<TaskRow> {
                new TaskRow() {
                    ID = "SM-SU-33",
                    Origin = "SM-A7L",
                    Destination = "SM-B50"
                }
            });

            system.RemoveJob(job.ID);
            Assert.Empty(view.Rows);
        }

        // private void WhenSystemUpdates()
        // {
        //     system.OnUpdate();
        // }
    }

    class FakeJob : JobWrapper
    {
        public string ID { get; set; } = "";

        public string Type { get; set; } = "";

        public string Origin { get; set; } = "";

        public string Destination { get; set; } = "";

        public List<TaskWrapper> Tasks { get; set; } = new();

        public FakeJob()
        {
        }

        public TaskWrapper? GetNextTask()
        {
            foreach (TaskWrapper task in Tasks)
            {
                if (!task.IsComplete) return task;
            }

            return Tasks[Tasks.Count - 1];
        }
    }

    class FakeTask : TaskWrapper
    {
        public string Type { get; set; } = "";

        public string Source { get; set; } = "";

        public string Destination { get; set; } = "";

        public bool IsComplete { get; set; } = false;
    }

    class FakeJobView : JobView
    {
        public Dictionary<string, TaskRow> Rows = new();

        public void OnJobAccepted(TaskRow row)
        {
            Rows[row.ID] = row;
        }

        public void OnJobRemoved(string id)
        {
            Rows.Remove(id);
        }
    }
}
