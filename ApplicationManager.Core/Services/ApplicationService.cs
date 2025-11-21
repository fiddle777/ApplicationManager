using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Models;
using ApplicationManager.Core.Repos;
using ApplicationManager.Core.Enums;
using ApplicationManager.Core;

namespace ApplicationManager.Core.Services
{
    public class ApplicationService
    {
        private readonly IApplicationRepo _repo;
        public ApplicationService(IApplicationRepo repo)
        {
            _repo = repo;
        }
        public IReadOnlyCollection<Application> GetAllSorted()
        {
            return _repo.GetAll().OrderBy(a => a).ToArray();
        }
        public IEnumerable<Application> GetStaleApplications()
        {
            var thresholdDays = Config.GetStaleDaysThreshold();
            var today = DateTime.UtcNow;
            var stale = _repo
                .GetAll()
                .Where(a => IsStale(a, today, thresholdDays))
                .ToArray();
            foreach (var app in stale)
            {
                MarkUrgentIfStale(app);
            }
            return stale;
        }
        // REIKALAVIMAS: Naudojate 'switch' su 'when' raktažodžiu (0.5 t.)
        // 'switch' su 'when' panaudotas papildomoms sąlygoms aprašyti (pvz. "stale"
        // aplikacijoms(aplikacijoms kuriose ilgą laiką negautas atsakymas) ir artėjantiems pokalbiams pagal statusą ir datas).
        private static bool IsStale(Application app, DateTime today, int thresholdDays)
        {
            var lastContact = app.LastContactDate?.Date;
            return app.Status switch
            {
                ApplicationStatus.LaukiamaAtsakymo
                    when lastContact.HasValue && (today - lastContact.Value).TotalDays >= thresholdDays
                    => true,
                _ => false
            };
        }
        public string? GetAlert(Application app)
        {
            var today = DateTime.UtcNow.Date;
            var lastContact = app.LastContactDate?.Date;
            var interviewDate = app.InterviewDate?.Date;
            var staleThreshold = Config.GetStaleDaysThreshold();
            var upcomingDays = Config.UpcomingInterviewDays;

            return app.Status switch
            {
                ApplicationStatus.LaukiamaAtsakymo
                    when lastContact.HasValue && (today - lastContact.Value).TotalDays >= staleThreshold
                => $"Aplikacija be atsakymo: {(today - lastContact.Value).TotalDays:F0} dienų.",

                ApplicationStatus.PokalbisSuplanuotas
                    when interviewDate.HasValue &&
                    (interviewDate.Value - today).TotalDays >= 0 &&
                    (interviewDate.Value - today).TotalDays <= upcomingDays
                => $"Artėjantis pokalbis: {interviewDate:yyyy-MM-dd}.",

                _ => null
            };
        }
        // REIKALAVIMAS: Naudojate 'Range' tipą (0.5 t.)
        // Naudojamas indeksatorius su Range ([^count..]) grąžinti paskutinius N įvykių
        // elementų iš masyvo.

        // REIKALAVIMAS: Naudojami numatyti ir vardiniai argumentai (0.5 t.)
        // Parametras 'count' turi numatytą reikšmę (5) ir kviečiamas naudojant
        // vardinį argumentą, pvz. GetLastEvents(app, count: 3).
        public ApplicationEvent[] GetLastEvents(Application app, int count = 5) {
            // REIKALAVIMAS: Naudojami delegatai arba lambda funkcijos (1.5 t.)
            // LINQ metodams (OrderBy, Where ir pan.) perduodamos lambda išraiškos
            // (pvz. a => a, e => e.Timestamp), kurios yra delegatų realizacija.
            var ordered = app.Events.OrderBy(e => e.Timestamp).ToArray();
            if(ordered.Length <= count)
            {
                return ordered;
            }
            return ordered[^count..];
        }
        // REIKALAIVMAS: Naudojamas raktažodis 'params' (0.5 t.)
        // Leidžia perduoti kelis Application parametrus vienu iškvietimu,
        // pvz. manager.AddApplication(app1, app2).
        public void AddApplication(params Application[] applications)
        {
            foreach (var app in applications)
            {
                _repo.Add(app);
            }
        }
        public bool TryParseMonthlyWage(string? input, out int? wage)
        {
            if(string.IsNullOrWhiteSpace(input))
            {
                wage = null;
                return true;
            }
            if(int.TryParse(input, out int parsedWage) && parsedWage >= 0)
            {
                wage = parsedWage;
                return true;
            }
            wage = null;
            return false;
        }
        // REIKALAVIMAS: Naudojamas operatorius 'is' (0.5 t.)
        // 'obj is Application app' panaudota tipui patikrinti ir iš karto išskleisti
        // kintamąjį 'app' tolimesniam naudojimui.
        // REQUIREMENT: Naudojamas šablonų atitikimas(1 t.)
        // 'obj is Application app' naudoja pattern matching, kad patikrintų tipą
        // ir iš karto sukurtų kintamąjį 'app' tolimesniam naudojimui.
        public string DescribeObject(object? obj)
        {
            if(obj is Application app)
            {
                var (company, position, status) = app;
                return $"{company} - {position} ({status})";
            }
            return obj?.ToString() ?? "null";
        }
        public void UpdateApplication(Application application)
        {
            if (application is null)
                throw new ArgumentNullException(nameof(application));

            _repo.Update(application);
        }
        public void MarkUrgentIfStale(Application app)
        {
            var today = DateTime.UtcNow.Date;
            var threshold = Config.GetStaleDaysThreshold();
            if (IsStale(app, today, threshold))
            {
                app.Flags |= ApplicationFlags.Urgent;
            }
            bool isUrgent = (app.Flags & ApplicationFlags.Urgent) != 0;
        }
    }
}
