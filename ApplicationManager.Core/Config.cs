using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationManager.Core
{
    public static class Config
    {
        public static int? StaleDaysThreshold { get; private set; }
        public static int UpcomingInterviewDays { get; private set; }
        static Config()
        {
            StaleDaysThreshold = 21;
            UpcomingInterviewDays = 7;
        }
        public static int GetStaleDaysThreshold()
        {
            return StaleDaysThreshold ?? 21;
        }
    }
}
