using System;
using System.Collections.Generic;
using System.Net;
using DriverAssist.Implementation;
using DriverAssist.Test;
using Xunit;
using Xunit.Abstractions;

namespace DriverAssist.ECS
{
    public class JobSystemTest
    {
        private readonly JobSystem system;
        const int TRANSPORT = 0;
        const int WAREHOUSE = 1;
        const int SEQUENTIAL = 2;
        const int PARALLEL = 3;
        protected Logger logger;
        FakeJobView view;


        public JobSystemTest(ITestOutputHelper output)
        {
            XunitLogger.Init(output);
            logger = LogFactory.GetLogger(this.GetType().Name);

            system = new JobSystem();
            view = new FakeJobView();
            system.JobUpdated += view.OnAddJob;
            system.JobRemoved += view.OnRemoveJob;
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
                Tasks = new List<TaskWrapper>{new FakeTask()
                {
                    Source = "SM-A6I",
                    Destination = "SM-A7L",
                }}
            };
            FakeJobView view = new FakeJobView();
            system.JobUpdated += view.OnAddJob;
            system.JobRemoved += view.OnRemoveJob;
            system.AddJob(job);

            // Assert.Equal(system.Jobs2["SM-SU-33"].Job, job);
            Assert.Equal(new List<JobRow>(view.Rows.Values), new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A6I",
                            Destination = "SM-A7L"
                    }}
                }
            });

            system.RemoveJob(job.ID);
            Assert.Empty(view.Rows);
        }

        [Fact]
        public void JobNext()
        {
            FakeJob job = new FakeJob()
            {
                ID = "SM-SU-33"
            };
            FakeTask task1 = job.AddTask(new FakeTask()
            {
                Type = WAREHOUSE,
                Source = "SM-A6I",
                Destination = "SM-A7L",
            });
            FakeTask task2 = job.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "SM-A7L",
                Destination = "SM-B50",
            });

            system.AddJob(job);

            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A6I",
                            Destination = "SM-A7L",
                            Complete=false
                    }}
                }
            }, new List<JobRow>(view.Rows.Values));

            task1.IsComplete = true;
            system.OnUpdate();

            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-B50",
                            Complete=false
                    }}
                }
            }, new List<JobRow>(view.Rows.Values));

            system.RemoveJob(job.ID);
            Assert.Empty(view.Rows);
        }

        /// The job has two tasks.
        /// When the first task is completed,
        /// Then the view should update.
        [Fact]
        public void FirstJobUpdates()
        {
            FakeJob job = new FakeJob()
            {
                ID = "MF-SU-53"
            };
            FakeTask parallelTask = job.AddTask(new FakeTask()
            {
                Type = PARALLEL,
            });
            FakeTask task1 = parallelTask.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "MF-C1L",
                Destination = "MF-C2S",
            });

            parallelTask.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "MF-C1L",
                Destination = "MF-B1S",
            });

            system.AddJob(job);
            task1.IsComplete = true;
            system.OnUpdate();

            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "MF-SU-53",
                    Tasks = {
                        new TaskRow() {
                            Origin = "MF-C1L",
                            Destination = "MF-C2S",
                            Complete=true
                        },
                        new TaskRow() {
                            Origin = "MF-C1L",
                            Destination = "MF-B1S",
                            Complete=false
                        }
                    }
                }
            }, new List<JobRow>(view.Rows.Values));
        }

        /// Job has multiple parallel tasks.
        /// Print them all.
        [Fact]
        public void ShowsAllParallelTasks()
        {
            // 1
            FakeTask parent = new FakeTask()
            {
                Type = SEQUENTIAL,
            };
            // 1.1
            parent.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "SM-A6I",
                Destination = "SM-A7L",
                IsComplete = true,
            });
            // 1.2
            FakeTask task12 = parent.AddTask(new FakeTask()
            {
                Type = PARALLEL,
                Source = "SM-A6I",
                Destination = "SM-A7L",
                IsComplete = true
            });
            // 1.2.1
            task12.AddTask(new FakeTask()
            {
                Type = WAREHOUSE,
                Destination = "SM-A7L",
                IsComplete = true
            });
            // 1.3
            FakeTask task13 = parent.AddTask(new FakeTask()
            {
                Type = PARALLEL,
                IsComplete = false
            });
            // 1.3.1
            task13.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "SM-A7L",
                Destination = "SM-B7S",
            });
            // 1.3.2
            task13.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "SM-A7L",
                Destination = "SM-A5S",
            });
            // 1.3.3
            task13.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "SM-A7L",
                Destination = "SM-A3S",
            });

            FakeJob job = new FakeJob()
            {
                ID = "SM-SU-33",
                Origin = "",
                Destination = "",
                Tasks = { parent }
            };
            FakeJobView view = new FakeJobView();
            system.JobUpdated += view.OnAddJob;
            // system.RemoveTask += view.OnRemoveJob;
            system.AddJob(job);
            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-B7S"
                        },
                         new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-A5S"
                        },
                         new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-A3S"
                        }
                    }
                }
            }, new List<JobRow>(view.Rows.Values));

            // job.Tasks = { fakeTask2};
            // fakeTask1.IsComplete = true;
            system.OnUpdate();
            // Assert.Equal(system.Jobs["SM-SU-33"], job);
            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-B7S"
                        },
                         new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-A5S"
                        },
                         new TaskRow() {
                            Origin = "SM-A7L",
                            Destination = "SM-A3S"
                        }
                    }
                }
            }, new List<JobRow>(view.Rows.Values));

            // system.RemoveJob(job.ID);
            // Assert.Empty(view.Rows);
        }

        /// Job has multiple parallel tasks.
        /// Print them all.
        [Fact]
        public void KeepWhenJobIsComplete()
        {
            FakeJob job = new FakeJob()
            {
                ID = "SM-SU-33",
                Origin = "",
                Destination = ""
            };
            FakeTask task = job.AddTask(new FakeTask()
            {
                Type = TRANSPORT,
                Source = "SM-A6I",
                Destination = "SM-A7L",
                IsComplete = false,
            });
            FakeJobView view = new FakeJobView();
            system.JobUpdated += view.OnAddJob;
            system.JobRemoved += view.OnRemoveJob;
            system.AddJob(job);
            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A6I",
                            Destination = "SM-A7L",
                            Complete=false
                        }
                    }
                }
            }, new List<JobRow>(view.Rows.Values));

            // job.Tasks = { fakeTask2};
            task.IsComplete = true;
            system.OnUpdate();
            // Assert.Equal(system.Jobs["SM-SU-33"], job);
            Assert.Equal(new List<JobRow> {
                new JobRow() {
                    ID = "SM-SU-33",
                    Tasks = {
                        new TaskRow() {
                            Origin = "SM-A6I",
                            Destination = "SM-A7L",
                            Complete=true
                        }
                    }
                }
            }, new List<JobRow>(view.Rows.Values));

            // system.RemoveJob(job.ID);
            // Assert.Empty(view.Rows);
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

        internal FakeTask AddTask(FakeTask task)
        {
            Tasks.Add(task);
            return task;
        }
    }

    class FakeTask : TaskWrapper
    {
        public int Type { get; set; } = 0;

        public string Source { get; set; } = "";

        public string Destination { get; set; } = "";

        public bool IsSingular { get { return this.Type == 0 || this.Type == 1; } }

        public bool IsComplete { get; set; } = false;

        public bool IsParallel { get { return this.Type == 3; } }

        public bool IsSequential { get { return this.Type == 2; } }

        public List<TaskWrapper> Tasks { get; set; } = new();

        public FakeTask AddTask(FakeTask task)
        {
            Tasks.Add(task);
            return task;
        }

        public override string ToString()
        {
            return $"FakeTask[Source={Source}, Destination={Destination}, IsComplete={IsComplete}]";
        }
    }

    class FakeJobView : JobView
    {
        public Dictionary<string, JobRow> Rows = new();

        public void OnAddJob(JobRow row)
        {
            Rows[row.ID] = row;
        }

        public void OnRemoveJob(string id)
        {
            Rows.Remove(id);
        }
    }
}
