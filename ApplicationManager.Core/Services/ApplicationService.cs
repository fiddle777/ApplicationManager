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
            return _repo.GetAll().Where(a => IsStale(a, today, thresholdDays));
        }
        private static bool IsStale(Application app, DateTime today, int thresholdDays)
        {
            var lastContact = app.LastContactDate?.Date;
            return app.Status switch
            {
                ApplicationStatus.PendingReply
                    when lastContact.HasValue && (today - lastContact.Value).TotalDays >= thresholdDays
                    => true,
                _ => false
            };
        }
        public string? GetAlert(Application app)
        {
            var today = DateTime.UtcNow.Date;
            var lastContact = app.LastContactDate?.Date;
            var InterviewDate = app.InterviewDate?.Date;
            var staleThreshold = Config.GetStaleDaysThreshold();
            var upcomingDays = Config.UpcomingInterviewDays;

            return app.Status switch
            {
                ApplicationStatus.PendingReply
                    when lastContact.HasValue && (today - lastContact.Value).TotalDays >= staleThreshold
                => "Aplikacija be atsakymo(dienos): " + staleThreshold,

                ApplicationStatus.InterviewScheduled
                    when InterviewDate.HasValue &&
                    (InterviewDate.Value - today).TotalDays >= 0 &&
                    (InterviewDate.Value - today).TotalDays <= upcomingDays
                => "Artėjantis pokalbis: " + InterviewDate,

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
    }
}
