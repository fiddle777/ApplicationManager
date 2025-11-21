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
        // REIKALAVIMAS: Naudojamas statinis konstruktorius (1 t.)
        // Statiniame konstruktoriuje nustatomos numatytos konfigūracijos reikšmės
        // (StaleDaysThreshold, UpcomingInterviewDays) vieną kartą programos starte.
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
