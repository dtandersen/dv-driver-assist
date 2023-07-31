using System.Collections.Generic;

namespace DriverAssist
{
    public class JobManager
    {
        public readonly List<JobWrapper> Jobs = new();

        internal void Add(JobWrapper jobWrapper)
        {
            Jobs.Add(jobWrapper);
        }
    }
}
