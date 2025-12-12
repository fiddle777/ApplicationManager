using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationManager.Core.Models;
using ApplicationManager.Core.Repos;
using ApplicationManager.Core.Enums;
using ApplicationManager.Core;
using ApplicationManager.Core.Collections;
using ApplicationManager.Core.Exceptions;

namespace ApplicationManager.Core.Services
{
    public class ApplicationService
    {
        private readonly IApplicationRepo _repo;

        // REIKALAVIMAS: Naudojate įvykius savo projekte (1 t.)
        // Įvykis iškviečiamas kai aplikacija pridedama / atnaujinama / pašalinama.
        public event EventHandler<ApplicationChangedEventArgs>? ApplicationChanged;

        public ApplicationService(IApplicationRepo repo)
        {
            _repo = repo;
        }
        public IReadOnlyCollection<Application> GetAllSorted()
        {
            return _repo.GetAll().OrderBy(a => a).ToArray();
        }

        public ApplicationCollection<Application> GetAllAsCollection()
        {
            return new ApplicationCollection<Application>(_repo.GetAll());
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

        public ApplicationEvent[] GetLastEvents(Application app, int count = 5) {
            var ordered = app.Events.OrderBy(e => e.Timestamp).ToArray();
            if(ordered.Length <= count)
            {
                return ordered;
            }
            return ordered[^count..];
        }
        public void AddApplication(params Application[] applications)
        {
            foreach (var app in applications)
            {
                _repo.Add(app);
                OnApplicationChanged("Added", app);
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
            OnApplicationChanged("Updated", application);
        }

        public void RemoveApplication(int id)
        {
            if (!_repo.Remove(id))
            {
                throw new ApplicationRepositoryException($"Aplikacija su id={id} nerasta.");
            }
            OnApplicationChanged("Removed", new Application { Id = id, CompanyName = string.Empty, PositionName = string.Empty });
        }

        private void OnApplicationChanged(string action, Application app)
        {
            ApplicationChanged?.Invoke(this, new ApplicationChangedEventArgs(action, app));
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
